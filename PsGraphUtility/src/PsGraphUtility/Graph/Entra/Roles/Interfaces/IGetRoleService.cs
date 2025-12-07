using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Entra.Roles.Models;

namespace PsGraphUtility.Graph.Entra.Roles.Interfaces
{
    public interface IGetRoleService
    {
        Task<GraphRoleDto> GetRoleAsync(
            GraphAuthContext context,
            string roleKey,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<GraphRoleDto>> ListRolesAsync(
            GraphAuthContext context,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<GraphRoleDto>> GetRolesForUserAsync(
            GraphAuthContext context,
            string userKey,
            CancellationToken cancellationToken = default);
    }
}
