using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.HTTP;
using PsGraphUtility.Routes.Devices;
using PsGraphUtility.Graph.Entra.Devices.Interfaces;
using PsGraphUtility.Graph.Entra.Devices.Models;
using PsGraphUtility.Graph.Interface;

namespace PsGraphUtility.Graph.Entra.Devices
{
    public sealed class SetDeviceService : ISetDeviceService
    {
        private readonly IGraphClient _graph;
        private readonly IGetDeviceService _getter;

        public SetDeviceService(IGraphClient graph, IGetDeviceService getter)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _getter = getter ?? throw new ArgumentNullException(nameof(getter));
        }

        public async Task<GraphDeviceDto> UpdateDeviceAsync(
            GraphAuthContext context,
            string deviceId,
            string? displayName = null,
            bool? accountEnabled = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("Device id must be provided.", nameof(deviceId));

            var payload = new Dictionary<string, object>();

            if (displayName is not null) payload["displayName"] = displayName;
            if (accountEnabled.HasValue) payload["accountEnabled"] = accountEnabled.Value;

            if (payload.Count == 0)
                throw new GraphAuthException("No fields provided to update.");

            var json = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(
                HttpMethod.Patch,
                DeviceRoutes.Device(deviceId))
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _graph.SendAsync(context, request, cancellationToken)
                                       .ConfigureAwait(false);

            await GraphResponseHelper.EnsureSuccessAsync(
                response,
                $"update device '{deviceId}'",
                cancellationToken);

            return await _getter.GetDeviceAsync(context, deviceId, cancellationToken)
                                .ConfigureAwait(false);
        }
    }
}
