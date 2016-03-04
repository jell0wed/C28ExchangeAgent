To install the C28 Transport Agent

#### 1. Make sure to uninstall previously installed version of the C28 Agent

**From the Exchange PowerShell:**

Issue 

``` 
Get-TransportAgent
```

 and check whether there is an entry named "C28 Transport Agent"

*if there is no such entry, skip to step X*

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

##### 3.1 Creating the directory copying files

Create a directory wherever you want (it is suggested to use ```C:\Program Files\C28ExchangeAgent\``` as it will be used throughout this guide).

Clone the git repository content in that folder.

##### 3.2 Creating the environment variable

Create a *system environment variable* as follows:

``` 
Name: C28AgentInstallDir
Value: C:\Program Files\C28ExchangeAgent\ (or your installation path)
```

##### 3.2 Installing the new transport agent

**From an Exchange PowerShell:**

Issue the following commnad to install the agent

``` 
Install-TransportAgent -Name "C28 Transport Agent" -TransportAgentFactory 
```









