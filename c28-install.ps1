$url = "https://github.com/sprintmarketing/c28exchange-agent/archive/master.zip"
$out = "$env:C28AgentInstallDir\master.zip"
$agent_loc = "$env:C28AgentInstallDir"
$agent_dir = "$agent_loc\C28ExchangeAgent-master\"

Add-Type -AssemblyName System.IO.Compression.FileSystem
function Unzip
{
    param([string]$zipfile, [string]$outpath)

    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath)
}

Write-Host "Uninstalling previous installed agents"
Uninstall-TransportAgent "C28 Rewriting Agent" -Confirm:$false
Uninstall-TransportAgent "C28 Transport Agent" -Confirm:$false

Write-Host "Stopping MSExchangeTransport service"
Stop-Service -Name "Microsoft Exchange Transport"

Write-Host "Resetting IIS services"
iisreset.exe

Write-Host "Deleting old files"
Remove-Item "$agent_dir" -Force -Recurse -ErrorAction SilentlyContinue

Write-Host "Downloading latest version from GitHub..."
Invoke-WebRequest -Uri $url -OutFile $out

Write-Host "Unzipping to agent directory"
Unzip $out $agent_loc

Write-Host "Installing agents"
Install-TransportAgent -Name "C28 Rewriting Agent" -TransportAgentFactory SprintMarketing.C28.ExchangeAgent.C28RewritingFactory -AssemblyPath "$agent_dir\binaries\SprintMarketing.C28.ExchangeAgent.dll"  -Confirm:$false
Install-TransportAgent -Name "C28 Transport Agent" -TransportAgentFactory SprintMarketing.C28.ExchangeAgent.C28AgentFactory -AssemblyPath "$agent_dir\binaries\SprintMarketing.C28.ExchangeAgent.dll"  -Confirm:$false

Set-TransportAgent -Priority 1 "C28 Rewriting Agent" -Confirm:$false
Set-TransportAgent -Priority 2 "C28 Transport Agent" -Confirm:$false

Get-TransportAgent "C28 Rewriting Agent" | Enable-TransportAgent -Confirm:$false
Get-TransportAgent "C28 Transport Agent" | Enable-TransportAgent -Confirm:$false

iisreset.exe
Start-Service -Name "Microsoft Exchange Transport"
