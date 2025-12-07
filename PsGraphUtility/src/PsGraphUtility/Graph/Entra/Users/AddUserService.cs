//path src/PsGraphUtility/Graph/Users/AddUserService.cs
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.HTTP;
using PsGraphUtility.Routes.Users;
using PsGraphUtility.Graph.Entra.Users.Interface;
using PsGraphUtility.Graph.Entra.Users.Models;
using PsGraphUtility.Graph.Interface;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PsGraphUtility.Graph.Entra.Users;

public sealed class AddUserService : IAddUserService
{
    private readonly IGraphClient _graph;

    public AddUserService(IGraphClient graph) => _graph = graph;

    public async Task<GraphUserDto> CreateUserAsync(
        GraphAuthContext context,
        string userPrincipalName,
        string displayName,
        string? mailNickname = null,
        string? password = null,
        bool accountEnabled = true,
        bool forceChangePasswordNextSignIn = true,
        bool forceChangePasswordNextSignInWithMfa = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mailNickname))
        {
            var at = userPrincipalName.IndexOf('@');
            mailNickname = at > 0 ? userPrincipalName[..at] : userPrincipalName;
        }

        password ??= "P@ssw0rd!" + Guid.NewGuid().ToString("N")[..6];

        var payload = new
        {
            accountEnabled,
            displayName,
            mailNickname,
            userPrincipalName,
            passwordProfile = new
            {
                forceChangePasswordNextSignIn,
                forceChangePasswordNextSignInWithMfa,
                password
            }
        };

        var json = JsonSerializer.Serialize(payload);
        using var request = new HttpRequestMessage(HttpMethod.Post, UserRoutes.Users())
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await _graph.SendAsync(context, request, cancellationToken)
                                   .ConfigureAwait(false);
        //    var body = await response.Content.ReadAsStringAsync(cancellationToken)
        //                                     .ConfigureAwait(false);

        var body = await GraphResponseHelper.EnsureSuccessAsync(
            response,
            $"create user '{userPrincipalName}'",
            cancellationToken);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        return new GraphUserDto
        {
            Id = root.GetProperty("id").GetString() ?? string.Empty,
            UserPrincipalName = root.GetProperty("userPrincipalName").GetString() ?? string.Empty,
            DisplayName = root.GetProperty("displayName").GetString() ?? string.Empty,
            Mail = root.TryGetProperty("mail", out var mailProp) ? mailProp.GetString() : null
        };
    }
}
