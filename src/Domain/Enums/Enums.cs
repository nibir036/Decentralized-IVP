// Copyright (c) 2026 Muktadirul Islam Nibir. All rights reserved.

namespace Domain.Enums;

public enum UserRole      { Issuer, Holder, Verifier, Admin }
public enum UserStatus    { Active, Suspended, Deactivated }
public enum DidStatus     { Active, Deactivated, Rotated }
public enum DidMethod     { Indy, Web, Key, Cheqd }
public enum WalletType    { Indy, Askar }
public enum SchemaStatus  { Draft, Published, Deprecated }
public enum CredDefStatus { Active, Deprecated }
public enum RevRegStatus  { Init, Active, Full, Decommissioned }
public enum CredentialStatus  { Pending, Issued, Revoked, Expired }
public enum PresentationStatus { Sent, Received, Verified, Failed, Abandoned }
public enum VerificationResult { Pass, Fail, PartialFail }

public enum AuditAction
{
    UserRegistered, UserLogin, UserLogout,
    DidCreated, DidDeactivated,
    SchemaPublished, CredDefPublished,
    CredentialIssued, CredentialRevoked,
    PresentationRequested, PresentationVerified, PresentationFailed,
    WalletCreated, AdminAction
}