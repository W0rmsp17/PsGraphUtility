//path src/PsGraphUtility/Graph/Users/SetUserService.cs
using PsGraphUtility.Auth;
using PsGraphUtility.Graph;
using PsGraphUtility.Graph.HTTP;
using PsGraphUtility.Graph.Users;
using PsGraphUtility.Graph.Users.Interface;
using PsGraphUtility.Graph.Users.Models;
using PsGraphUtility.Routes.Users;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PsGraphUtility.Graph.Users;

public sealed class SetUserService : ISetUserService
{
    private readonly IGraphClient _graph;
    private readonly IUserLookupService _lookup;

    public SetUserService(IGraphClient graph, IUserLookupService lookup)
    {
        _graph = graph;
        _lookup = lookup;
    }

    public async Task<GraphUserDto> UpdateUserAsync(
        GraphAuthContext context,
        string userIdOrUpn,
        string? displayName = null,
        string? mail = null,
        string? jobTitle = null,
        string? mobilePhone = null,
        string? officeLocation = null,
        string[]? businessPhones = null,
        string? password = null,
        bool? forceChangePassword = null,
        bool? blockSignIn = null,
        CancellationToken cancellationToken = default)
    {
        var userId = await _lookup.ResolveUserIdAsync(
            context, userIdOrUpn, cancellationToken).ConfigureAwait(false);

        var payload = new Dictionary<string, object>();

        if (displayName is not null) payload["displayName"] = displayName;
        if (mail is not null) payload["mail"] = mail;
        if (jobTitle is not null) payload["jobTitle"] = jobTitle;
        if (mobilePhone is not null) payload["mobilePhone"] = mobilePhone;
        if (officeLocation is not null) payload["officeLocation"] = officeLocation;
        if (businessPhones is not null) payload["businessPhones"] = businessPhones;

        if (blockSignIn.HasValue)
        {
            payload["accountEnabled"] = !blockSignIn.Value;
        }

        if (password is not null || forceChangePassword.HasValue)
        {
            var pp = new Dictionary<string, object>();

            if (password is not null)
                pp["password"] = password;

            if (forceChangePassword.HasValue)
                pp["forceChangePasswordNextSignIn"] = forceChangePassword.Value;

            if (pp.Count > 0)
                payload["passwordProfile"] = pp;
        }

        if (payload.Count == 0)
            throw new GraphAuthException("No fields provided to update.");

        var json = JsonSerializer.Serialize(payload);

        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            UserRoutes.UserById(userId))
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await _graph.SendAsync(context, request, cancellationToken)
                                   .ConfigureAwait(false);

        var body = await GraphResponseHelper.EnsureSuccessAsync(
            response,
            $"update user '{userIdOrUpn}'",
            cancellationToken);

        if (string.IsNullOrWhiteSpace(body))
        {
            using var getReq = new HttpRequestMessage(
                HttpMethod.Get,
                UserRoutes.UserById(userId));

            var getResp = await _graph.SendAsync(context, getReq, cancellationToken)
                                      .ConfigureAwait(false);
            var getBody = await getResp.Content.ReadAsStringAsync(cancellationToken)
                                               .ConfigureAwait(false);

            if (!getResp.IsSuccessStatusCode)
                throw new GraphAuthException(
                    $"Failed to re-read user '{userIdOrUpn}' after update. Status: {(int)getResp.StatusCode} {getResp.ReasonPhrase}. Body: {getBody}");

            using var getDoc = JsonDocument.Parse(getBody);
            return MapUser(getDoc.RootElement);
        }

        using var doc = JsonDocument.Parse(body);
        return MapUser(doc.RootElement);
    }


    private static GraphUserDto MapUser(JsonElement root) => new()
    {
        Id = root.GetProperty("id").GetString() ?? string.Empty,
        UserPrincipalName = root.GetProperty("userPrincipalName").GetString() ?? string.Empty,
        DisplayName = root.GetProperty("displayName").GetString() ?? string.Empty,
        Mail = root.TryGetProperty("mail", out var mailProp) ? mailProp.GetString() : null,
        JobTitle = root.TryGetProperty("jobTitle", out var jobProp) ? jobProp.GetString() : null,
        MobilePhone = root.TryGetProperty("mobilePhone", out var mobProp) ? mobProp.GetString() : null,
        OfficeLocation = root.TryGetProperty("officeLocation", out var offProp) ? offProp.GetString() : null,
        BusinessPhones = root.TryGetProperty("businessPhones", out var bpProp) &&
                            bpProp.ValueKind == JsonValueKind.Array
            ? bpProp.EnumerateArray().Select(p => p.GetString() ?? string.Empty).ToArray()
            : System.Array.Empty<string>()
    };
}
