# C28 Exchange Transport Agent

#### 1. Make sure to uninstall previously installed version of the C28 Agent

**From the Exchange PowerShell:**

Issue 

``` 
Get-TransportAgent
```

 and check whether there is an entry named "C28 Transport Agent"

*if there is no such entry, skip to step 3*

#### 2. Removing the installed TransportAgent

**From the Exchange PowerShell**:

Disable the active transport agent by typing 

``` 
Get-TransportAgent "C28 Transport Agent" | Disable-TransportAgent
```

Uninstall the active transport agent by typing

``` 
Uninstall-TransportAgent "C28 Transport Agent"
```

You may me prompted to confirm the removal of the agent.

*** Retart your PowerShell and the MSExchangeTransport Service ***

#### 3. Installing the C28 Transport Agent

##### 3.1 Creating the directory copying files and adjusting permissions

Create a directory named `C28` in the exchange transport agents directory so that the full path to the C28 Transport Agent DLLs looks like this :

``` 
C:\Program Files\Microsoft\Exchange Server\V<exchange version>\TransportRoles\agents\C28
```

Clone the git repository *content* in that folder.

##### 3.2 Creating the environment variable

Create a *system environment variable* as follows:

``` 
Name: C28AgentInstallDir
Value: C:\Program Files\Microsoft\Exchange Server\V<exchange version>\TransportRoles\agents\C28
```

##### 3.3 (IMPORTANT) Unlock the DLL for execution (Windows Server feature only?)

Windows Server seems to have some enhanced security features that prevent DLLs (unsigned?) from being executed. **Therefore, you need to unlock the C28 Transport Agent**:

You can unlock a DLL by going in the File Properties dialog of that file and by clicking the "Unlock" ("Débloquer") button at the bottom of the dialog box. Do so for the C28 Transport Agent DLL file located at:

``` 
$env:C28AgentInstallDir\binaries\SprintMarketing.C28.ExchangeAgent.dll
```

##### 3.5 Restart the IIS Services

Issue the 

``` 
iisreset.exe
```

command to restart the IIS Services so that the new DLL gets loaded.

##### 3.4 Installing the new transport agent

**From an Exchange PowerShell:**

Issue the following command to install the agent

``` 
Install-TransportAgent -Name "C28 Transport Agent" -TransportAgentFactory SprintMarketing.C28.ExchangeAgent.C28AgentFactory -AssemblyPath "$env:C28AgentInstallDir\binaries\SprintMarketing.C28.ExchangeAgent.dll"
```

You should be prompted to confirm the agent installation

Issue `Get-TransportAgent` to make sure the agent has been properly installed.

##### 3.3 Enabling and changing the agent priority

Issue the following to change the priority of the transport agent as the **first** transport agent:

``` 
Set-TransportAgent -Priority 1 "C28 Transport Agent"
```

Then issue the following to enable the transport agent:

``` 
Get-TransportAgent "C28 Transport Agent" | Enable-TransportAgent
```

*** Retart your PowerShell and the MSExchangeTransport Service ***

#### 4. Configure config.json

Open the file `config.json` and configure it to suit your needs.

``` 
{
  "fetch_api_key": "<your api key>",
  "fetch_interval_min": 20,
  "fetch_eager":  false,
  "fetch_url": "",
  "fetch_cache_file": "cache.json",
  "log_file": "c28.log",
  "log_level": "All",
  "log_max_size":  "20MB"
}
```

* **fetch_api_key**: your api key (contact us)
* **fetch_interval_min**: number of mins elapsed to update the cache
* **fetch_eager**: always fetch the domain data from the remote api (*Warning*: this will fetch domain data from the api for every incoming email -not recommended)
* **fetch_url**: http(s) base api uri where to fetch domain data (contact us)
* **fetch_cache_file**: path where to store the cached domain data
* **log_file**: path where to store the log file
* **log_level**: one of `all|off|debug|info|warn|error|fatal`
* **log_max_size**: max log size before the rotating log file

The `config.json`, `cache.json` and `c28.log` file paths are not absolute file paths. Those files will be created at the root of your environment variable `$C28AgentInstallDir`. 

Please contact us to get your **api_key** and the **fetch_url** url.

# Troubleshooting





