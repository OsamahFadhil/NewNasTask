using Insight.Invoicing.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Insight.Invoicing.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class AdminController : ControllerBase
{
    public AdminController()
    {
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> GetDashboardStatistics()
    {
        await Task.CompletedTask;

        return Ok(new
        {
            TotalContracts = 0,
            PendingContracts = 0,
            ApprovedContracts = 0,
            TotalRevenue = 0m,
            OverdueInstallments = 0,
            PendingReceipts = 0
        });
    }


    [HttpGet("reports/overdue")]
    public async Task<ActionResult<object>> GetOverdueReport(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] int? minOverdueDays = null)
    {
        await Task.CompletedTask;

        return Ok(new
        {
            OverdueInstallments = Array.Empty<InstallmentDto>(),
            TotalCount = 0,
            TotalOverdueAmount = 0m,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = 0
        });
    }


    [HttpGet("reports/revenue")]
    public async Task<ActionResult<object>> GetRevenueReport(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.AddMonths(-12);
        endDate ??= DateTime.UtcNow;

        await Task.CompletedTask;

        return Ok(new
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = 0m,
            TotalPaid = 0m,
            TotalOutstanding = 0m,
            PaymentsByMonth = Array.Empty<object>()
        });
    }

    
    [HttpGet("reports/tenant-activity")]
    public async Task<ActionResult<object>> GetTenantActivityReport(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        await Task.CompletedTask;

        return Ok(new
        {
            TenantActivities = Array.Empty<object>(),
            TotalCount = 0,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = 0
        });
    }
}

