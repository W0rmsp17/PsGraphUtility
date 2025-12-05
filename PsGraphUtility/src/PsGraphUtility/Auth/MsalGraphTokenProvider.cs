using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace PsGraphUtility.Auth;

public sealed class MsalGraphTokenProvider : IGraphTokenProvider
{
    private readonly string _publicClientId;
    private readonly Func<DeviceCodeResult, Task> _deviceCodeCallback;
    private readonly IPublicClientApplication _app;

    public MsalGraphTokenProvider(
        string publicClientId,
        Func<DeviceCodeResult, Task> deviceCodeCallback)
    {
        _publicClientId = publicClientId ?? throw new ArgumentNullException(nameof(publicClientId));
        _deviceCodeCallback = deviceCodeCallback ?? throw new ArgumentNullException(nameof(deviceCodeCallback));

        _app = PublicClientApplicationBuilder
            .Create(_publicClientId)
            .WithAuthority("https://login.microsoftonline.com/organizations")
            .WithDefaultRedirectUri()
            .Build();
    }

    public async Task<AccessTokenInfo> AcquireTokenAsync(
        GraphAuthContext context,
        CancellationToken cancellationToken = default)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));

        try
        {
            return context.AuthMode switch
            {
                AuthMode.Delegated => await AcquireDelegatedAsync(context, cancellationToken)
                                            .ConfigureAwait(false),
                AuthMode.Application => throw new NotImplementedException(
                    "Application auth not implemented yet."),
                _ => throw new GraphAuthException($"Unsupported auth mode '{context.AuthMode}'.")
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (MsalException ex)
        {
            throw new GraphAuthException("Failed to acquire token via MSAL.", ex);
        }
    }

    private async Task<AccessTokenInfo> AcquireDelegatedAsync(
        GraphAuthContext context,
        CancellationToken cancellationToken)
    {
        var scopes = context.Permissions.DelegatedScopes?.ToArray() ?? Array.Empty<string>();
        if (scopes.Length == 0)
            throw new GraphAuthException("No delegated scopes configured for this context.");

        AuthenticationResult result;

        try
        {
            var accounts = await _app.GetAccountsAsync().ConfigureAwait(false);

            var account =
                (!string.IsNullOrWhiteSpace(context.UserPrincipalName)
                    ? accounts.FirstOrDefault(a =>
                        a.Username.Equals(context.UserPrincipalName, StringComparison.OrdinalIgnoreCase))
                    : null)
                ?? accounts.FirstOrDefault();

            if (account is null)
                throw new MsalUiRequiredException("no_account", "No cached account.");

            result = await _app
                .AcquireTokenSilent(scopes, account)
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (MsalUiRequiredException)
        {
            result = await _app
                .AcquireTokenWithDeviceCode(scopes, _deviceCodeCallback)
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        var principal = result.ClaimsPrincipal;
        var claims = principal?.Claims ?? Enumerable.Empty<System.Security.Claims.Claim>();

        string tenantId =
            claims.FirstOrDefault(c => c.Type == "tid")?.Value
            ?? result.TenantId
            ?? string.Empty;

        string? upn =
            claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value
            ?? claims.FirstOrDefault(c => c.Type == "upn")?.Value
            ?? result.Account?.Username;

        return new AccessTokenInfo
        {
            AccessToken = result.AccessToken,
            ExpiresOnUtc = result.ExpiresOn,
            EffectiveScopesOrRoles = result.Scopes?.ToArray() ?? Array.Empty<string>(),
            AuthMode = context.AuthMode,
            AccountKind = context.AccountKind,
            TenantId = tenantId,
            UserPrincipalName = upn
        };
    }
}
