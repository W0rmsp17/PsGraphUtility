using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Entra.Users.Models;

namespace PsGraphUtility.Graph.Entra.Groups.Interface
{
    public interface IGroupMemberService
    {
        Task<IReadOnlyList<GraphUserDto>> GetMembersAsync(
            GraphAuthContext context,
            string groupIdOrName,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<GraphUserDto>> GetOwnersAsync(
            GraphAuthContext context,
            string groupIdOrName,
            CancellationToken cancellationToken = default);

        Task AddMemberAsync(
            GraphAuthContext context,
            string groupKey,
            string userKey,
            CancellationToken cancellationToken = default);

        Task RemoveMemberAsync(
            GraphAuthContext context,
            string groupKey,
            string userKey,
            CancellationToken cancellationToken = default);

        Task AddOwnerAsync(
            GraphAuthContext context,
            string groupKey,
            string userKey,
            CancellationToken cancellationToken = default);

        Task RemoveOwnerAsync(
            GraphAuthContext context,
            string groupKey,
            string userKey,
            CancellationToken cancellationToken = default);
    }
}
