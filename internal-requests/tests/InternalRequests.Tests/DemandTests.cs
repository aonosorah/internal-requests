
using InternalRequests.Domain.Entities;
using InternalRequests.Domain.Enums;

namespace InternalRequests.Tests;

public class DemandTests
{
    [Theory]
    [InlineData("Erro ao processar pagamento")]
    [InlineData("Solicitação URGENTE")]
    [InlineData("Falha com erro no sistema")]
    public void ShouldAssignHighPriority(string description)
    {
        var demand = CreateDemand(description);

        Assert.Equal(DemandPriority.High, demand.Priority);
    }

    [Theory]
    [InlineData("Solicitação de teclado")]
    [InlineData("Atualização de cadastro")]
    public void ShouldAssignNormalPriority(string description)
    {
        var demand = CreateDemand(description);

        Assert.Equal(DemandPriority.Normal, demand.Priority);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ShouldRejectEmptyTitle(string title)
    {
        Assert.Throws<ArgumentException>(() =>
            new Demand(
                title,
                "Descrição válida",
                DateTime.UtcNow,
                "TI",
                DemandStatus.Pending));
    }

    [Fact]
    public void ShouldRejectEmptyDescription()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateDemand(""));
    }

    [Fact]
    public void ShouldRejectEmptySector()
    {
        Assert.Throws<ArgumentException>(() =>
            new Demand(
                "Título válido",
                "Descrição válida",
                DateTime.UtcNow,
                "",
                DemandStatus.Pending));
    }

    [Fact]
    public void ShouldRejectInvalidStatus()
    {
        Assert.Throws<ArgumentException>(() =>
            new Demand(
                "Título válido",
                "Descrição válida",
                DateTime.UtcNow,
                "TI",
                (DemandStatus)999));
    }

    [Fact]
    public void ShouldRejectMissingRequestDate()
    {
        Assert.Throws<ArgumentException>(() =>
            new Demand(
                "Título válido",
                "Descrição válida",
                default,
                "TI",
                DemandStatus.Pending));
    }

    // TESTES DE ATUALIZAÇÃO DE STATUS

    [Fact]
    public void ShouldUpdateStatusFromPendingToInProgress()
    {
        var demand = CreateDemand("Solicitação de equipamento");

        demand.UpdateStatus(DemandStatus.InProgress);

        Assert.Equal(DemandStatus.InProgress, demand.Status);
    }

    [Fact]
    public void ShouldUpdateStatusFromInProgressToCompleted()
    {
        var demand = CreateDemand("Solicitação de equipamento");

        demand.UpdateStatus(DemandStatus.InProgress);
        demand.UpdateStatus(DemandStatus.Completed);

        Assert.Equal(DemandStatus.Completed, demand.Status);
    }

    [Fact]
    public void ShouldAllowUpdatingStatusToSameValue()
    {
        var demand = CreateDemand("Solicitação de equipamento");

        demand.UpdateStatus(DemandStatus.Pending);

        Assert.Equal(DemandStatus.Pending, demand.Status);
    }

    [Fact]
    public void ShouldRejectInvalidStatusOnUpdate()
    {
        var demand = CreateDemand("Solicitação de equipamento");

        Assert.Throws<ArgumentException>(() =>
            demand.UpdateStatus((DemandStatus)999));
    }

    [Fact]
    public void ShouldKeepPreviousStatusWhenUpdateIsInvalid()
    {
        var demand = CreateDemand("Solicitação de equipamento");

        Assert.Throws<ArgumentException>(() =>
            demand.UpdateStatus((DemandStatus)999));

        Assert.Equal(DemandStatus.Pending, demand.Status);
    }

    [Fact]
    public void ShouldPreserveDemandDataWhenUpdatingStatus()
    {
        var demand = CreateDemand("Erro urgente no sistema");

        var originalId = demand.Id;
        var originalTitle = demand.Title;
        var originalDescription = demand.Description;
        var originalSector = demand.Sector;
        var originalRequestDate = demand.RequestDate;
        var originalPriority = demand.Priority;

        demand.UpdateStatus(DemandStatus.Completed);

        Assert.Equal(DemandStatus.Completed, demand.Status);
        Assert.Equal(originalId, demand.Id);
        Assert.Equal(originalTitle, demand.Title);
        Assert.Equal(originalDescription, demand.Description);
        Assert.Equal(originalSector, demand.Sector);
        Assert.Equal(originalRequestDate, demand.RequestDate);
        Assert.Equal(originalPriority, demand.Priority);
        Assert.Equal(DemandPriority.High, demand.Priority);
    }

    private static Demand CreateDemand(string description)
    {
        return new Demand(
            "Demanda de teste",
            description,
            DateTime.UtcNow,
            "TI",
            DemandStatus.Pending);
    }
}
