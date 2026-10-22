
using InternalRequests.Application.DTOs;
using InternalRequests.Domain.Entities;
using InternalRequests.Domain.Enums;

namespace InternalRequests.Application.Interfaces;

public interface IDemandRepository
{
    Task AddAsync(Demand demand);

    Task<List<Demand>> GetAllAsync();

    Task<Demand?> GetByIdAsync(Guid id);

    Task UpdateAsync(Demand demand);

    Task<PagedResult<Demand>> GetPagedAsync(
        int page,
        int pageSize,
        DemandStatus? status,
        DemandPriority? priority,
        string? sector
    );
}
