$here = Split-Path -Parent $PSCommandPath
Add-Type -Path (Join-Path $here '..\..\bin\Debug\net8.0\PsGraphUtility.dll')

Export-ModuleMember -Cmdlet @(
    'Connect-GphTenant',
    'Get-GphSession',
    'Get-GphUser',
    'Set-GphUserContext',
    'Set-GphUser'
    'Set-GphGroup'
)
