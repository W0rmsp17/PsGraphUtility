using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Groups.Models;

namespace PsGraphUtility.Graph.Groups.Interface;

public interface IGetGroupService
{
    Task<GraphGroupDto> GetGroupAsync(
        GraphAuthContext context,
        string groupKey,
        string? type = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<GraphGroupDto>> ListGroupsAsync(
        GraphAuthContext context,
        string? typeFilter = null,  // "Unified" | "Security" | "MailSecurity" | "Distribution"
        CancellationToken cancellationToken = default);
}
