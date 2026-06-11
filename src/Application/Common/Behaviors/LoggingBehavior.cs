// Copyright (c) 2026 Muktadirul Islam Nibir. All rights reserved.

using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);
        var response = await next();
        logger.LogInformation("Handled {RequestName}", typeof(TRequest).Name);
        return response;
    }
}