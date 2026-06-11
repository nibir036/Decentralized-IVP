// Copyright (c) 2026 Muktadirul Islam Nibir. All rights reserved.

using Infrastructure.AcaPy;
using Infrastructure.DidResolver;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(
                config.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = config["Jwt:Issuer"],
                    ValidAudience            = config["Jwt:Audience"],
                    IssuerSigningKey         = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]!)),
                    ClockSkew                = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        services.Configure<AcaPyOptions>(config.GetSection("AcaPy"));
        services.AddHttpClient<IAcaPyClient, AcaPyClient>(client =>
        {
            client.BaseAddress = new Uri(config["AcaPy:AdminUrl"]!);
            client.DefaultRequestHeaders.Add("X-Api-Key", config["AcaPy:AdminApiKey"]);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.Configure<DidResolverOptions>(config.GetSection("DidResolver"));
        services.AddHttpClient<IDidResolverClient, UniversalResolverClient>(client =>
        {
            client.BaseAddress = new Uri(config["DidResolver:BaseUrl"]!);
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.Configure<JwtOptions>(config.GetSection("Jwt"));
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}