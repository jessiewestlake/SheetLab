# ServiceNow AD Unlock Semantic Kernel Workflow

This repository packages design guidance, a C# starter host, and PowerShell tooling for automating ServiceNow AD unlock requests using Microsoft Semantic Kernel (SK) agents.

## Components
- `docs/semantic-kernel-servicenow.md` — architecture and workflow overview.
- `src/ServiceNowUnlockWorkflow/` — .NET 8 console host that wires up Semantic Kernel agents.
- `scripts/UnlockAccount.ps1` — hardened PowerShell entry point for executing unlocks on a Windows server.
- `.github/workflows/dotnet-build.yml` — CI template to build and lint the solution.

## Getting Started
1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download) and PowerShell 7 on the development workstation.
2. Restore dependencies: `dotnet restore src/ServiceNowUnlockWorkflow/ServiceNowUnlockWorkflow.csproj`.
3. Configure `appsettings.json` (create alongside the project) with ServiceNow, PowerShell, and SMTP settings referencing secret names only.
4. Deploy `scripts/UnlockAccount.ps1` to the execution host and register required secrets with PowerShell SecretManagement.
5. Run locally with `dotnet run --project src/ServiceNowUnlockWorkflow/ServiceNowUnlockWorkflow.csproj`.

## Secrets
Secrets are never stored in source control. Populate vaults using:
```powershell
Register-SecretVault -Name UnlockVault -ModuleName Microsoft.PowerShell.SecretStore
Set-Secret -Name ServiceNowBasicAuth -Secret (Get-Credential)
Set-Secret -Name AdServiceAccount -Secret (Get-Credential)
```

## Extending
- Swap the PowerShell process call with an MCP server endpoint once available.
- Add telemetry and resilience policies (retry, circuit breaker) to ServiceNow interactions.
- Integrate unit tests using xUnit + Moq.

See `AGENTS.md` for repository guidance tailored for Codex agents.
