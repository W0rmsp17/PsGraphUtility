using System;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PsGraphUtility.Auth.Powershell
{
    internal static class ExchangeSessionManager
    {
        private static readonly SemaphoreSlim _lock = new(1, 1);
        private static System.Management.Automation.PowerShell? _ps;

        internal static async Task RunInExchangeSessionAsync(
            Func<System.Management.Automation.PowerShell, Task> action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            await _lock.WaitAsync().ConfigureAwait(false);

            try
            {
                _ps ??= System.Management.Automation.PowerShell.Create();

                if (!IsConnectedToExchangeOnline(_ps))
                {
                    _ps.Commands.Clear();
                    _ps.AddCommand("Connect-ExchangeOnline");
                    _ps.Invoke();
                    _ps.Commands.Clear();

                    if (_ps.HadErrors)
                        throw new InvalidOperationException(
                            "Failed to connect to Exchange Online. " +
                            "Ensure ExchangeOnlineManagement is installed and you can connect interactively.");
                }

                await action(_ps).ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
        }

        private static bool IsConnectedToExchangeOnline(System.Management.Automation.PowerShell ps)
        {
            ps.Commands.Clear();
            ps.AddCommand("Get-PSSession")
              .AddParameter("ConfigurationName", "Microsoft.Exchange");

            var sessions = ps.Invoke();
            ps.Commands.Clear();

            return sessions.Any();
        }
    }
}
