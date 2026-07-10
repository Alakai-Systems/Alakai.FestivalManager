using Alakai.FestivalManager.Application.Contracts.Repositories;
using Alakai.FestivalManager.Application.Features.CompetitionEntries.Commands.CreateCompetitionEntry;
using Alakai.FestivalManager.Application.Features.CompetitionEntries.Contracts.DTOs;
using AutoMapper;

namespace Alakai.FestivalManager.Tests.Unit.Application.Features.CompetitionEntries;

public class CreateCompetitionEntryHandlerTests
{
    private readonly Mock<ICompetitionEntryRepository> _entryRepo = new();
    private readonly Mock<ICompetitionRepository> _competitionRepo = new();
    private readonly Mock<ICompetitionCapacityRepository> _capacityRepo = new();
    private readonly Mock<IRegistrationRepository> _regRepo = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly CreateCompetitionEntryHandler _sut;

    private readonly Competition _competition = new() { RequiresPartner = false };
    private readonly Registration _registration = new() { Email = "jose@test.com" };
    private readonly CompetitionCapacity _capacity = new() { Capacity = 10, DanceRole = DanceRole.Leader };

    public CreateCompetitionEntryHandlerTests()
    {
        _sut = new CreateCompetitionEntryHandler(
            _entryRepo.Object, _competitionRepo.Object, _regRepo.Object, _mapper.Object, _capacityRepo.Object);

        _competitionRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(_competition);
        _regRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(_registration);
        _entryRepo.Setup(r => r.ExistsByCompetitionAndRegistrationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _entryRepo.Setup(r => r.CountActiveByCapacityIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(5);
        _mapper.Setup(m => m.Map<CompetitionEntry>(It.IsAny<object>())).Returns(new CompetitionEntry());
        _mapper.Setup(m => m.Map<CompetitionEntryDto>(It.IsAny<CompetitionEntry>())).Returns(new CompetitionEntryDto());
    }

    private CreateCompetitionEntryCommand BuildCommand(Guid? capacityId = null) => new()
    {
        CompetitionId = Guid.NewGuid(),
        RegistrationId = Guid.NewGuid(),
        CompetitionCapacityId = capacityId ?? Guid.NewGuid()
    };

    private void SetupCapacity(Guid capacityId, Guid? competitionId = null, int capacity = 10, int activeEntries = 0)
    {
        _capacity.Capacity = capacity;
        if (competitionId.HasValue) _capacity.CompetitionId = competitionId.Value;
        _capacityRepo.Setup(r => r.GetByIdAsync(capacityId, It.IsAny<CancellationToken>())).ReturnsAsync(_capacity);
        _entryRepo.Setup(r => r.CountActiveByCapacityIdAsync(capacityId, It.IsAny<CancellationToken>())).ReturnsAsync(activeEntries);
    }

    [Fact]
    public async Task HandleAsync_WhenCompetitionNotFound_ThrowsNotFoundException()
    {
        _competitionRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Competition?)null);

        Func<Task> act = () => _sut.HandleAsync(BuildCommand());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Competition*");
    }

