// Copyright (c) 2026 Muktadirul Islam Nibir. All rights reserved.

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Email).IsUnique();
        b.Property(x => x.Email).HasMaxLength(256).IsRequired();
        b.Property(x => x.Name).HasMaxLength(256).IsRequired();
        b.Property(x => x.Role).HasConversion<string>();
        b.Property(x => x.Status).HasConversion<string>();
        b.HasOne(x => x.Did).WithOne(x => x.User).HasForeignKey<DID>(x => x.UserId);
        b.HasOne(x => x.WalletBinding).WithOne(x => x.User).HasForeignKey<WalletBinding>(x => x.UserId);
    }
}

public class DidConfiguration : IEntityTypeConfiguration<DID>
{
    public void Configure(EntityTypeBuilder<DID> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.DidValue).IsUnique();
        b.HasIndex(x => new { x.UserId, x.Status });
        b.Property(x => x.DidValue).HasMaxLength(512).IsRequired();
        b.Property(x => x.DidMethod).HasConversion<string>();
        b.Property(x => x.Status).HasConversion<string>();
        b.Property(x => x.DidDocument).HasColumnType("jsonb");
    }
}

public class WalletBindingConfiguration : IEntityTypeConfiguration<WalletBinding>
{
    public void Configure(EntityTypeBuilder<WalletBinding> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.UserId).IsUnique();
        b.HasIndex(x => x.AcaPyWalletId).IsUnique();
        b.Property(x => x.AcaPyWalletId).HasMaxLength(256).IsRequired();
        b.Property(x => x.WalletType).HasConversion<string>();
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.TokenHash).IsUnique();
        b.HasIndex(x => new { x.UserId, x.IsUsed, x.IsInvalidated });
        b.Property(x => x.TokenHash).HasMaxLength(512).IsRequired();
        b.Property(x => x.FamilyId).HasMaxLength(256).IsRequired();
    }
}

public class CredentialSchemaConfiguration : IEntityTypeConfiguration<CredentialSchema>
{
    public void Configure(EntityTypeBuilder<CredentialSchema> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.IssuerId, x.SchemaName, x.SchemaVersion }).IsUnique();
        b.HasIndex(x => x.LedgerSchemaId);
        b.Property(x => x.SchemaName).HasMaxLength(256).IsRequired();
        b.Property(x => x.SchemaVersion).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasConversion<string>();
        b.Property(x => x.Attributes).HasColumnType("text[]");
        b.HasOne(x => x.Issuer).WithMany(x => x.Schemas)
            .HasForeignKey(x => x.IssuerId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CredentialDefinitionConfiguration : IEntityTypeConfiguration<CredentialDefinition>
{
    public void Configure(EntityTypeBuilder<CredentialDefinition> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.LedgerCredDefId);
        b.Property(x => x.Tag).HasMaxLength(64).IsRequired();
        b.Property(x => x.Status).HasConversion<string>();
        b.HasOne(x => x.Schema).WithMany(x => x.CredentialDefinitions)
            .HasForeignKey(x => x.SchemaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Issuer).WithMany()
            .HasForeignKey(x => x.IssuerId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RevocationRegistryConfiguration : IEntityTypeConfiguration<RevocationRegistry>
{
    public void Configure(EntityTypeBuilder<RevocationRegistry> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.CredentialDefinitionId, x.Status });
        b.Property(x => x.Status).HasConversion<string>();
        b.HasOne(x => x.CredentialDefinition).WithMany(x => x.RevocationRegistries)
            .HasForeignKey(x => x.CredentialDefinitionId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CredentialConfiguration : IEntityTypeConfiguration<Credential>
{
    public void Configure(EntityTypeBuilder<Credential> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.IssuerId, x.Status });
        b.HasIndex(x => new { x.HolderId, x.Status });
        b.HasIndex(x => x.AcaPyCredExId);
        b.Property(x => x.CredentialType).HasMaxLength(256).IsRequired();
        b.Property(x => x.Status).HasConversion<string>();
        b.HasOne(x => x.Issuer).WithMany(x => x.IssuedCredentials)
            .HasForeignKey(x => x.IssuerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Holder).WithMany(x => x.ReceivedCredentials)
            .HasForeignKey(x => x.HolderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.RevocationRecord).WithOne(x => x.Credential)
            .HasForeignKey<RevocationRecord>(x => x.CredentialId);
    }
}

public class PresentationRequestConfiguration : IEntityTypeConfiguration<PresentationRequest>
{
    public void Configure(EntityTypeBuilder<PresentationRequest> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.VerifierId, x.Status });
        b.Property(x => x.Status).HasConversion<string>();
        b.Property(x => x.RequestedAttributes).HasColumnType("jsonb");
        b.Property(x => x.RequestedPredicates).HasColumnType("jsonb");
        b.HasOne(x => x.Verifier).WithMany(x => x.PresentationRequests)
            .HasForeignKey(x => x.VerifierId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.PresentationResponse).WithOne(x => x.PresentationRequest)
            .HasForeignKey<PresentationResponse>(x => x.PresentationRequestId);
    }
}

public class VerificationRecordConfiguration : IEntityTypeConfiguration<VerificationRecord>
{
    public void Configure(EntityTypeBuilder<VerificationRecord> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.VerifierId, x.Timestamp });
        b.Property(x => x.OverallResult).HasConversion<string>();
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ActorId, x.Timestamp });
        b.HasIndex(x => new { x.Action, x.Timestamp });
        b.Property(x => x.Action).HasConversion<string>();
        b.Property(x => x.Metadata).HasColumnType("jsonb");
        b.HasOne(x => x.Actor).WithMany(x => x.AuditLogs)
            .HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);
    }
}