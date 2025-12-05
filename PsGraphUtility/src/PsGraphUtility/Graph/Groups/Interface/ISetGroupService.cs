using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Groups.Models;

namespace PsGraphUtility.Graph.Groups.Interface;

public interface ISetGroupService
{
    Task<GraphGroupDto> UpdateGroupAsync(
        GraphAuthContext context,
        string groupKey,                 // id | displayName | mail | nickname
        string? displayName = null,
        string? description = null,
        string? mailNickname = null,
        string? visibility = null,       // "Public" | "Private" | null
        bool? mailEnabled = null,
        bool? securityEnabled = null,
        CancellationToken cancellationToken = default);
}
