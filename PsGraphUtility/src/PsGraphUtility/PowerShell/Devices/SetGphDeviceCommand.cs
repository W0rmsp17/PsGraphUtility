using System.Management.Automation;
using PsGraphUtility.Graph.Entra.Devices.Models;

namespace PsGraphUtility.PowerShell.Devices
{
    [Cmdlet(VerbsCommon.Set, "GphDevice")]
    [OutputType(typeof(GraphDeviceDto))]
    public sealed class SetGphDeviceCommand : DeviceCmdletBase
    {
        // AAD device id (same value from Get-GphDevice | select Id)
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Device { get; set; } = null!;

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string? DisplayName { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public bool AccountEnabled { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var bp = MyInvocation.BoundParameters;
            var hasUpdate =
                bp.ContainsKey(nameof(DisplayName)) ||
                bp.ContainsKey(nameof(AccountEnabled));

            if (!hasUpdate)
            {
                WriteWarning("No updatable properties were specified. Nothing to change.");
                return;
            }

            bool? accountEnabled = bp.ContainsKey(nameof(AccountEnabled))
                ? AccountEnabled
                : (bool?)null;

            var ctx = RequireContext();

            var updated = SetDevices
                .UpdateDeviceAsync(
                    ctx,
                    Device,
                    displayName: DisplayName,
                    accountEnabled: accountEnabled)
                .GetAwaiter()
                .GetResult();

            WriteObject(updated);
        }
    }
}
