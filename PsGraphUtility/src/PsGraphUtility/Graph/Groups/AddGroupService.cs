using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Groups.Interface;
using PsGraphUtility.Graph.Groups.Models;
using PsGraphUtility.Graph.HTTP;
using PsGraphUtility.Routes.Groups;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PsGraphUtility.Graph.Groups;

public sealed class AddGroupService : IAddGroupService
{
    private readonly IGraphClient _graph;
    private readonly IGetGroupService _getter;

    public AddGroupService(
        IGraphClient graph,
        IGetGroupService getter)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _getter = getter ?? throw new ArgumentNullException(nameof(getter));
    }

    public async Task<GraphGroupDto> CreateGroupAsync(
        GraphAuthContext context,
        string displayName,
        string mailNickname,
        string? description = null,
        string? visibility = null,
        string groupType = "Unified",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name must be provided.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(mailNickname))
            throw new ArgumentException("Mail nickname must be provided.", nameof(mailNickname));

        var isUnified = string.Equals(groupType, "Unified", StringComparison.OrdinalIgnoreCase);

        var payload = new Dictionary<string, object?>
        {
            ["displayName"] = displayName,
            ["mailNickname"] = mailNickname,
            ["description"] = description,
            ["visibility"] = visibility,
            ["mailEnabled"] = isUnified,
            ["securityEnabled"] = !isUnified,
            ["groupTypes"] = isUnified ? new[] { "Unified" } : Array.Empty<string>()
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            GroupRoutes.Groups())
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await _graph.SendAsync(context, request, cancellationToken)
                                   .ConfigureAwait(false);

        var body = await GraphResponseHelper.EnsureSuccessAsync(
            response,
            $"create group '{displayName}'",
            cancellationToken);

        using var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("id", out var idProp))
            throw new GraphAuthException("Graph create group response did not contain an 'id'.");

        var groupId = idProp.GetString();
        if (string.IsNullOrWhiteSpace(groupId))
            throw new GraphAuthException("Graph create group response contained an empty 'id'.");

        return await _getter
            .GetGroupAsync(context, groupId, null, cancellationToken)
            .ConfigureAwait(false);
    }

}
