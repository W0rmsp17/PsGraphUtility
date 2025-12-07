using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Entra.Devices.Models;

namespace PsGraphUtility.Graph.Entra.Devices.Interfaces
{
    public interface IGetDeviceService
    {
        Task<GraphDeviceDto> GetDeviceAsync(
            GraphAuthContext context,
            string deviceKey,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<GraphDeviceDto>> ListDevicesAsync(
            GraphAuthContext context,
            bool enabledOnly = false,
            CancellationToken cancellationToken = default);
    }
}
