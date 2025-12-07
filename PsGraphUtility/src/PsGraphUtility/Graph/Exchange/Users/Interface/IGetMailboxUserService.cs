using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Exchange.Users.Models;

namespace PsGraphUtility.Graph.Exchange.Users.Interface
{
    public interface IGetMailboxUserService
    {
        Task<GraphMailboxUserDto> GetMailboxUserAsync(
            GraphAuthContext context,
            string userKey,
            bool includeMailbox,
            bool includeMailboxStats,
            bool includeFolderStats,
            bool includeFolderPermissions,
            CancellationToken cancellationToken = default);
    }
}
