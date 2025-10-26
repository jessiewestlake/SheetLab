using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceNowUnlockWorkflow.Configuration;

namespace ServiceNowUnlockWorkflow.Services;

public class PowerShellUnlockService
{
    private readonly AppSettings _settings;
    private readonly SecretStoreBridge _secretStore;
    private readonly ILogger<PowerShellUnlockService> _logger;

    public PowerShellUnlockService(IOptions<AppSettings> settings, SecretStoreBridge secretStore, ILogger<PowerShellUnlockService> logger)
    {
        _settings = settings.Value;
        _secretStore = secretStore;
        _logger = logger;
    }

    public async Task<UnlockResult> UnlockAccountAsync(string userPrincipalName, CancellationToken cancellationToken)
    {
        var scriptPath = _settings.PowerShell.ScriptPath;
        var serviceAccountSecret = await _secretStore.GetSecretAsync(_settings.PowerShell.ServiceAccountSecretName, cancellationToken);

        var psi = new ProcessStartInfo
        {
            FileName = "pwsh",
            ArgumentList =
            {
                "-File",
                scriptPath,
                "-UserPrincipalName", userPrincipalName,
                "-ServiceAccountSecret", serviceAccountSecret
            },
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        _logger.LogInformation("Invoking PowerShell unlock for {User}", userPrincipalName);
        using var process = Process.Start(psi);
        if (process is null)
        {
            return new UnlockResult(false, "Failed to start PowerShell process");
        }

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await Task.WhenAll(outputTask, errorTask, process.WaitForExitAsync(cancellationToken));
        var output = outputTask.Result;
        var error = errorTask.Result;

        if (process.ExitCode == 0)
        {
            return new UnlockResult(true, output.Trim());
        }

        _logger.LogError("PowerShell unlock failed for {User}: {Error}", userPrincipalName, error);
        return new UnlockResult(false, error.Trim());
    }
}
