using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceNowUnlockWorkflow.Agents;
using ServiceNowUnlockWorkflow.Configuration;
using ServiceNowUnlockWorkflow.Services;

namespace ServiceNowUnlockWorkflow;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<AppSettings>(context.Configuration);
                services.AddSingleton<ServiceNowClient>();
                services.AddSingleton<SecretStoreBridge>();
                services.AddSingleton<PowerShellUnlockService>();
                services.AddSingleton<SmtpNotificationService>();

                services.AddSingleton<IncidentMonitorAgent>();
                services.AddSingleton<UnlockCoordinatorAgent>();

                services.AddHostedService<WorkflowHost>();
            })
            .Build();

        await host.RunAsync();
    }
}
