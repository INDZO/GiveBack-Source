using GiveBack.Api.Middleware;

namespace GiveBack.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

    public static IApplicationBuilder UseConcurrencyLimiter(this IApplicationBuilder app)
        => app.UseMiddleware<ConcurrencyLimiterMiddleware>();
}
