using System.Threading;
using System.Threading.Tasks;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Entra.Devices.Models;

namespace PsGraphUtility.Graph.Entra.Devices.Interfaces
{
    public interface ISetDeviceService
    {
        Task<GraphDeviceDto> UpdateDeviceAsync(
            GraphAuthContext context,
            string deviceId,
            string? displayName = null,
            bool? accountEnabled = null,
            CancellationToken cancellationToken = default);
    }
}
