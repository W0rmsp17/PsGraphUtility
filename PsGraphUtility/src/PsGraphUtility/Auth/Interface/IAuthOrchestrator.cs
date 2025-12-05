using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace PsGraphUtility.Auth;

public interface IAuthOrchestrator
{
    
    
    
    Task<GraphAuthContext> ConnectBootstrapAdminAsync(
        string? tenantId,
        string displayName,
        CancellationToken cancellationToken = default);

    Task<GraphAuthContext> ConnectUserDelegatedAsync(
        string? tenantId,
        string? displayName,
        CancellationToken cancellationToken = default);

    GraphAuthContext? GetDefaultContext();
    IReadOnlyList<GraphAuthContext> GetAllContexts();

    bool SetDefaultContext(Guid id); 
    bool RemoveContext(Guid id);


    Task<GraphAuthContext> ConnectServiceAppAsync(
        string tenantId,
        string clientId,
        AuthCredentials credentials,
        string displayName,
        CancellationToken cancellationToken = default);
}
