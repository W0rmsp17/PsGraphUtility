using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Entra.Groups.Models;

namespace PsGraphUtility.Graph.Entra.Groups.Interface
{
    public interface IUserGroupService
    {
        Task<IReadOnlyList<GraphGroupDto>> GetGroupsForUserAsync(
            GraphAuthContext context,
            string userKey, // UPN or Id
            CancellationToken cancellationToken = default);
    }
}
