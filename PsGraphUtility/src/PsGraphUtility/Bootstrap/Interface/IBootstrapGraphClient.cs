// src/PsGraphUtility/Bootstrap/IBootstrapGraphClient.cs
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;

namespace PsGraphUtility.Bootstrap.Interface;

public interface IBootstrapGraphClient
{
    Task<(string appId, string objectId, string displayName)> CreateBootstrapAppAsync(
        GraphAuthContext context,
        CancellationToken cancellationToken = default);

    Task<(string upn, string objectId)> CreateServiceAccountAsync(
        GraphAuthContext context,
        string? desiredUpn,
        CancellationToken cancellationToken = default);
}
