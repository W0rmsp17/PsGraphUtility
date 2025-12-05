//path src/PsGraphUtility/Auth/GraphAuthContext.cs
using System;
using System.Collections.Generic;

namespace PsGraphUtility.Auth;

public enum AuthMode
{
    Delegated,
    Application
}

public enum AccountKind
{
    BootstrapAdminDelegated,
    UserDelegated,
    ServiceApp,
    ServiceAccount
}

public sealed class AuthCredentials
{
    public bool UseDeviceCode { get; init; }

    public string? ClientSecret { get; init; }
    public string? CertificateThumbprint { get; init; }
}

public sealed class PermissionProfile
{
    public IReadOnlyCollection<string> DelegatedScopes { get; init; } =
        Array.Empty<string>();

    public IReadOnlyCollection<string> ApplicationRoles { get; init; } =
        Array.Empty<string>();
}

public sealed class BootstrapStatus
{
    public bool IsBootstrapAdmin { get; init; }
    public bool HasDirectoryWrite { get; init; }
    public bool HasUserWrite { get; init; }
    public bool HasApplicationWrite { get; init; }
    public bool HasOfflineAccess { get; init; }
}

public sealed class GraphAuthContext
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    public AuthMode AuthMode { get; init; }
    public AccountKind AccountKind { get; init; }

    public string TenantId { get; set; } = string.Empty;
    public string? ClientId { get; init; }
    public string Authority { get; set; } = string.Empty;

    public string? UserPrincipalName { get; set; }
    public string? ServiceAccountUpn { get; init; }

    public PermissionProfile Permissions { get; init; } = new();
    public AuthCredentials Credentials { get; init; } = new();
    public BootstrapStatus Bootstrap { get; init; } = new();

    public bool IsDefault { get; set; }

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastUsedAtUtc { get; set; }

    public bool IsDelegated => AuthMode == AuthMode.Delegated;
    public bool IsApplication => AuthMode == AuthMode.Application;

    public bool CanBootstrap =>
        AccountKind == AccountKind.BootstrapAdminDelegated &&
        Bootstrap.IsBootstrapAdmin &&
        Bootstrap.HasDirectoryWrite &&
        Bootstrap.HasUserWrite &&
        Bootstrap.HasApplicationWrite &&
        Bootstrap.HasOfflineAccess;
}
