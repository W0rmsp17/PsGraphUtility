
using System.Threading;
using System.Threading.Tasks;

namespace PsGraphUtility.Auth;

public interface IGraphTokenProvider
{
    Task<AccessTokenInfo> AcquireTokenAsync(
        GraphAuthContext context,
        CancellationToken cancellationToken = default);
}
