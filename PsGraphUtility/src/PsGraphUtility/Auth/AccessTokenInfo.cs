using System;
namespace PsGraphUtility.Auth;

public sealed class AccessTokenInfo
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTimeOffset ExpiresOnUtc { get; init; }
    public string[] EffectiveScopesOrRoles { get; init; } = Array.Empty<string>();

    public AuthMode AuthMode { get; init; }
    public AccountKind AccountKind { get; init; }

    public string TenantId { get; init; } = string.Empty;
    public string? UserPrincipalName { get; init; }

    public bool IsExpired() => ExpiresOnUtc <= DateTimeOffset.UtcNow;

    public string ShortTenantId =>
        TenantId.Length > 8 ? TenantId[..8] : TenantId;

    public string UserLocalPart =>
        string.IsNullOrEmpty(UserPrincipalName)
            ? string.Empty
            : UserPrincipalName.Split('@')[0];
}

