using Insight.Invoicing.Application.Commands.PaymentReceipts;
using Insight.Invoicing.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Insight.Invoicing.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentReceiptsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentReceiptsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Tenant")]
    [Consumes("multipart/form-data")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ActionResult<PaymentReceiptDto>> UploadPaymentReceipt(
        [FromForm] int contractId,
        [FromForm] int installmentId,
        [FromForm] decimal amountPaid,
        [FromForm] DateTime paymentDate,
        [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required");
        }

        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return BadRequest("Invalid user ID");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileContent = memoryStream.ToArray();

        var command = new UploadPaymentReceiptCommand(
            contractId,
            installmentId,
            amountPaid,
            paymentDate,
            fileContent,
            file.FileName,
            file.ContentType,
            userId);

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPaymentReceipt), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public Task<ActionResult<PaymentReceiptDto>> GetPaymentReceipt(int id)
    {
        return Task.FromResult<ActionResult<PaymentReceiptDto>>(NotFound("Not implemented yet"));
    }

    [HttpPost("{id}/validate")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult> ValidatePaymentReceipt(int id, [FromBody] ValidatePaymentReceiptDto validationDto)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return BadRequest("Invalid user ID");
        }

        var command = new ValidatePaymentReceiptCommand(
            id,
            validationDto.IsApproved,
            userId,
            validationDto.Comments);

        var result = await _mediator.Send(command);

        if (!result)
        {
            return BadRequest("Failed to validate payment receipt");
        }

        var action = validationDto.IsApproved ? "approved" : "rejected";
        return Ok(new { Message = $"Payment receipt {action} successfully" });
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Administrator")]
    public Task<ActionResult<object>> GetPendingValidation(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = Ok(new
        {
            PaymentReceipts = Array.Empty<PaymentReceiptDto>(),
            TotalCount = 0,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = 0
        });

        return Task.FromResult<ActionResult<object>>(result);
    }
}
