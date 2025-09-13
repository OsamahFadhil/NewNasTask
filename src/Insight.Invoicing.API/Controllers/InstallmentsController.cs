using Insight.Invoicing.Application.DTOs;
using Insight.Invoicing.Application.Queries.Installments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Insight.Invoicing.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InstallmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InstallmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("contracts/{contractId}")]
    public async Task<ActionResult<object>> GetContractInstallments(
        int contractId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetContractInstallmentsQuery(contractId, pageNumber, pageSize);
        var (installments, totalCount) = await _mediator.Send(query);

        return Ok(new
        {
            Installments = installments,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpGet("overdue")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<object>> GetOverdueInstallments(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] int? minOverdueDays = null)
    {
        var query = new GetOverdueInstallmentsQuery(pageNumber, pageSize, minOverdueDays);
        var (installments, totalCount) = await _mediator.Send(query);

        return Ok(new
        {
            Installments = installments,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            MinOverdueDays = minOverdueDays
        });
    }
}

