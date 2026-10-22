using InternalRequests.Application.DTOs;
using InternalRequests.Application.Interfaces;
using InternalRequests.Domain.Entities;

namespace InternalRequests.Application.Services;

public class DemandService
{
    private readonly IDemandRepository _repository;

    public DemandService(IDemandRepository repository)
    {
        _repository = repository;
    }

    public async Task<DemandResponse> CreateAsync(
        CreateDemandRequest request)
    {
        if (request.RequestDate is null)
            throw new ArgumentException(
                "Data de solicitação é obrigatória.");

        if (request.Status is null)
            throw new ArgumentException(
                "Status é obrigatório.");

        var demand = new Demand(
            request.Title,
            request.Description,
            request.RequestDate.Value,
            request.Sector,
            request.Status.Value
        );

        await _repository.AddAsync(demand);

        return MapToResponse(demand);
    }

    public async Task<List<DemandResponse>> GetAllAsync()
    {
        var demands = await _repository.GetAllAsync();

        return demands.Select(MapToResponse).ToList();
    }

    public async Task<DemandResponse?> GetByIdAsync(Guid id)
    {
        var demand = await _repository.GetByIdAsync(id);

        return demand is null
            ? null
            : MapToResponse(demand);
    }

    private static DemandResponse MapToResponse(Demand demand)
    {
        return new DemandResponse
        {
            Id = demand.Id,
            Title = demand.Title,
            Description = demand.Description,
            RequestDate = demand.RequestDate,
            Sector = demand.Sector,
            Status = demand.Status,
            Priority = demand.Priority
        };
    }
}
