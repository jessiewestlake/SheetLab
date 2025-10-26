# Semantic Kernel Multi-Agent Workflow for ServiceNow AD Unlocks

This document summarizes the architecture and operational flow for the ServiceNow Active Directory (AD) account unlock automation built on Microsoft Semantic Kernel (SK).

## Goals
- Monitor ServiceNow incident tickets for "AD account unlock" requests.
- Coordinate PowerShell tooling on a secure Windows host to unlock accounts.
- Notify requesters via SMTP relay when the operation completes.
- Store and retrieve secrets using PowerShell SecretManagement / SecretStore (with fallbacks).

## Reference Material
- [Microsoft Semantic Kernel documentation](https://learn.microsoft.com/semantic-kernel/) covers agent orchestration, planner APIs, and connector integration.
- [ServiceNow Table API reference](https://developer.servicenow.com/dev.do#!/reference/api/sandiego/rest/c_TableAPI) for incident polling.
- [SecretManagement module](https://learn.microsoft.com/powershell/module/microsoft.powershell.secretmanagement/) for secret retrieval.

## Agent Topology
1. **Incident Monitor Agent** (C#, SK Planner): Polls ServiceNow, classifies incidents, enqueues unlock jobs.
2. **Unlock Coordinator Agent** (C#, SK Orchestrator): Consumes jobs, gathers context, chooses execution path.
3. **PowerShell Executor Agent** (future MCP server, interim local process): Performs unlock using PowerShell remoting.
4. **Notification Agent** (C#, SK Function): Sends completion status via SMTP.

The initial release implements the first three using SK Agents and a hardened local PowerShell script. The MCP PowerShell server remains an optional enhancement.

## Workflow Sequence
1. Incident Monitor queries ServiceNow incidents filtered by category/subcategory and state.
2. Matching incidents are published to a queue (in-memory or persistent).
3. Unlock Coordinator pulls a job, requests secrets (ServiceNow credentials, AD service account) through the Secret Store bridge, and dispatches to the PowerShell executor.
4. PowerShell script runs on the Windows host, unlocking the account and returning status JSON.
5. Notification Agent emails requester and updates ServiceNow incident notes/state.

## Secrets Strategy
- Preferred: PowerShell SecretManagement with SecretStore vault on Windows server.
- Fallback: DPAPI protected `Microsoft.Extensions.Configuration.UserSecrets` or Windows Credential Manager accessed through C#.
- Configuration file `appsettings.json` references secret names only.

## Deployment Notes
- Run the C# host as a Windows service or scheduled task with `dotnet run`/single-file publish.
- Ensure TLS trust for ServiceNow and SMTP endpoints.
- Harden PowerShell execution via Constrained Language Mode and code signing if available.

## Next Steps
- Replace local PowerShell invocation with MCP server once available.
- Add unit tests for agent planners and ServiceNow client wrappers.
- Integrate telemetry (Application Insights, Prometheus) for run visibility.
