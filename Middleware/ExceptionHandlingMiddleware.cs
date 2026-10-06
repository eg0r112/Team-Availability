using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using TeamAvailability.Domain.Exceptions;

namespace TeamAvailability.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

        var (status, title, detail) = exception switch
        {
            ValidationException validation => (StatusCodes.Status400BadRequest, "Validation failed",
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage))),
            NotFoundException notFound => (StatusCodes.Status404NotFound, "Not found", notFound.Message),
            ConflictException conflict => (StatusCodes.Status409Conflict, "Conflict", conflict.Message),
            DomainException domain => (StatusCodes.Status400BadRequest, "Business rule violation", domain.Message),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency conflict",
                "The record was modified by another user. Refresh and retry."),
            _ => (StatusCodes.Status500InternalServerError, "Internal server error",
                context.RequestServices.GetService<IHostEnvironment>()?.IsEnvironment("Testing") == true
                    ? GetDeepMessage(exception)
                    : "An unexpected error occurred.")
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = $"https://httpstatuses.com/{status}",
            title,
            status,
            detail,
            traceId = context.TraceIdentifier,
            correlationId = context.Items["CorrelationId"]?.ToString()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }

    private static string GetDeepMessage(Exception exception)
    {
        var messages = new List<string>();
        for (var ex = exception; ex is not null; ex = ex.InnerException)
            messages.Add(ex.Message);

        return string.Join(" -> ", messages);
    }
}
