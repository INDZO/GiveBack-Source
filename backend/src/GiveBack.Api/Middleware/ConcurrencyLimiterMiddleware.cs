using GiveBack.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GiveBack.Api.Middleware;

public sealed class ConcurrencyLimiterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ConcurrencyLimiterMiddleware> _logger;
    private readonly SemaphoreSlim _globalSemaphore;
    private readonly IReadOnlyList<EndpointLimiterState> _endpointLimiters;
    private readonly TimeSpan _queueTimeout;
    private readonly int _retryAfterSeconds;

    public ConcurrencyLimiterMiddleware(RequestDelegate next, IOptions<ConcurrencyLimiterOptions> options, ILogger<ConcurrencyLimiterMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        var value = options.Value;
        _globalSemaphore = new SemaphoreSlim(Math.Max(1, value.GlobalLimit), Math.Max(1, value.GlobalLimit));
        _endpointLimiters = value.EndpointLimits
            .Where(e => e.Limit > 0)
            .Select(e => new EndpointLimiterState(e.Method, e.PathPattern, new SemaphoreSlim(e.Limit, e.Limit)))
            .ToList();
        _queueTimeout = TimeSpan.FromSeconds(Math.Max(1, value.QueueTimeoutSeconds));
        _retryAfterSeconds = Math.Max(1, value.RetryAfterSeconds);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var acquiredGlobal = false;
        EndpointLimiterState? endpointLimiter = null;
        var acquiredEndpoint = false;

        try
        {
            acquiredGlobal = await TryWaitAsync(_globalSemaphore, context.RequestAborted);
            if (!acquiredGlobal)
            {
                await RejectAsync(context, "Global concurrency limit queue timed out.");
                return;
            }

            endpointLimiter = _endpointLimiters.FirstOrDefault(l => l.Matches(context.Request.Method, context.Request.Path));
            if (endpointLimiter is not null)
            {
                acquiredEndpoint = await TryWaitAsync(endpointLimiter.Semaphore, context.RequestAborted);
                if (!acquiredEndpoint)
                {
                    await RejectAsync(context, $"Endpoint concurrency limit queue timed out for {context.Request.Method} {context.Request.Path}.");
                    return;
                }
            }

            await _next(context);
        }
        finally
        {
            if (acquiredEndpoint && endpointLimiter is not null)
            {
                endpointLimiter.Semaphore.Release();
            }
            if (acquiredGlobal)
            {
                _globalSemaphore.Release();
            }
        }
    }

    private async Task<bool> TryWaitAsync(SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        try
        {
            return await semaphore.WaitAsync(_queueTimeout, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private async Task RejectAsync(HttpContext context, string message)
    {
        _logger.LogWarning("Concurrency limiter rejected request: {Message}", message);
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.Headers.RetryAfter = _retryAfterSeconds.ToString();
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status503ServiceUnavailable,
            Title = "Service unavailable",
            Detail = message,
            Instance = context.Request.Path
        };
        await context.Response.WriteAsJsonAsync(problem);
    }

    private sealed class EndpointLimiterState
    {
        private readonly string _method;
        private readonly string[] _patternSegments;

        public EndpointLimiterState(string method, string pathPattern, SemaphoreSlim semaphore)
        {
            _method = method.ToUpperInvariant();
            _patternSegments = pathPattern.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            Semaphore = semaphore;
        }

        public SemaphoreSlim Semaphore { get; }

        public bool Matches(string method, PathString path)
        {
            if (!string.Equals(_method, method, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            var segments = path.Value?.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            if (segments.Length != _patternSegments.Length)
            {
                return false;
            }
            for (var i = 0; i < segments.Length; i++)
            {
                if (_patternSegments[i] == "*")
                {
                    continue;
                }
                if (!string.Equals(_patternSegments[i], segments[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
