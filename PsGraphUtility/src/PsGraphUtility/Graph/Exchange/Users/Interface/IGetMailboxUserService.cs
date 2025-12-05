using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Users.Models;

namespace PsGraphUtility.Graph.Exchange.Users.Interface
{
    public interface IGetMailboxUserService
    {
        Task<GraphMailboxUserDto> GetMailboxUserAsync(
            GraphAuthContext context,
            string userIdOrUpn,
            CancellationToken cancellationToken = default);
    }
}
