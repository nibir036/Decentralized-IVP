// Copyright (c) 2026 Muktadirul Islam Nibir. All rights reserved.

using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Identity;

public class JwtOptions
{
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}

public interface IJwtService
{
    string GenerateAccessToken(User user);
    (string RawToken, string TokenHash, string FamilyId, DateTime ExpiresAt) GenerateRefreshToken(string? existingFamilyId = null);
    ClaimsPrincipal? ValidateAccessToken(string token);
}

public class JwtService(IOptions<JwtOptions> opts) : IJwtService
{
    private readonly JwtOptions _opts = opts.Value;

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             _opts.Issuer,
            audience:           _opts.Audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(_opts.AccessTokenExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string RawToken, string TokenHash, string FamilyId, DateTime ExpiresAt) GenerateRefreshToken(string? existingFamilyId = null)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
        var familyId = existingFamilyId ?? Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddDays(_opts.RefreshTokenExpiryDays);
        return (raw, hash, familyId, expiresAt);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.SecretKey));
            var result = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = false, // we check expiry separately
                ValidateIssuerSigningKey = true,
                ValidIssuer              = _opts.Issuer,
                ValidAudience            = _opts.Audience,
                IssuerSigningKey         = key
            }, out _);
            return result;
        }
        catch
        {
            return null;
        }
    }
}