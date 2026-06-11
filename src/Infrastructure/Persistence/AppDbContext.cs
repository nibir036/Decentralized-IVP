// Copyright (c) 2026 Muktadirul Islam Nibir. All rights reserved.

using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User>                 Users                 => Set<User>();
    public DbSet<DID>                  DIDs                  => Set<DID>();
    public DbSet<WalletBinding>        WalletBindings        => Set<WalletBinding>();
    public DbSet<RefreshToken>         RefreshTokens         => Set<RefreshToken>();
    public DbSet<CredentialSchema>     CredentialSchemas     => Set<CredentialSchema>();
    public DbSet<CredentialDefinition> CredentialDefinitions => Set<CredentialDefinition>();
    public DbSet<RevocationRegistry>   RevocationRegistries  => Set<RevocationRegistry>();
    public DbSet<Credential>           Credentials           => Set<Credential>();
    public DbSet<RevocationRecord>     RevocationRecords     => Set<RevocationRecord>();
    public DbSet<PresentationRequest>  PresentationRequests  => Set<PresentationRequest>();
    public DbSet<PresentationResponse> PresentationResponses => Set<PresentationResponse>();
    public DbSet<VerificationRecord>   VerificationRecords   => Set<VerificationRecord>();
    public DbSet<AuditLog>             AuditLogs             => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        BlockAuditLogMutations();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        BlockAuditLogMutations();
        return base.SaveChangesAsync(ct);
    }

    private void BlockAuditLogMutations()
    {
        var violations = ChangeTracker.Entries<AuditLog>()
            .Where(e => e.State is EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (violations.Count > 0)
            throw new InvalidOperationException(
                "AuditLog records are append-only and cannot be modified or deleted.");
    }
}