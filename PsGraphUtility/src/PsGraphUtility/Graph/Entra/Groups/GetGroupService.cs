using PsGraphUtility.Auth;
using PsGraphUtility.Graph.HTTP;
using PsGraphUtility.Routes.Groups;
using PsGraphUtility.Graph.Entra.Groups.Interface;
using PsGraphUtility.Graph.Entra.Groups.Models;
using PsGraphUtility.Graph.Interface;
using System.Text.Json;

namespace PsGraphUtility.Graph.Entra.Groups;

public sealed class GetGroupService : IGetGroupService
{
    private readonly IGraphClient _graph;
    private readonly IGroupLookupService _lookup;

    public GetGroupService(IGraphClient graph, IGroupLookupService lookup)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));
    }

    public async Task<GraphGroupDto> GetGroupAsync(
        GraphAuthContext context,
        string groupKey,
        string? type = null,
        CancellationToken cancellationToken = default)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        if (string.IsNullOrWhiteSpace(groupKey))
            throw new ArgumentException("Group key must be provided.", nameof(groupKey));

        var groupId = await _lookup.ResolveGroupIdAsync(
            context, groupKey, cancellationToken).ConfigureAwait(false);

        using var req = new HttpRequestMessage(
            HttpMethod.Get,
            GroupRoutes.GroupById(groupId));

        var res = await _graph.SendAsync(context, req, cancellationToken)
                              .ConfigureAwait(false);

        var body = await GraphResponseHelper.EnsureSuccessAsync(
            res,
            $"get group '{groupKey}'",
            cancellationToken);

        using var doc = JsonDocument.Parse(body);
        return MapGroup(doc.RootElement);
    }





    public async Task<IReadOnlyList<GraphGroupDto>> ListGroupsAsync(
       GraphAuthContext context,
       string? typeFilter = null,
       CancellationToken cancellationToken = default)
    {
        var url = BuildListUrl(typeFilter);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await _graph.SendAsync(context, request, cancellationToken)
                                   .ConfigureAwait(false);
        //var body = await response.Content.ReadAsStringAsync(cancellationToken)
        //   .ConfigureAwait(false);

        var body = await GraphResponseHelper.EnsureSuccessAsync(
            response,
            "list groups",
            cancellationToken);

        using var doc = JsonDocument.Parse(body);
        var list = doc.RootElement.GetProperty("value")
                                  .EnumerateArray()
                                  .Select(MapGroup)
                                  .ToList();

        var t = typeFilter?.ToLowerInvariant();
        if (t is "mailsecurity" or "mail-sec" or "distribution" or "distro")
        {
            list = list
                .Where(g => !(g.GroupTypes ?? Array.Empty<string>())
                    .Any(x => string.Equals(x, "Unified", StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        return list;
    }


    private static string BuildListUrl(string? typeFilter)
    {
        var baseUrl = GroupRoutes.Groups();
        var select =
            "$select=id,displayName,description,mail,mailNickname,mailEnabled,securityEnabled,groupTypes";

        string? filter = typeFilter?.ToLowerInvariant() switch
        {
            "unified" or "m365" =>
                "groupTypes/any(c:c eq 'Unified')",
            "security" =>
                "mailEnabled eq false and securityEnabled eq true",
            "mailsecurity" or "mail-sec" =>
                "mailEnabled eq true and securityEnabled eq true",
            "distribution" or "distro" =>
                "mailEnabled eq true and securityEnabled eq false",
            _ => null
        };

        if (filter is null)
            return $"{baseUrl}?{select}&$top=200";

        var encodedFilter = Uri.EscapeDataString(filter);
        return $"{baseUrl}?{select}&$filter={encodedFilter}&$top=200";
    }


    private static GraphGroupDto MapGroup(JsonElement root) => new()
    {
        Id = root.GetProperty("id").GetString() ?? string.Empty,
        DisplayName = root.GetProperty("displayName").GetString() ?? string.Empty,
        Description = root.TryGetProperty("description", out var desc) ? desc.GetString() : null,

        Mail = root.TryGetProperty("mail", out var mail) ? mail.GetString() : null,
        MailNickname = root.TryGetProperty("mailNickname", out var nick) ? nick.GetString() : null,
        MailEnabled = root.TryGetProperty("mailEnabled", out var me) && me.ValueKind == JsonValueKind.True,
        SecurityEnabled = root.TryGetProperty("securityEnabled", out var se) && se.ValueKind == JsonValueKind.True,

        GroupTypes = root.TryGetProperty("groupTypes", out var gt) && gt.ValueKind == JsonValueKind.Array
        ? gt.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray()
        : Array.Empty<string>(),

        Classification = root.TryGetProperty("classification", out var cls) ? cls.GetString() : null,

        CreatedDateTime = root.TryGetProperty("createdDateTime", out var cd) && cd.ValueKind == JsonValueKind.String
        ? cd.GetDateTimeOffset()
        : null,
        ExpirationDateTime = root.TryGetProperty("expirationDateTime", out var ed) && ed.ValueKind == JsonValueKind.String
        ? ed.GetDateTimeOffset()
        : null,
        RenewedDateTime = root.TryGetProperty("renewedDateTime", out var rd) && rd.ValueKind == JsonValueKind.String
        ? rd.GetDateTimeOffset()
        : null,

        IsAssignableToRole = root.TryGetProperty("isAssignableToRole", out var iar) && iar.ValueKind != JsonValueKind.Null
        ? iar.GetBoolean()
        : null,

        MembershipRule = root.TryGetProperty("membershipRule", out var mr) ? mr.GetString() : null,
        MembershipRuleProcessingState = root.TryGetProperty("membershipRuleProcessingState", out var mrps) ? mrps.GetString() : null,

        OnPremisesLastSyncDateTime = root.TryGetProperty("onPremisesLastSyncDateTime", out var ls) && ls.ValueKind == JsonValueKind.String
        ? ls.GetDateTimeOffset()
        : null,
        OnPremisesSecurityIdentifier = root.TryGetProperty("onPremisesSecurityIdentifier", out var sid) ? sid.GetString() : null,
        OnPremisesSyncEnabled = root.TryGetProperty("onPremisesSyncEnabled", out var sync) && sync.ValueKind != JsonValueKind.Null
        ? sync.GetBoolean()
        : null,

        PreferredDataLocation = root.TryGetProperty("preferredDataLocation", out var pdl) ? pdl.GetString() : null,
        PreferredLanguage = root.TryGetProperty("preferredLanguage", out var pl) ? pl.GetString() : null,

        ProxyAddresses = root.TryGetProperty("proxyAddresses", out var pa) && pa.ValueKind == JsonValueKind.Array
        ? pa.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray()
        : Array.Empty<string>(),

        ResourceBehaviorOptions = root.TryGetProperty("resourceBehaviorOptions", out var rbo) && rbo.ValueKind == JsonValueKind.Array
        ? rbo.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray()
        : Array.Empty<string>(),
        ResourceProvisioningOptions = root.TryGetProperty("resourceProvisioningOptions", out var rpo) && rpo.ValueKind == JsonValueKind.Array
        ? rpo.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray()
        : Array.Empty<string>(),

        Theme = root.TryGetProperty("theme", out var theme) ? theme.GetString() : null,
        Visibility = root.TryGetProperty("visibility", out var vis) ? vis.GetString() : null
    };

}
