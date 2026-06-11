// Copyright (c) 2026 Muktadirul Islam Nibir. All rights reserved.

using FluentValidation;
using System.Net;
using System.Text.Json;

namespace API.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Method} {Path}",
                ctx.Request.Method, ctx.Request.Path);
            await HandleExceptionAsync(ctx, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        var (statusCode, code, message, details) = ex switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "VALIDATION_FAILED",
                "One or more validation errors occurred.",
                ve.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList()),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "UNAUTHORIZED",
                "Authentication is required.",
                (IEnumerable<string>)[]),

            KeyNotFoundException knf => (
                HttpStatusCode.NotFound,
                "NOT_FOUND",
                knf.Message,
                (IEnumerable<string>)[]),

            InvalidOperationException ioe => (
                HttpStatusCode.BadRequest,
                "INVALID_OPERATION",
                ioe.Message,
                (IEnumerable<string>)[]),

            _ => (
                HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR",
                "An unexpected error occurred.",
                (IEnumerable<string>)[])
        };

        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = (int)statusCode;

        var envelope = new
        {
            success = false,
            error = new { code, message, details },
            timestamp = DateTime.UtcNow
        };

        await ctx.Response.WriteAsync(
            JsonSerializer.Serialize(envelope,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
    }
}