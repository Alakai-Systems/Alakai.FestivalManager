using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IRegistrationRepository
{
    Task AddAsync(Registration registration, CancellationToken cancellationToken = default);
    Task<Registration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<Registration?> GetByEditionAndEmailAsync(Guid editionId, string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEditionAndEmailAsync(Guid editionId, string email, CancellationToken cancellationToken = default);
    void Update(Registration registration);
    void Delete(Registration registration);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
