using InternalRequests.Domain.Enums;

namespace InternalRequests.Application.DTOs;

public class DemandResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public string Sector { get; set; } = string.Empty;
    public DemandStatus Status { get; set; }
    public DemandPriority Priority { get; set; }
}
