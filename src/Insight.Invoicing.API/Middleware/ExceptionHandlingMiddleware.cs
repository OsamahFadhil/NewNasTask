using FluentValidation;
using Insight.Invoicing.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Insight.Invoicing.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = CreateProblemDetails(exception);

        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(problemDetails, jsonOptions);
        await context.Response.WriteAsync(json);
    }

    private static ProblemDetails CreateProblemDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException validationEx => new ValidationProblemDetails(
                validationEx.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Title = "Validation failed",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = "One or more validation errors occurred"
            },

            InvalidContractStateException contractEx => new ProblemDetails
            {
                Title = "Invalid contract state",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = contractEx.Message
            },

            InvalidInstallmentOperationException installmentEx => new ProblemDetails
            {
                Title = "Invalid installment operation",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = installmentEx.Message
            },

            InvalidPaymentAmountException paymentEx => new ProblemDetails
            {
                Title = "Invalid payment amount",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = paymentEx.Message
            },

            BusinessRuleViolationException businessEx => new ProblemDetails
            {
                Title = "Business rule violation",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = businessEx.Message
            },

            DomainException domainEx => new ProblemDetails
            {
                Title = "Domain error",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = domainEx.Message
            },

            UnauthorizedAccessException => new ProblemDetails
            {
                Title = "Access denied",
                Status = (int)HttpStatusCode.Forbidden,
                Detail = "You don't have permission to access this resource"
            },

            ArgumentNullException => new ProblemDetails
            {
                Title = "Required parameter missing",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = "A required parameter was not provided"
            },

            ArgumentException argEx => new ProblemDetails
            {
                Title = "Invalid argument",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = argEx.Message
            },

            KeyNotFoundException => new ProblemDetails
            {
                Title = "Resource not found",
                Status = (int)HttpStatusCode.NotFound,
                Detail = "The requested resource was not found"
            },

            _ => new ProblemDetails
            {
                Title = "Internal server error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "An unexpected error occurred"
            }
        };
    }
}
