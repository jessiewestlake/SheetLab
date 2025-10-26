namespace ServiceNowUnlockWorkflow.Services;

public class ServiceNowIncident
{
    public string SysId { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string CallerEmail { get; set; } = string.Empty;
    public string UserPrincipalName { get; set; } = string.Empty;
}
