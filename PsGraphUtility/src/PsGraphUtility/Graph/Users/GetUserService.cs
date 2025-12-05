using PsGraphUtility.Auth;
using PsGraphUtility.Graph;
using PsGraphUtility.Graph.HTTP;
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

public sealed class GetUserService : IGetUserService
{
    private readonly IGraphClient _graph;
    private readonly IUserLookupService _lookup;

    public GetUserService(IGraphClient graph, IUserLookupService lookup)
    {
        _graph = graph;
        _lookup = lookup;
    }

    public async Task<GraphUserDto> GetUserAsync(
        GraphAuthContext context,
        string userIdOrUpn,
        CancellationToken cancellationToken = default)
    {
        var userId = await _lookup.ResolveUserIdAsync(
            context, userIdOrUpn, cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            UserRoutes.UserById(userId));

        var response = await _graph.SendAsync(context, request, cancellationToken)
                                   .ConfigureAwait(false);
     //   var body = await response.Content.ReadAsStringAsync(cancellationToken)
     //                                    .ConfigureAwait(false);

        var body = await GraphResponseHelper.EnsureSuccessAsync(
            response,
            "some operation description",
            cancellationToken);

        return MapUser(JsonDocument.Parse(body).RootElement);
    }

    public async Task<IReadOnlyList<GraphUserDto>> ListUsersAsync(
        GraphAuthContext context,
        CancellationToken cancellationToken = default)
    {
        var url = UserRoutes.Users() +
                  "?$select=id,displayName,userPrincipalName,mail,jobTitle,mobilePhone,officeLocation,businessPhones&$top=200";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await _graph.SendAsync(context, request, cancellationToken)
                                   .ConfigureAwait(false);
   //     var body = await response.Content.ReadAsStringAsync(cancellationToken)
   //                                      .ConfigureAwait(false);

        var body = await GraphResponseHelper.EnsureSuccessAsync(
            response,
            "some operation description",
            cancellationToken);

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("value")
                              .EnumerateArray()
                              .Select(MapUser)
                              .ToList();
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
