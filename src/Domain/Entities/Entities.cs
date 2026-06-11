// Copyright (c) 2026 Muktadirul Islam Nibir. All rights reserved.

using Domain.Enums;
using System.Text.Json;

namespace Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public abstract class AuditableEntity : BaseEntity
{
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class User : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DID? Did { get; set; }
    public WalletBinding? WalletBinding { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Credential> IssuedCredentials { get; set; } = [];
    public ICollection<Credential> ReceivedCredentials { get; set; } = [];
    public ICollection<CredentialSchema> Schemas { get; set; } = [];
    public ICollection<PresentationRequest> PresentationRequests { get; set; } = [];
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}

public class DID : AuditableEntity
{
    public Guid UserId { get; set; }
    public string DidValue { get; set; } = default!;
    public DidMethod DidMethod { get; set; }
    public JsonDocument? DidDocument { get; set; }
    public DateTime? DidDocumentCachedAt { get; set; }
    public DidStatus Status { get; set; } = DidStatus.Active;
    public bool AnchoredOnLedger { get; set; }
    public User User { get; set; } = default!;
}

public class WalletBinding : BaseEntity
{
    public Guid UserId { get; set; }
    public string AcaPyWalletId { get; set; } = default!;
    public string WalletLabel { get; set; } = default!;
    public WalletType WalletType { get; set; } = WalletType.Askar;
    public User User { get; set; } = default!;
}

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = default!;
    public string FamilyId { get; set; } = default!;
    public bool IsUsed { get; set; }
    public bool IsInvalidated { get; set; }
    public DateTime ExpiresAt { get; set; }
    public User User { get; set; } = default!;
}

public class CredentialSchema : AuditableEntity
{
    public Guid IssuerId { get; set; }
    public string SchemaName { get; set; } = default!;
    public string SchemaVersion { get; set; } = default!;
    public string[] Attributes { get; set; } = [];
    public string? LedgerSchemaId { get; set; }
    public SchemaStatus Status { get; set; } = SchemaStatus.Draft;
    public User Issuer { get; set; } = default!;
    public ICollection<CredentialDefinition> CredentialDefinitions { get; set; } = [];
}

public class CredentialDefinition : BaseEntity
{
    public Guid SchemaId { get; set; }
    public Guid IssuerId { get; set; }
    public string? LedgerCredDefId { get; set; }
    public string Tag { get; set; } = "default";
    public bool SupportRevocation { get; set; }
    public CredDefStatus Status { get; set; } = CredDefStatus.Active;
    public CredentialSchema Schema { get; set; } = default!;
    public User Issuer { get; set; } = default!;
    public ICollection<RevocationRegistry> RevocationRegistries { get; set; } = [];
    public ICollection<Credential> Credentials { get; set; } = [];
}

public class RevocationRegistry : BaseEntity
{
    public Guid CredentialDefinitionId { get; set; }
    public Guid IssuerId { get; set; }
    public string? LedgerRevRegId { get; set; }
    public int MaxCredentials { get; set; } = 1000;
    public int IssuedCount { get; set; }
    public RevRegStatus Status { get; set; } = RevRegStatus.Init;
    public string? TailsFileUrl { get; set; }
    public CredentialDefinition CredentialDefinition { get; set; } = default!;
    public User Issuer { get; set; } = default!;
}

public class Credential : BaseEntity
{
    public Guid IssuerId { get; set; }
    public Guid HolderId { get; set; }
    public Guid SchemaId { get; set; }
    public Guid CredentialDefinitionId { get; set; }
    public string CredentialType { get; set; } = default!;
    public string? CredentialHash { get; set; }
    public string? AcaPyCredExId { get; set; }
    public int? RevocationIndex { get; set; }
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpirationDate { get; set; }
    public CredentialStatus Status { get; set; } = CredentialStatus.Pending;
    public User Issuer { get; set; } = default!;
    public User Holder { get; set; } = default!;
    public CredentialSchema Schema { get; set; } = default!;
    public CredentialDefinition CredentialDefinition { get; set; } = default!;
    public RevocationRecord? RevocationRecord { get; set; }
}

public class RevocationRecord : BaseEntity
{
    public Guid CredentialId { get; set; }
    public Guid IssuerId { get; set; }
    public string Reason { get; set; } = default!;
    public string? LedgerTxHash { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Credential Credential { get; set; } = default!;
    public User Issuer { get; set; } = default!;
}

public class PresentationRequest : AuditableEntity
{
    public Guid VerifierId { get; set; }
    public Guid? HolderId { get; set; }
    public JsonDocument? RequestedAttributes { get; set; }
    public JsonDocument? RequestedPredicates { get; set; }
    public string? AcaPyPresExId { get; set; }
    public string? ConnectionId { get; set; }
    public PresentationStatus Status { get; set; } = PresentationStatus.Sent;
    public DateTime ExpiresAt { get; set; }
    public User Verifier { get; set; } = default!;
    public User? Holder { get; set; }
    public PresentationResponse? PresentationResponse { get; set; }
}

public class PresentationResponse : BaseEntity
{
    public Guid PresentationRequestId { get; set; }
    public Guid HolderId { get; set; }
    public string? PresentationHash { get; set; }
    public bool IsValid { get; set; }
    public string? FailureReason { get; set; }
    public DateTime VerifiedAt { get; set; } = DateTime.UtcNow;
    public PresentationRequest PresentationRequest { get; set; } = default!;
    public User Holder { get; set; } = default!;
    public VerificationRecord? VerificationRecord { get; set; }
}

public class VerificationRecord : BaseEntity
{
    public Guid VerifierId { get; set; }
    public Guid PresentationResponseId { get; set; }
    public bool SignatureValid { get; set; }
    public bool SchemaValid { get; set; }
    public bool NotRevoked { get; set; }
    public bool NotExpired { get; set; }
    public bool IssuerTrusted { get; set; }
    public VerificationResult OverallResult { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public User Verifier { get; set; } = default!;
    public PresentationResponse PresentationResponse { get; set; } = default!;
}

public class AuditLog : BaseEntity
{
    public Guid ActorId { get; set; }
    public AuditAction Action { get; set; }
    public string EntityType { get; set; } = default!;
    public Guid EntityId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public User Actor { get; set; } = default!;
}