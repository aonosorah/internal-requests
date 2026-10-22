using InternalRequests.Domain.Enums;

namespace InternalRequests.Domain.Entities;

public class Demand
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime RequestDate { get; private set; }
    public string Sector { get; private set; } = string.Empty;
    public DemandStatus Status { get; private set; }
    public DemandPriority Priority { get; private set; }

    private Demand() { }

    public Demand(
        string title,
        string description,
        DateTime requestDate,
        string sector,
        DemandStatus status)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Título é obrigatório.");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Descrição é obrigatória.");

        if (string.IsNullOrWhiteSpace(sector))
            throw new ArgumentException("Setor é obrigatório.");

        if (requestDate == default)
            throw new ArgumentException("Data de solicitação é obrigatória.");

        if (!Enum.IsDefined(status))
            throw new ArgumentException("Status inválido.");

        Title = title.Trim();
        Description = description.Trim();
        RequestDate = requestDate;
        Sector = sector.Trim();
        Status = status;

        Priority = CalculatePriority(Description);
    }

    private static DemandPriority CalculatePriority(string description)
    {
        return description.Contains(
            "urgente", StringComparison.OrdinalIgnoreCase)
            || description.Contains(
                "erro", StringComparison.OrdinalIgnoreCase)
            ? DemandPriority.High
            : DemandPriority.Normal;
    }
}
