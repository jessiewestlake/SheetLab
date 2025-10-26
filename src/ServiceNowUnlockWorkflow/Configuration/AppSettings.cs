namespace ServiceNowUnlockWorkflow.Configuration;

public class AppSettings
{
    public ServiceNowSettings ServiceNow { get; set; } = new();
    public PowerShellSettings PowerShell { get; set; } = new();
    public SmtpSettings Smtp { get; set; } = new();
}

public class ServiceNowSettings
{
    public string BaseUrl { get; set; } = "https://example.service-now.com";
    public string Query { get; set; } = "category=software^u_request_type=AD Unlock^state=1";
    public string CredentialName { get; set; } = "ServiceNowBasicAuth";
    public string SuccessState { get; set; } = "6";
    public string FailureState { get; set; } = "2";
}

public class PowerShellSettings
{
    public string ScriptPath { get; set; } = "scripts/UnlockAccount.ps1";
    public string ServiceAccountSecretName { get; set; } = "AdServiceAccount";
}

public class SmtpSettings
{
    public string Host { get; set; } = "smtp.example.corp";
    public int Port { get; set; } = 25;
    public bool EnableSsl { get; set; } = false;
    public string FromAddress { get; set; } = "it-support@example.corp";
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
