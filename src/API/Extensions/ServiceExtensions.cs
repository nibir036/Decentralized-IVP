// Copyright (c) 2026 Muktadirul Islam Nibir. All rights reserved.

namespace API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
                policy
                    .WithOrigins("http://localhost:3000", "http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });

        services.AddHealthChecks()
            .AddNpgSql(
                name: "postgres",
                tags: ["ready", "db"]);

        return services;
    }
}