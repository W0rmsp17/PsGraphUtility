using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Routes.Devices;
using PsGraphUtility.Graph.HTTP;
using PsGraphUtility.Graph.Entra.Devices.Interfaces;
using PsGraphUtility.Graph.Entra.Devices.Models;
using PsGraphUtility.Graph.Interface;

namespace PsGraphUtility.Graph.Entra.Devices
{
    public sealed class GetDeviceService : IGetDeviceService
    {
        private readonly IGraphClient _graph;

        public GetDeviceService(IGraphClient graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        }

        public async Task<GraphDeviceDto> GetDeviceAsync(
            GraphAuthContext context,
            string deviceKey,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                DeviceRoutes.Device(deviceKey));

            var response = await _graph.SendAsync(context, request, cancellationToken);
            var body = await GraphResponseHelper.EnsureSuccessAsync(
                response,
                $"get device '{deviceKey}'",
                cancellationToken);

            using var doc = JsonDocument.Parse(body);
            return MapDevice(doc.RootElement);
        }

        public async Task<IReadOnlyList<GraphDeviceDto>> ListDevicesAsync(
            GraphAuthContext context,
            bool enabledOnly = false,
            CancellationToken cancellationToken = default)
        {
            var url = enabledOnly
                ? DeviceRoutes.ListEnabled()
                : DeviceRoutes.ListAll();

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _graph.SendAsync(context, request, cancellationToken);

            var body = await GraphResponseHelper.EnsureSuccessAsync(
                response,
                $"list devices",
                cancellationToken);

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("value")
                .EnumerateArray()
                .Select(MapDevice)
                .ToList();
        }

        private static GraphDeviceDto MapDevice(JsonElement e) => new()
        {
            Id = e.GetProperty("id").GetString() ?? "",
            DisplayName = e.TryGetProperty("displayName", out var dn) ? dn.GetString() : null,
            DeviceId = e.TryGetProperty("deviceId", out var did) ? did.GetString() : null,
            OperatingSystem = e.TryGetProperty("operatingSystem", out var os) ? os.GetString() : null,
            OperatingSystemVersion = e.TryGetProperty("operatingSystemVersion", out var osv) ? osv.GetString() : null,
            AccountEnabled = e.TryGetProperty("accountEnabled", out var en) ? en.GetBoolean() : null,
            ApproximateLastSignInDateTime =
                e.TryGetProperty("approximateLastSignInDateTime", out var si) && si.ValueKind == JsonValueKind.String
                    ? si.GetDateTimeOffset()
                    : null,
            DeviceCategory = e.TryGetProperty("deviceCategory", out var dc) ? dc.GetString() : null,
            DeviceOwnership = e.TryGetProperty("deviceOwnership", out var dox) ? dox.GetString() : null,
            TrustType = e.TryGetProperty("trustType", out var tt) ? tt.GetString() : null,
            ManagementType = e.TryGetProperty("managementType", out var mt) ? mt.GetString() : null,
            ComplianceState = e.TryGetProperty("complianceState", out var cs) ? cs.GetString() : null
        };
    }
}
