using System;
using System.Management.Automation;
using PsGraphUtility.Auth;
using PsGraphUtility.Graph.Entra.Devices.Interfaces;

namespace PsGraphUtility.PowerShell.Devices
{
    public abstract class DeviceCmdletBase : PSCmdlet
    {
        protected GraphAuthContext RequireContext()
        {
            var ctx = GlobalServices.AuthOrchestrator.GetDefaultContext();
            if (ctx is null)
                throw new InvalidOperationException(
                    "No active Graph session. Run Connect-GphTenant first.");
            return ctx;
        }

        protected IGetDeviceService GetDevices =>
            GlobalServices.DeviceService
            ?? throw new InvalidOperationException("DeviceService is not initialised.");

        protected ISetDeviceService SetDevices =>
            GlobalServices.SetDeviceService
            ?? throw new InvalidOperationException("SetDeviceService is not initialised.");
    }
}
