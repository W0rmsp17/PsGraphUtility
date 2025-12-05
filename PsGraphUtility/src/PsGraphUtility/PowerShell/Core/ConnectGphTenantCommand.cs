using System;
using System.Threading;
using System.Management.Automation;
using PsGraphUtility.Auth;
using PsGraphUtility.PowerShell;
using PsGraphUtility.PowerShell.Core;
using PsGraphUtility.Auth;

namespace PsGraphUtility.PowerShell.Core;

[Cmdlet(VerbsCommunications.Connect, "GphTenant")]
[OutputType(typeof(GphSessionInfo))]
public sealed class ConnectGphTenantCommand : PSCmdlet
{
    [Parameter(Mandatory = false)]
    public string? Name { get; set; }

    protected override void ProcessRecord()
    {
        var auth = GlobalServices.AuthOrchestrator;

        var ctx = auth
            .ConnectUserDelegatedAsync(
                tenantId: null,
                displayName: Name,
                cancellationToken: CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        var bootstrap = GlobalServices.BootstrapService
            .EnsureBootstrapAsync(ctx, CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        var info = new GphSessionInfo
        {
            Id = ctx.Id,
            Name = ctx.Name,
            TenantId = ctx.TenantId,
            AuthMode = ctx.AuthMode.ToString(),
            IsDefault = ctx.IsDefault
        };

        WriteObject(info);
    }
}
