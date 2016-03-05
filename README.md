To install the C28 Transport Agent

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

Create a directory wherever you want (it is suggested to use ```C:\Program Files\C28ExchangeAgent\``` as it will be used throughout this guide).

Clone the git repository content in that folder.

Add **total control permissions** to the Network Service group to the newly created directory.

(The exact english name is NETWORK SERVICE)

*(Le nom en français est SERVICE RESEAU)*

but really just type in "SERVICE" and hit the "Verify Names..." or "Vérifier les noms..." button and select the NETWORK SERVICE user/group.

##### 3.2 Creating the environment variable

Create a *system environment variable* as follows:

``` 
Name: C28AgentInstallDir
Value: C:\Program Files\C28ExchangeAgent (or your installation path)
```

##### 3.2 Installing the new transport agent

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







