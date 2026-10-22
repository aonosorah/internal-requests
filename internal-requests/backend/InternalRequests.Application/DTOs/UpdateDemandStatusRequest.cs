using InternalRequests.Domain.Enums;

namespace InternalRequests.Application.DTOs;

public class UpdateDemandStatusRequest
{
    public DemandStatus? Status { get; set; }
}
