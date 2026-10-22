using InternalRequests.Application.Interfaces;
using InternalRequests.Domain.Entities;
using InternalRequests.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InternalRequests.Infrastructure.Repositories;

public class DemandRepository : IDemandRepository
{
    private readonly AppDbContext _context;

    public DemandRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Demand demand)
    {
        await _context.Demands.AddAsync(demand);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Demand>> GetAllAsync()
    {
        return await _context.Demands
            .AsNoTracking()
            .OrderByDescending(d => d.RequestDate)
            .ToListAsync();
    }

    public async Task<Demand?> GetByIdAsync(Guid id)
    {
        return await _context.Demands
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);
    }
}
