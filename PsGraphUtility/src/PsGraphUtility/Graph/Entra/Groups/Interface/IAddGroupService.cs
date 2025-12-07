using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Entra.Groups.Models;
using System.Threading;
using System.Threading.Tasks;

namespace PsGraphUtility.Graph.Entra.Groups.Interface
{
    public interface IAddGroupService
    {
        Task<GraphGroupDto> CreateGroupAsync(
            GraphAuthContext context,
            string displayName,
            string mailNickname,
            string? description = null,
            string? visibility = null,
            string groupType = "Unified",
            CancellationToken cancellationToken = default);
    }
}
