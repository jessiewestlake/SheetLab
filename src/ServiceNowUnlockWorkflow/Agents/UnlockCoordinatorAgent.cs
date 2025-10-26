using System.Collections.Concurrent;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using ServiceNowUnlockWorkflow.Services;

namespace ServiceNowUnlockWorkflow.Agents;

public class UnlockCoordinatorAgent
{
    private readonly ConcurrentQueue<ServiceNowIncident> _queue = new();
    private readonly PowerShellUnlockService _unlockService;
    private readonly SmtpNotificationService _notificationService;
    private readonly ServiceNowClient _serviceNowClient;
    private readonly Kernel _kernel;

    public UnlockCoordinatorAgent(
        PowerShellUnlockService unlockService,
        SmtpNotificationService notificationService,
        ServiceNowClient serviceNowClient)
    {
        _unlockService = unlockService;
        _notificationService = notificationService;
        _serviceNowClient = serviceNowClient;
        _kernel = Kernel.CreateBuilder().Build();
    }

    public Task EnqueueAsync(ServiceNowIncident incident, CancellationToken cancellationToken)
    {
        _queue.Enqueue(incident);
        return Task.CompletedTask;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var incident))
            {
                var context = new KernelArguments
                {
                    ["upn"] = incident.UserPrincipalName,
                    ["incidentNumber"] = incident.Number
                };

                var unlockResult = await _unlockService.UnlockAccountAsync(incident.UserPrincipalName, cancellationToken);
                await _serviceNowClient.UpdateIncidentAsync(incident, unlockResult, cancellationToken);
                await _notificationService.NotifyAsync(incident, unlockResult, cancellationToken);
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }
    }
}
