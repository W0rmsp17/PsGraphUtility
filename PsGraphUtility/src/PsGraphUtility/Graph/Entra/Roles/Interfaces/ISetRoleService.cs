using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Entra.Roles.Models;

namespace PsGraphUtility.Graph.Entra.Roles.Interfaces
{
    public interface ISetRoleService
    {
        Task<GraphRoleDto> SyncRoleMembersAsync(
            GraphAuthContext context,
            string roleKey,
            IEnumerable<string> members,
            bool ensureExact,
            CancellationToken cancellationToken = default);
    }
}
