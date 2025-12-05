using PsGraphUtility.Auth;
using PsGraphUtility.Graph.SignIn.Models;
using PsGraphUtility.PowerShell;
using System.Management.Automation;
using PsGraphUtility.Graph.SignIn;

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
