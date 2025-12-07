using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Entra.Users.Models;

namespace PsGraphUtility.Graph.Entra.Roles.Interfaces
{
    public interface IRoleMemberService
    {
        Task<IReadOnlyList<GraphUserDto>> GetMembersAsync(
            GraphAuthContext context,
            string roleKey,
            CancellationToken cancellationToken = default);

        Task AddMemberAsync(
            GraphAuthContext context,
            string roleKey,
            string userKey,
            CancellationToken cancellationToken = default);

        Task RemoveMemberAsync(
            GraphAuthContext context,
            string roleKey,
            string userKey,
            CancellationToken cancellationToken = default);
    }
}
