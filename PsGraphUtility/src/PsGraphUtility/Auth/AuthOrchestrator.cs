//Path src/PsGraphUtility/Auth/AuthOrchestrator.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Routes.Account;
namespace PsGraphUtility.Auth;

public sealed class AuthOrchestrator : IAuthOrchestrator
{
    private readonly IGraphTokenProvider _tokenProvider;
    private readonly List<GraphAuthContext> _sessions = new();

    public AuthOrchestrator(IGraphTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
    }

    public async Task<GraphAuthContext> ConnectUserDelegatedAsync(
        string? tenantId,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        var authority = AuthDefaults.BuildAuthority(tenantId);

        var ctx = new GraphAuthContext
        {
            Name = displayName ?? string.Empty,
            AuthMode = AuthMode.Delegated,
            AccountKind = AccountKind.UserDelegated,
            TenantId = tenantId ?? string.Empty,
            Authority = authority,
            Permissions = new PermissionProfile
            {
                DelegatedScopes = AuthDefaults.UserDelegatedBaseScopes
            }
        };

        try
        {
            var token = await _tokenProvider.AcquireTokenAsync(ctx, cancellationToken)
                                           .ConfigureAwait(false);

            if (token.IsExpired())
                throw new GraphAuthException("Received an already-expired access token.");

            if (!string.IsNullOrWhiteSpace(token.TenantId))
            {
                ctx.TenantId = token.TenantId;
                ctx.Authority = AuthDefaults.BuildAuthority(token.TenantId);
            }

            ctx.UserPrincipalName = token.UserPrincipalName;

            if (string.IsNullOrWhiteSpace(ctx.Name))
            {
                var local = token.UserLocalPart;
                var shortId = token.ShortTenantId;
                var stamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");

                ctx.Name = !string.IsNullOrWhiteSpace(local)
                    ? $"gph-{local}-{stamp}"
                    : $"gph-{shortId}-{stamp}";
            }

            ctx.IsDefault = !_sessions.Any();
            _sessions.Add(ctx);
            return ctx;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GraphAuthException("Failed to acquire delegated token.", ex);
        }
    }



    public GraphAuthContext? GetDefaultContext() =>
        _sessions.FirstOrDefault(s => s.IsDefault);

    public IReadOnlyList<GraphAuthContext> GetAllContexts() => _sessions.AsReadOnly();

    public bool SetDefaultContext(Guid id)
    {
        var target = _sessions.FirstOrDefault(s => s.Id == id);
        if (target is null) return false;

        foreach (var s in _sessions) s.IsDefault = false;
        target.IsDefault = true;
        return true;
    }

    public bool RemoveContext(Guid id)
    {
        var removed = _sessions.RemoveAll(s => s.Id == id);
        if (removed <= 0) return false;

        if (!_sessions.Any(s => s.IsDefault) && _sessions.Count > 0)
            _sessions[0].IsDefault = true;

        return true;
    }
    public Task<GraphAuthContext> ConnectBootstrapAdminAsync(
        string? tenantId,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Bootstrap admin connect not implemented yet.");
    }

    public Task<GraphAuthContext> ConnectServiceAppAsync(
        string tenantId,
        string clientId,
        AuthCredentials credentials,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Service app connect not implemented yet.");
    }

}
