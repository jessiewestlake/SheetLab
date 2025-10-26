param(
    [Parameter(Mandatory=$true)]
    [string]$UserPrincipalName,

    [Parameter(Mandatory=$true)]
    [string]$ServiceAccountSecret
)

$ErrorActionPreference = 'Stop'

try {
    Import-Module Microsoft.PowerShell.SecretManagement -ErrorAction Stop
    Import-Module Microsoft.PowerShell.SecretStore -ErrorAction Stop
} catch {
    Write-Warning "SecretManagement modules not available, falling back to DPAPI/cred manager if implemented externally."
}

function Get-AdCredential {
    param([string]$SecretName)

    try {
        return Get-Secret -Name $SecretName -ErrorAction Stop
    } catch {
        throw "Failed to retrieve secret $SecretName: $_"
    }
}

function Unlock-AdAccountHardened {
    param(
        [string]$Upn,
        [string]$CredentialSecret
    )

    $credential = $null
    if ($CredentialSecret) {
        $credential = Get-AdCredential -SecretName $CredentialSecret
    }

    if (-not (Get-Module ActiveDirectory -ListAvailable)) {
        throw "ActiveDirectory module is required."
    }

    Import-Module ActiveDirectory

    if ($credential -is [System.Management.Automation.PSCredential]) {
        Unlock-ADAccount -Identity $Upn -Credential $credential -ErrorAction Stop
    } else {
        Unlock-ADAccount -Identity $Upn -ErrorAction Stop
    }
}

Unlock-AdAccountHardened -Upn $UserPrincipalName -CredentialSecret $ServiceAccountSecret

Write-Output "Unlocked $UserPrincipalName at $(Get-Date -Format o)"
