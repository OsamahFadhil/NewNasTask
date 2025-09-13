using Insight.Invoicing.Application.Commands.Contracts;
using Insight.Invoicing.Application.DTOs;
using Insight.Invoicing.Application.Queries.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Insight.Invoicing.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContractsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ContractDto>> GetContract(int id)
    {
        var query = new GetContractQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound($"Contract with ID {id} not found");
        }

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "Tenant")]
    public async Task<ActionResult<object>> GetUserContracts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return BadRequest("Invalid user ID");
        }

        var query = new GetUserContractsQuery(userId, pageNumber, pageSize);
        var (contracts, totalCount) = await _mediator.Send(query);

        return Ok(new
        {
            Contracts = contracts,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpPost]
    [Authorize(Roles = "Tenant")]
    public async Task<ActionResult<ContractDto>> CreateContract([FromBody] CreateContractCommand command)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return BadRequest("Invalid user ID");
        }

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetContract), new { id = result.Id }, result);
    }

    [HttpPost("{id}/submit")]
    [Authorize(Roles = "Tenant")]
    public async Task<ActionResult> SubmitContract(int id)
    {
        var command = new SubmitContractCommand(id);
        var result = await _mediator.Send(command);

        if (!result)
        {
            return BadRequest("Failed to submit contract");
        }

        return Ok(new { Message = "Contract submitted successfully" });
    }

    [HttpGet("admin/pending")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<object>> GetPendingContracts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetPendingContractsQuery(pageNumber, pageSize);
        var (contracts, totalCount) = await _mediator.Send(query);

        return Ok(new
        {
            Contracts = contracts,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpPost("admin/{id}/approve")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult> ApproveContract(int id, [FromBody] ContractActionDto actionDto)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return BadRequest("Invalid user ID");
        }

        var command = new ApproveContractCommand(id, userId, actionDto.Comments);
        var result = await _mediator.Send(command);

        if (!result)
        {
            return BadRequest("Failed to approve contract");
        }

        return Ok(new { Message = "Contract approved successfully" });
    }

    [HttpPost("admin/{id}/reject")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult> RejectContract(int id, [FromBody] ContractActionDto actionDto)
    {
        if (string.IsNullOrWhiteSpace(actionDto.Comments))
        {
            return BadRequest("Rejection reason is required");
        }

        var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return BadRequest("Invalid user ID");
        }

        var command = new RejectContractCommand(id, userId, actionDto.Comments);
        var result = await _mediator.Send(command);

        if (!result)
        {
            return BadRequest("Failed to reject contract");
        }

        return Ok(new { Message = "Contract rejected successfully" });
    }
}

