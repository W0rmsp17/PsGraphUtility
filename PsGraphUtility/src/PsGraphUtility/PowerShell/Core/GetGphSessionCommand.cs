using System;
using System.Linq;
using System.Threading;
using System.Management.Automation;

using PsGraphUtility.Auth;
using PsGraphUtility.PowerShell.Core;   
using PsGraphUtility.PowerShell;       

namespace PsGraphUtility.PowerShell.Core;

[Cmdlet(VerbsCommon.Get, "GphSession")]
[OutputType(typeof(GphSessionInfo))]
public sealed class GetGphSessionCommand : PSCmdlet
{
    [Parameter]
    public SwitchParameter Default { get; set; }

    protected override void ProcessRecord()
    {
        IAuthOrchestrator auth = GlobalServices.AuthOrchestrator;

        var contexts = Default.IsPresent
            ? auth.GetDefaultContext() is { } d ? new[] { d } : Array.Empty<GraphAuthContext>()
            : auth.GetAllContexts().ToArray();

        foreach (var ctx in contexts)
        {
            WriteObject(new GphSessionInfo
            {
                Id = ctx.Id,
                Name = ctx.Name,
                TenantId = ctx.TenantId,
                AuthMode = ctx.AuthMode.ToString(),
                IsDefault = ctx.IsDefault
            });
        }
    }
}
