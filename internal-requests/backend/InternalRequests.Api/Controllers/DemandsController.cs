using InternalRequests.Application.DTOs;
using InternalRequests.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace InternalRequests.Api.Controllers;

[ApiController]
[Route("api/demands")]
public class DemandsController : ControllerBase
{
    private readonly DemandService _service;

    public DemandsController(DemandService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(DemandResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DemandResponse>> Create(
        [FromBody] CreateDemandRequest request)
    {
        try
        {
            var demand = await _service.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = demand.Id },
                demand
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<DemandResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DemandResponse>>> GetAll()
    {
        var demands = await _service.GetAllAsync();
        return Ok(demands);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DemandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DemandResponse>> GetById(Guid id)
    {
        var demand = await _service.GetByIdAsync(id);

        if (demand is null)
            return NotFound(new { message = "Demanda não encontrada." });

        return Ok(demand);
    }
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(DemandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateDemandStatusRequest request)
    {
        try
        {
            var demand = await _service.UpdateStatusAsync(id, request);

            if (demand is null)
                return NotFound(new { message = "Demanda não encontrada." });

            return Ok(demand);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
