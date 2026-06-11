# ============================================================
# DecentralizedIVP - Phase 0 Scaffold (Windows PowerShell)
# Run from repo root: .\scaffold.ps1
# ============================================================

Write-Host "Creating .NET solution..." -ForegroundColor Cyan
dotnet new sln -n DecentralizedIVP

Write-Host "Creating projects..." -ForegroundColor Cyan
dotnet new webapi    -n API            --framework net10.0 --no-openapi -o src/API
dotnet new classlib  -n Application    --framework net10.0              -o src/Application
dotnet new classlib  -n Domain         --framework net10.0              -o src/Domain
dotnet new classlib  -n Infrastructure --framework net10.0              -o src/Infrastructure
dotnet new xunit     -n Tests.Unit     --framework net10.0              -o tests/Tests.Unit
dotnet new xunit     -n Tests.Integration --framework net10.0           -o tests/Tests.Integration

Write-Host "Adding to solution..." -ForegroundColor Cyan
dotnet sln add src/API/API.csproj
dotnet sln add src/Application/Application.csproj
dotnet sln add src/Domain/Domain.csproj
dotnet sln add src/Infrastructure/Infrastructure.csproj
dotnet sln add tests/Tests.Unit/Tests.Unit.csproj
dotnet sln add tests/Tests.Integration/Tests.Integration.csproj

Write-Host "Wiring project references..." -ForegroundColor Cyan
dotnet add src/API/API.csproj reference src/Application/Application.csproj
dotnet add src/API/API.csproj reference src/Infrastructure/Infrastructure.csproj
dotnet add src/Application/Application.csproj reference src/Domain/Domain.csproj
dotnet add src/Infrastructure/Infrastructure.csproj reference src/Application/Application.csproj
dotnet add tests/Tests.Unit/Tests.Unit.csproj reference src/Application/Application.csproj
dotnet add tests/Tests.Unit/Tests.Unit.csproj reference src/Domain/Domain.csproj
dotnet add tests/Tests.Integration/Tests.Integration.csproj reference src/API/API.csproj

Write-Host "Installing NuGet packages..." -ForegroundColor Cyan

dotnet add src/Application/Application.csproj package MediatR
dotnet add src/Application/Application.csproj package FluentValidation
dotnet add src/Application/Application.csproj package FluentValidation.DependencyInjectionExtensions
dotnet add src/Application/Application.csproj package AutoMapper
dotnet add src/Application/Application.csproj package Microsoft.Extensions.Logging.Abstractions

dotnet add src/Infrastructure/Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/Infrastructure/Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/Infrastructure/Infrastructure.csproj package BCrypt.Net-Next
dotnet add src/Infrastructure/Infrastructure.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/Infrastructure/Infrastructure.csproj package System.IdentityModel.Tokens.Jwt

dotnet add src/API/API.csproj package Serilog.AspNetCore
dotnet add src/API/API.csproj package Serilog.Sinks.Console
dotnet add src/API/API.csproj package Microsoft.AspNetCore.OpenApi
dotnet add src/API/API.csproj package Scalar.AspNetCore
dotnet add src/API/API.csproj package AutoMapper.Extensions.Microsoft.DependencyInjection

dotnet add tests/Tests.Unit/Tests.Unit.csproj package Moq
dotnet add tests/Tests.Unit/Tests.Unit.csproj package FluentAssertions
dotnet add tests/Tests.Integration/Tests.Integration.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/Tests.Integration/Tests.Integration.csproj package Testcontainers.PostgreSql
dotnet add tests/Tests.Integration/Tests.Integration.csproj package FluentAssertions

Write-Host "Creating directory structure..." -ForegroundColor Cyan
$dirs = @(
    "src/API/Controllers",
    "src/API/Middleware",
    "src/API/Extensions",
    "src/Application/Identity/Commands",
    "src/Application/Identity/Queries",
    "src/Application/Credentials/Commands",
    "src/Application/Credentials/Queries",
    "src/Application/Schemas/Commands",
    "src/Application/Schemas/Queries",
    "src/Application/Verification/Commands",
    "src/Application/Verification/Queries",
    "src/Application/Revocation/Commands",
    "src/Application/Revocation/Queries",
    "src/Application/Audit/Commands",
    "src/Application/Audit/Queries",
    "src/Application/Common/Interfaces",
    "src/Application/Common/Behaviors",
    "src/Application/Common/Mappings",
    "src/Domain/Entities",
    "src/Domain/Enums",
    "src/Domain/Events",
    "src/Domain/Common",
    "src/Infrastructure/Persistence/Configurations",
    "src/Infrastructure/Persistence/Migrations",
    "src/Infrastructure/AcaPy",
    "src/Infrastructure/DidResolver",
    "src/Infrastructure/Identity"
)
foreach ($dir in $dirs) {
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
}

Write-Host ""
Write-Host "Phase 0 scaffold complete." -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. docker compose up -d postgres von-network tails-server acapy universal-resolver"
Write-Host "  2. dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/API"
Write-Host "  3. dotnet ef database update --project src/Infrastructure --startup-project src/API"
Write-Host "  4. dotnet run --project src/API"