// Copyright (c) 2026 Muktadirul Islam Nibir. All rights reserved.

namespace Infrastructure.AcaPy;

// ── Options ───────────────────────────────────────────────────

public class AcaPyOptions
{
    public string AdminUrl { get; set; } = default!;
    public string AdminApiKey { get; set; } = default!;
    public string MultitenantJwtSecret { get; set; } = default!;
    public string WebhookApiKey { get; set; } = default!;
}

// ── Response DTOs ─────────────────────────────────────────────

public record CreateWalletResponse(string WalletId, string Token);
public record CreateDidResponse(string Did, string Verkey);
public record RegisterNymResponse(string Did, string Verkey);
public record GetTenantTokenResponse(string Token);

// ── Interface ─────────────────────────────────────────────────

public interface IAcaPyClient
{
    // Wallet & Identity
    Task<CreateWalletResponse> CreateWalletAsync(string label, string walletKey, CancellationToken ct = default);
    Task<GetTenantTokenResponse> GetTenantTokenAsync(string walletId, CancellationToken ct = default);
    Task<CreateDidResponse> CreateDidAsync(string tenantToken, CancellationToken ct = default);
    Task<RegisterNymResponse> RegisterNymAsync(string did, string verkey, string? role = null, CancellationToken ct = default);

    // Phase 2 — Schemas & CredDefs (stubs, implemented in Phase 2)
    Task<string> PublishSchemaAsync(string tenantToken, string name, string version, string[] attributes, CancellationToken ct = default);
    Task<string> CreateCredentialDefinitionAsync(string tenantToken, string schemaId, string tag, bool supportRevocation, CancellationToken ct = default);

    // Phase 3 — Issuance (stubs, implemented in Phase 3)
    Task<string> CreateConnectionInvitationAsync(string tenantToken, CancellationToken ct = default);
    Task<string> SendCredentialOfferAsync(string tenantToken, string connectionId, string credDefId, Dictionary<string, string> attributes, CancellationToken ct = default);

    // Phase 4 — Verification (stubs, implemented in Phase 4)
    Task<string> SendProofRequestAsync(string tenantToken, string connectionId, object requestedAttributes, object? requestedPredicates = null, CancellationToken ct = default);

    // Phase 5 — Revocation (stubs, implemented in Phase 5)
    Task<(string RevRegId, string TailsFileUrl)> ProvisionRevocationRegistryAsync(string tenantToken, string credDefId, int maxCredCount, CancellationToken ct = default);
    Task RevokeCredentialAsync(string tenantToken, string credExId, bool publish = true, CancellationToken ct = default);

    // Status
    Task<bool> IsReadyAsync(CancellationToken ct = default);
}

// ── Implementation ────────────────────────────────────────────

public class AcaPyClient(HttpClient http) : IAcaPyClient
{
    public async Task<bool> IsReadyAsync(CancellationToken ct = default)
    {
        var response = await http.GetAsync("/status/ready", ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<CreateWalletResponse> CreateWalletAsync(string label, string walletKey, CancellationToken ct = default)
    {
        var body = new
        {
            label,
            wallet_key = walletKey,
            wallet_type = "askar",
            wallet_dispatch_type = "default"
        };
        var response = await http.PostAsJsonAsync("/multitenancy/wallet", body, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<AcaPyWalletResult>(cancellationToken: ct);
        return new CreateWalletResponse(json!.WalletId, json.Token);
    }

    public async Task<GetTenantTokenResponse> GetTenantTokenAsync(string walletId, CancellationToken ct = default)
    {
        var response = await http.PostAsync($"/multitenancy/wallet/{walletId}/token", null, ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<AcaPyTokenResult>(cancellationToken: ct);
        return new GetTenantTokenResponse(json!.Token);
    }

    public async Task<CreateDidResponse> CreateDidAsync(string tenantToken, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/wallet/did/create");
        request.Headers.Add("Authorization", $"Bearer {tenantToken}");
        request.Content = JsonContent.Create(new { method = "sov", options = new { key_type = "ed25519" } });

        var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<AcaPyDidResult>(cancellationToken: ct);
        return new CreateDidResponse(json!.Result.Did, json.Result.Verkey);
    }

    public async Task<RegisterNymResponse> RegisterNymAsync(string did, string verkey, string? role = null, CancellationToken ct = default)
    {
        var url = $"/ledger/register-nym?did={did}&verkey={verkey}";
        if (role != null) url += $"&role={role}";

        var response = await http.PostAsync(url, null, ct);
        response.EnsureSuccessStatusCode();
        return new RegisterNymResponse(did, verkey);
    }

    // ── Phase 2–5 stubs (return NotImplementedException until implemented) ──

    public Task<string> PublishSchemaAsync(string t, string n, string v, string[] a, CancellationToken ct = default)
        => throw new NotImplementedException("Implemented in Phase 2");

    public Task<string> CreateCredentialDefinitionAsync(string t, string s, string tag, bool r, CancellationToken ct = default)
        => throw new NotImplementedException("Implemented in Phase 2");

    public Task<string> CreateConnectionInvitationAsync(string t, CancellationToken ct = default)
        => throw new NotImplementedException("Implemented in Phase 3");

    public Task<string> SendCredentialOfferAsync(string t, string c, string d, Dictionary<string, string> a, CancellationToken ct = default)
        => throw new NotImplementedException("Implemented in Phase 3");

    public Task<string> SendProofRequestAsync(string t, string c, object ra, object? rp = null, CancellationToken ct = default)
        => throw new NotImplementedException("Implemented in Phase 4");

    public Task<(string, string)> ProvisionRevocationRegistryAsync(string t, string d, int m, CancellationToken ct = default)
        => throw new NotImplementedException("Implemented in Phase 5");

    public Task RevokeCredentialAsync(string t, string c, bool p = true, CancellationToken ct = default)
        => throw new NotImplementedException("Implemented in Phase 5");

    // ── Private response records ──────────────────────────────

    private record AcaPyWalletResult(
        [property: System.Text.Json.Serialization.JsonPropertyName("wallet_id")] string WalletId,
        [property: System.Text.Json.Serialization.JsonPropertyName("token")] string Token);

    private record AcaPyTokenResult(
        [property: System.Text.Json.Serialization.JsonPropertyName("token")] string Token);

    private record AcaPyDidInfo(
        [property: System.Text.Json.Serialization.JsonPropertyName("did")] string Did,
        [property: System.Text.Json.Serialization.JsonPropertyName("verkey")] string Verkey);

    private record AcaPyDidResult(
        [property: System.Text.Json.Serialization.JsonPropertyName("result")] AcaPyDidInfo Result);
}