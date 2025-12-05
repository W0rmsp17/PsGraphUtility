using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;

namespace PsGraphUtility.Graph.Users.Interface;

public interface IUserLookupService
{
    Task<string> ResolveUserIdAsync(
        GraphAuthContext context,
        string upnOrId,
        CancellationToken cancellationToken = default);
}
