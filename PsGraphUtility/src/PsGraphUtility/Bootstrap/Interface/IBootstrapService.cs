using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;

namespace PsGraphUtility.Bootstrap;

public interface IBootstrapService
{
    Task<BootstrapProfile> EnsureBootstrapAsync(
        GraphAuthContext context,
        CancellationToken cancellationToken = default);
}
