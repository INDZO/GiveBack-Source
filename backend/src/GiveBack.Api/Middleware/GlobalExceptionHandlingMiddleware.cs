using FluentValidation;
using GiveBack.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace GiveBack.Api.Middleware;

public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            ValidationException => ((int)HttpStatusCode.BadRequest, "Validation failed"),
            ValidationProblemException => ((int)HttpStatusCode.BadRequest, "Validation failed"),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized"),
            ForbiddenException => ((int)HttpStatusCode.Forbidden, "Forbidden"),
            NotFoundException => ((int)HttpStatusCode.NotFound, "Not found"),
            ConflictException => ((int)HttpStatusCode.Conflict, "Conflict"),
            DbUpdateConcurrencyException => ((int)HttpStatusCode.Conflict, "Concurrency conflict"),
            DbUpdateException => ((int)HttpStatusCode.Conflict, "Database update conflict"),
            ArgumentException => ((int)HttpStatusCode.BadRequest, "Bad request"),
            _ => ((int)HttpStatusCode.InternalServerError, "Unexpected server error")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception.");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception with status {StatusCode}.", statusCode);
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = _environment.IsDevelopment() ? exception.Message : title,
            Instance = context.Request.Path
        };

        if (exception is ValidationException validationException)
        {
            problem.Extensions["errors"] = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }
        else if (exception is ValidationProblemException validationProblemException)
        {
            problem.Extensions["errors"] = validationProblemException.Errors;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
