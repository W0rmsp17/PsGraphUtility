using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph;
using PsGraphUtility.Graph.Groups.Interface;
using PsGraphUtility.Graph.Groups.Models;
using PsGraphUtility.Routes.Groups;
using PsGraphUtility.Graph.HTTP;

namespace PsGraphUtility.Graph.Groups;

public sealed class SetGroupService : ISetGroupService
{
    private readonly IGraphClient _graph;
    private readonly IGroupLookupService _lookup;
    private readonly IGetGroupService _getter;

    public SetGroupService(
        IGraphClient graph,
        IGroupLookupService lookup,
        IGetGroupService getter)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));
        _getter = getter ?? throw new ArgumentNullException(nameof(getter));
    }

    public async Task<GraphGroupDto> UpdateGroupAsync(
        GraphAuthContext context,
        string groupKey,
        string? displayName = null,
        string? description = null,
        string? mailNickname = null,
        string? visibility = null,
        bool? mailEnabled = null,
        bool? securityEnabled = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(groupKey))
            throw new ArgumentException("Group key must be provided.", nameof(groupKey));

        var payload = new Dictionary<string, object>();

        if (displayName is not null) payload["displayName"] = displayName;
        if (description is not null) payload["description"] = description;
        if (mailNickname is not null) payload["mailNickname"] = mailNickname;
        if (visibility is not null) payload["visibility"] = visibility;
        if (mailEnabled.HasValue) payload["mailEnabled"] = mailEnabled.Value;
        if (securityEnabled.HasValue) payload["securityEnabled"] = securityEnabled.Value;

        if (payload.Count == 0)
            throw new GraphAuthException("No fields provided to update.");

        var groupId = await _lookup
            .ResolveGroupIdAsync(context, groupKey, cancellationToken)
            .ConfigureAwait(false);

        var json = JsonSerializer.Serialize(payload);
        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            GroupRoutes.GroupById(groupId))
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await _graph.SendAsync(context, request, cancellationToken)
                                   .ConfigureAwait(false);

        await GraphResponseHelper.EnsureSuccessAsync(
            response,
            $"update group '{groupKey}'",
            cancellationToken);

        return await _getter
            .GetGroupAsync(context, groupId, null, cancellationToken)
            .ConfigureAwait(false);
    }


}
