using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceNowUnlockWorkflow.Configuration;

namespace ServiceNowUnlockWorkflow.Services;

public class ServiceNowClient
{
    private readonly HttpClient _httpClient;
    private readonly AppSettings _settings;
    private readonly SecretStoreBridge _secretStore;
    private readonly ILogger<ServiceNowClient> _logger;

    public ServiceNowClient(IOptions<AppSettings> settings, SecretStoreBridge secretStore, ILogger<ServiceNowClient> logger)
    {
        _settings = settings.Value;
        _secretStore = secretStore;
        _logger = logger;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_settings.ServiceNow.BaseUrl)
        };
    }

    public async Task<IReadOnlyList<ServiceNowIncident>> GetUnlockIncidentsAsync(CancellationToken cancellationToken)
    {
        var credentials = await _secretStore.GetBasicAuthAsync(_settings.ServiceNow.CredentialName, cancellationToken);
        var byteArray = System.Text.Encoding.ASCII.GetBytes($"{credentials.UserName}:{credentials.Password}");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        var url = $"/api/now/table/incident?sysparm_query={Uri.EscapeDataString(_settings.ServiceNow.Query)}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ServiceNowResponse>(cancellationToken: cancellationToken);
        return payload?.Result ?? Array.Empty<ServiceNowIncident>();
    }

    public async Task UpdateIncidentAsync(ServiceNowIncident incident, UnlockResult result, CancellationToken cancellationToken)
    {
        var note = $"Account unlock {(result.Success ? "succeeded" : "failed")}: {result.Message}";
        var body = new
        {
            work_notes = note,
            state = result.Success ? _settings.ServiceNow.SuccessState : _settings.ServiceNow.FailureState
        };

        var response = await _httpClient.PutAsJsonAsync($"/api/now/table/incident/{incident.SysId}", body, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private sealed class ServiceNowResponse
    {
        public List<ServiceNowIncident> Result { get; set; } = new();
    }
}
