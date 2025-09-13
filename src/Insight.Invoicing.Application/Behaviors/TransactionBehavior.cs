using MediatR;
using Microsoft.Extensions.Logging;

namespace Insight.Invoicing.Application.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Commands typically end with "Command"
        if (!requestName.EndsWith("Command"))
        {
            return await next();
        }

        _logger.LogInformation(
            "Starting transaction for command {CommandName}",
            requestName);

        try
        {
            var response = await next();

            _logger.LogInformation(
                "Completed transaction for command {CommandName}",
                requestName);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Transaction failed for command {CommandName}",
                requestName);

            throw;
        }
    }
}

