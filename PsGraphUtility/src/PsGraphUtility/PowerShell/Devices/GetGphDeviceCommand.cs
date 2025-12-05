using System.Management.Automation;
using PsGraphUtility.Graph.Device.Models;

namespace PsGraphUtility.PowerShell.Devices
{
    [Cmdlet(VerbsCommon.Get, "GphDevice")]
    [OutputType(typeof(GraphDeviceDto))]
    public sealed class GetGphDeviceCommand : DeviceCmdletBase
    {
        // For now, treat this as the device Id (we can add name lookup later)
        [Parameter(Position = 0)]
        public string? Device { get; set; }

        [Parameter]
        public SwitchParameter EnabledOnly { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var ctx = RequireContext();

            if (!string.IsNullOrWhiteSpace(Device))
            {
                var d = GetDevices
                    .GetDeviceAsync(ctx, Device)
                    .GetAwaiter()
                    .GetResult();

                WriteObject(d);
                return;
            }

            var list = GetDevices
                .ListDevicesAsync(ctx, EnabledOnly.IsPresent)
                .GetAwaiter()
                .GetResult();

            WriteObject(list, enumerateCollection: true);
        }
    }
}
