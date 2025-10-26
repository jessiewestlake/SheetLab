using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Planners.Handlebars;
using ServiceNowUnlockWorkflow.Services;

namespace ServiceNowUnlockWorkflow.Agents;

public class IncidentMonitorAgent
{
    private readonly ServiceNowClient _client;
    private readonly UnlockCoordinatorAgent _coordinator;
    private readonly Kernel _kernel;
    private readonly HandlebarsPlanner _planner;

    public IncidentMonitorAgent(ServiceNowClient client, UnlockCoordinatorAgent coordinator)
    {
        _client = client;
        _coordinator = coordinator;
        _kernel = Kernel.CreateBuilder().Build();
        _planner = new HandlebarsPlanner();
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var incidents = await _client.GetUnlockIncidentsAsync(cancellationToken);
            foreach (var incident in incidents)
            {
                var plan = await _planner.CreatePlanAsync(_kernel, $"Classify incident {incident.Number} for unlock workflow");
                if (plan.Steps.Count > 0)
                {
                    await _coordinator.EnqueueAsync(incident, cancellationToken);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }
    }
}
