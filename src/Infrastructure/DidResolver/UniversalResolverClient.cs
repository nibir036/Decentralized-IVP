// Copyright (c) 2026 Muktadirul Islam Nibir. All rights reserved.

using System.Text.Json;

namespace Infrastructure.DidResolver;

public class DidResolverOptions
{
    public string BaseUrl { get; set; } = default!;
}

public interface IDidResolverClient
{
    Task<JsonDocument?> ResolveAsync(string did, CancellationToken ct = default);
}

public class UniversalResolverClient(HttpClient http) : IDidResolverClient
{
    public async Task<JsonDocument?> ResolveAsync(string did, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"/1.0/identifiers/{did}", ct);

        if (!response.IsSuccessStatusCode)
            return null;

        var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonDocument.ParseAsync(stream, cancellationToken: ct);
    }
}