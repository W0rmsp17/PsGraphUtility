// src/PsGraphUtility/Bootstrap/BootstrapService.cs
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.HTTP;
using PsGraphUtility.Routes.Account;
using PsGraphUtility.Routes.Users;
using PsGraphUtility.Graph.Interface;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PsGraphUtility.Bootstrap;

public sealed class BootstrapService : IBootstrapService
{
    private readonly string _cachePath;
    private readonly IGraphClient _graph;

    public BootstrapService(IGraphClient graphClient)
    {
        _graph = graphClient ?? throw new ArgumentNullException(nameof(graphClient));

        var basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PsGraphUtility");

        Directory.CreateDirectory(basePath);
        _cachePath = Path.Combine(basePath, "bootstrap.json");
    }

    public async Task<BootstrapProfile> EnsureBootstrapAsync(
        GraphAuthContext context,
        CancellationToken cancellationToken = default)
    {
        var existing = await LoadProfileAsync(cancellationToken);
        if (existing is not null &&
            existing.TenantId.Equals(context.TenantId, StringComparison.OrdinalIgnoreCase))
        {
            return existing;
        }

        var created = await CreateBootstrapProfileAsync(context, cancellationToken);
        await SaveProfileAsync(created, cancellationToken);
        return created;
    }

    private async Task<BootstrapProfile?> LoadProfileAsync(CancellationToken ct)
    {
        if (!File.Exists(_cachePath)) return null;

        var json = await File.ReadAllTextAsync(_cachePath, ct);
        return JsonSerializer.Deserialize<BootstrapProfile>(json);
    }

    private async Task SaveProfileAsync(BootstrapProfile profile, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_cachePath, json, ct);
    }

    private async Task<BootstrapProfile> CreateBootstrapProfileAsync(
        GraphAuthContext context,
        CancellationToken ct)
    {
        var (appId, appObjectId, appDisplayName) =
            await CreateBootstrapAppAsync(context, ct);

        var (svcUpn, svcObjectId) =
            await CreateServiceAccountAsync(context, ct);

        return new BootstrapProfile
        {
            TenantId = context.TenantId,
            AppId = appId,
            AppObjectId = appObjectId,
            AppDisplayName = appDisplayName,
            ServiceAccountUpn = svcUpn,
            ServiceAccountObjectId = svcObjectId,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    private async Task<(string appId, string objectId, string displayName)> CreateBootstrapAppAsync(
        GraphAuthContext context,
        CancellationToken ct)
    {
        var payload = new
        {
            displayName = "PsGraphUtility-Bootstrap",
            signInAudience = "AzureADMultipleOrgs"
        };

        var json = JsonSerializer.Serialize(payload);
        using var request = new HttpRequestMessage(HttpMethod.Post, AccountRoutes.Applications())
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await _graph.SendAsync(context, request, ct).ConfigureAwait(false);
       // var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        var body = await GraphResponseHelper.EnsureSuccessAsync(
            response,
            "list users",
            ct);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var appId = root.GetProperty("appId").GetString() ?? string.Empty;
        var objectId = root.GetProperty("id").GetString() ?? string.Empty;
        var displayName = root.GetProperty("displayName").GetString() ?? string.Empty;

        return (appId, objectId, displayName);
    }

    private async Task<(string upn, string objectId)> CreateServiceAccountAsync(
        GraphAuthContext context,
        CancellationToken ct)
    {
        var defaultDomain = await GetDefaultDomainAsync(context, ct).ConfigureAwait(false);
        var upnLocalPart = "gph-bootstrap";
        var upn = $"{upnLocalPart}@{defaultDomain}";

        var password = GenerateRandomPassword(20);

        var payload = new
        {
            accountEnabled = true,
            displayName = "PsGraphUtility Service",
            mailNickname = upnLocalPart,
            userPrincipalName = upn,
            passwordProfile = new
            {
                forceChangePasswordNextSignIn = false,
                password = password
            }
        };

        var json = JsonSerializer.Serialize(payload);
        using var request = new HttpRequestMessage(HttpMethod.Post, UserRoutes.Users())
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await _graph.SendAsync(context, request, ct).ConfigureAwait(false);
       // var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        var body = await GraphResponseHelper.EnsureSuccessAsync(
            response,
            "some operation description",
            ct);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var objectId = root.GetProperty("id").GetString() ?? string.Empty;

        return (upn, objectId);
    }

    private async Task<string> GetDefaultDomainAsync(
    GraphAuthContext context,
    CancellationToken ct)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            AccountRoutes.Domains());  

        var response = await _graph.SendAsync(context, request, ct).ConfigureAwait(false);
      //  var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        var body = await GraphResponseHelper.EnsureSuccessAsync(
            response,
            "some operation description",
            ct);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var first = root.GetProperty("value")
            .EnumerateArray()
            .FirstOrDefault(e =>
                e.ValueKind == JsonValueKind.Object &&
                e.TryGetProperty("isDefault", out var d) &&
                d.ValueKind == JsonValueKind.True);

        var id = first.ValueKind == JsonValueKind.Object
            ? first.GetProperty("id").GetString()
            : null;

        if (string.IsNullOrWhiteSpace(id))
            throw new GraphAuthException("No default domain found for tenant.");

        return id!;
    }



    private static string GenerateRandomPassword(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*_-+";
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);

        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[bytes[i] % chars.Length];
        }
        return new string(result);
    }
}
