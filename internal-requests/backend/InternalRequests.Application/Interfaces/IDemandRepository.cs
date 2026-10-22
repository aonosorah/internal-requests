
using InternalRequests.Domain.Entities;

namespace InternalRequests.Application.Interfaces;

public interface IDemandRepository
{
    Task AddAsync(Demand demand);

    Task<List<Demand>> GetAllAsync();

    Task<Demand?> GetByIdAsync(Guid id);

    Task UpdateAsync(Demand demand);
}
