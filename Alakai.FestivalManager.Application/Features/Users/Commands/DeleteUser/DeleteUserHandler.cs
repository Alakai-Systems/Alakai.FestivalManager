namespace Alakai.FestivalManager.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserHandler
{
    private readonly IUserRepository _userRepository;

    public DeleteUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken = default)
    {
        User? user = await _userRepository.GetByIdAsync(command.Id, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"User with id '{command.Id}' was not found.");
        }

        user.IsActive = false;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