    [Fact]
    public async Task HandleAsync_WhenRegistrationNotFound_ThrowsNotFoundException()
    {
        _regRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Registration?)null);

        Func<Task> act = () => _sut.HandleAsync(BuildCommand());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Registration*");
    }

    [Fact]
    public async Task HandleAsync_WhenAlreadyEntered_ThrowsBusinessRuleException()
    {
        _entryRepo.Setup(r => r.ExistsByCompetitionAndRegistrationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        Func<Task> act = () => _sut.HandleAsync(BuildCommand());

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*already entered*");
    }

    [Fact]
    public async Task HandleAsync_WhenCapacityNotFound_ThrowsNotFoundException()
    {
        _capacityRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((CompetitionCapacity?)null);

        Func<Task> act = () => _sut.HandleAsync(BuildCommand());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*capacity*");
    }

    [Fact]
    public async Task HandleAsync_WhenCapacityBelongsToDifferentCompetition_ThrowsBusinessRuleException()
    {
        Guid capacityId = Guid.NewGuid();
        CompetitionCapacity wrongCapacity = new() { CompetitionId = Guid.NewGuid(), Capacity = 10 };
        _capacityRepo.Setup(r => r.GetByIdAsync(capacityId, It.IsAny<CancellationToken>())).ReturnsAsync(wrongCapacity);

        CreateCompetitionEntryCommand command = BuildCommand(capacityId);

        Func<Task> act = () => _sut.HandleAsync(command);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*does not belong*");
    }

    [Fact]
    public async Task HandleAsync_WhenCapacityIsFull_ThrowsBusinessRuleException()
    {
        Guid capacityId = Guid.NewGuid();
        CreateCompetitionEntryCommand command = BuildCommand(capacityId);
        _capacity.CompetitionId = command.CompetitionId;
        SetupCapacity(capacityId, command.CompetitionId, capacity: 5, activeEntries: 5);

        Func<Task> act = () => _sut.HandleAsync(command);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*full*");
    }

    [Fact]
    public async Task HandleAsync_WhenCapacityIsUnlimited_AllowsEntry()
    {
        Guid capacityId = Guid.NewGuid();
        CreateCompetitionEntryCommand command = BuildCommand(capacityId);
        _capacity.CompetitionId = command.CompetitionId;
        SetupCapacity(capacityId, command.CompetitionId, capacity: 0, activeEntries: 999); // 0 = unlimited

        CompetitionEntryDto result = await _sut.HandleAsync(command);

        result.Should().NotBeNull();
        _entryRepo.Verify(r => r.AddAsync(It.IsAny<CompetitionEntry>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoPartnerAndCompetitionRequiresPartner_SetsWaitingPartnerStatus()
    {
        Guid capacityId = Guid.NewGuid();
        CreateCompetitionEntryCommand command = BuildCommand(capacityId);
        command.PartnerRegistrationId = null;
        _competition.RequiresPartner = true;
        _capacity.CompetitionId = command.CompetitionId;
        SetupCapacity(capacityId, command.CompetitionId);

        CompetitionEntry capturedEntry = new();
        _mapper.Setup(m => m.Map<CompetitionEntry>(It.IsAny<object>())).Returns(capturedEntry);

        await _sut.HandleAsync(command);

        capturedEntry.Status.Should().Be(CompetitionEntryStatus.WaitingPartner);
    }

    [Fact]
    public async Task HandleAsync_WhenPartnerProvided_SetsConfirmedStatus()
    {
        Guid capacityId = Guid.NewGuid();
        CreateCompetitionEntryCommand command = BuildCommand(capacityId);
        command.PartnerRegistrationId = Guid.NewGuid();
        _competition.RequiresPartner = true;
        _capacity.CompetitionId = command.CompetitionId;
        SetupCapacity(capacityId, command.CompetitionId);

        _regRepo.Setup(r => r.GetByIdAsync(command.PartnerRegistrationId.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Registration());

        CompetitionEntry capturedEntry = new();
        _mapper.Setup(m => m.Map<CompetitionEntry>(It.IsAny<object>())).Returns(capturedEntry);

        await _sut.HandleAsync(command);

        capturedEntry.Status.Should().Be(CompetitionEntryStatus.Confirmed);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_SavesEntry()
    {
        Guid capacityId = Guid.NewGuid();
        CreateCompetitionEntryCommand command = BuildCommand(capacityId);
        _capacity.CompetitionId = command.CompetitionId;
        SetupCapacity(capacityId, command.CompetitionId);

        await _sut.HandleAsync(command);

        _entryRepo.Verify(r => r.AddAsync(It.IsAny<CompetitionEntry>(), It.IsAny<CancellationToken>()), Times.Once);
        _entryRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}