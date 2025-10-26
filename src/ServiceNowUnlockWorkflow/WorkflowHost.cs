using Microsoft.Extensions.Hosting;
using ServiceNowUnlockWorkflow.Agents;

namespace ServiceNowUnlockWorkflow;

public class WorkflowHost : BackgroundService
{
    private readonly IncidentMonitorAgent _monitor;
    private readonly UnlockCoordinatorAgent _coordinator;

    public WorkflowHost(IncidentMonitorAgent monitor, UnlockCoordinatorAgent coordinator)
    {
        _monitor = monitor;
        _coordinator = coordinator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var monitorTask = _monitor.RunAsync(stoppingToken);
        var coordinatorTask = _coordinator.RunAsync(stoppingToken);

        await Task.WhenAll(monitorTask, coordinatorTask);
    }
}
