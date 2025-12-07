using PsGraphUtility.Auth;
using PsGraphUtility.PowerShell;
using System.Management.Automation;
using PsGraphUtility.Graph.Entra.Devices;
using PsGraphUtility.Graph.Entra.SignIn.Models;

namespace PsGraphUtility.PowerShell.SignIn
{
    [Cmdlet(VerbsCommon.Get, "GphSignIn")]
    [OutputType(typeof(GraphSignInDto))]
    public sealed class GetGphSignInCommand : PSCmdlet
    {
        [Parameter(Mandatory = false)]
        [ValidateRange(0, 30)]
        public int? Day { get; set; }


        [Parameter(Mandatory = false)]
        public string? User { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var ctx = GlobalServices.AuthOrchestrator.GetDefaultContext()
                      ?? throw new InvalidOperationException(
                          "No active Graph session. Run Connect-GphTenant first.");

            var svc = GlobalServices.SignInLogService; 

            var logs = svc
                .GetSignInsAsync(ctx, Day, User)
                .GetAwaiter()
                .GetResult();

            WriteObject(logs, enumerateCollection: true);
        }
    }
}
