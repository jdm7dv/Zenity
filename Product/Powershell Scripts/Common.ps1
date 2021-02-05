# Powershell command parameter to stop execution if an error is encountered
$script:ErrorActionPreference = "Stop"
[string]$global:authenticationError = "UserName or password provided for authentication is invalid."

if(!([Environment]::Version.Major -eq 4) -or !([Environment]::Version.Build -ge 30319))
{
	Write-Host .Net Framework 4.0.30319 is not loaded with Powershell. Please Configure .Net Framework 4.0.30319 to run with Powershell.
	Write-Host Press any key to exit...
	[System.Console]::ReadKey($true) | Out-Null
	Exit
}

[Reflection.Assembly]::LoadWithPartialName("System.ServiceModel")
# Initialize variables for the script
[string]$scriptLocation = [IO.Path]::GetDirectoryName($myinvocation.mycommand.path)
[string]$configSettingsFile = [IO.Path]::Combine($scriptLocation, "ZentityConsole.config")

$global:hashTableObject = New-Object System.Collections.HashTable

#Check whether the Config file exists or not
if(!(test-path($configSettingsFile)))
{
     Export-Clixml -Path $configSettingsFile -InputObject $global:hashTableObject
}
else
{
     $global:hashTableObject = Import-Clixml -Path $configSettingsFile
}

# Function to Check whether the required Service Uri is present in the config file
# If the service is present return the uri
function Check-ZentityServiceExists
{
    Param
    (
        [Parameter(Mandatory=$true, HelpMessage="The service name is required.")][string]$serviceName
    )
    
    if($global:hashTableObject.Contains($serviceName))
    {
        return $true
    }
    else
    {
        return $false
    }
}

# Add a new service uri to the Config Settings File and returns the service uri
function Add-ZentityServiceToConfig
{
    Param
    (
        [Parameter(Mandatory=$true, HelpMessage="Enter the Service Name.")][string]$serviceName,
        [Parameter(Mandatory=$true, HelpMessage="Enter the endpoint Uri for the Service.")][string]$serviceUri
    )
    
    if($global:hashTableObject.Contains($serviceName))
    {
        $global:hashTableObject[$serviceName] = $serviceUri
    }
    else
    {
        $global:hashTableObject.Add($serviceName, $serviceUri)
    }
    
    Export-Clixml -Path $configSettingsFile -InputObject $global:hashTableObject
    
    return $true
}

# Get service uri for a service if present in config file
function Get-ZentityServiceUriForService
{
    Param
    (
        [Parameter(Mandatory=$true, HelpMessage="The service name is required.")][string]$serviceName
    )
    
        return $global:hashTableObject[$serviceName]
}

# Function to change the service uri for PublishingService
function Set-ZentityPublishingServiceUri
{
   Param
   (
       [Parameter(Mandatory=$true, HelpMessage="Enter the new service uri for the Publishing Service")][string]$serviceUri
   )
   
   Add-ZentityServiceToConfig "PublishingService" $serviceUri
   
   Write-Host "Publishing Service Uri has been updated, this console session has to be restarted. Press any key to exit this session."
   [System.Console]::ReadKey($true) | Out-Null
   Exit
}

# Function to change the service uri for AdminConfigurationService
function Set-ZentityAdminConfigurationServiceUri
{
   Param
   (
       [Parameter(Mandatory=$true, HelpMessage="Enter the new service uri for the Admin Configuration Service")][string]$serviceUri
   )
   
   Add-ZentityServiceToConfig "AdminConfigurationService" $serviceUri
   
   Write-Host "Admin Configuration Service Uri has been updated, this console session has to be restarted. Press any key to exit this session."
   [System.Console]::ReadKey($true) | Out-Null
   Exit
}

# Function to change the service uri for ResourceTypeService
function Set-ZentityResourceTypeServiceUri
{
   Param
   (
       [Parameter(Mandatory=$true, HelpMessage="Enter the new service uri for the Resource Type Service")][string]$serviceUri
   )
   
   Add-ZentityServiceToConfig "ResourceTypeService" $serviceUri
   
   Write-Host "Resource Type Service Uri has been updated, this console session has to be restarted. Press any key to exit this session."
   [System.Console]::ReadKey($true) | Out-Null
   Exit
}

# Function to change the service uri for PublishingConfigurationService
function Set-ZentityPublishingConfigurationServiceUri
{
   Param
   (
       [Parameter(Mandatory=$true, HelpMessage="Enter the new service uri for the Publishing Configuration Service")][string]$serviceUri
   )
   
   Add-ZentityServiceToConfig "PublishingConfigurationService" $serviceUri
   
   Write-Host "Publishing Configuration Service Uri has been updated, this console session has to be restarted. Press any key to exit this session."
   [System.Console]::ReadKey($true) | Out-Null
   Exit
}

# Function to change the service uri for DataModelService
function Set-ZentityDataModelServiceUri
{
   Param
   (
       [Parameter(Mandatory=$true, HelpMessage="Enter the new service uri for the DataModel Service")][string]$serviceUri
   )
   
   Add-ZentityServiceToConfig "DataModelService" $serviceUri
   
   Write-Host "DataModel Service Uri has been updated, this console session has to be restarted. Press any key to exit this session."
   [System.Console]::ReadKey($true) | Out-Null
   Exit
}

# Function to change the install path of zentity
function Set-ZentityInstallPath
{
   Param
   (
       [Parameter(Mandatory=$true, HelpMessage="Enter the install path of Zentity")][string]$installPath
   )
   
   Add-ZentityServiceToConfig "InstallPath" $installPath
   
   Write-Host "The install path has been changed, this console session has to be restarted. Press any key to exit this session."
   [System.Console]::ReadKey($true) | Out-Null
   Exit
}

# Function to find out the installed Windows SDK path from the registry
function Get-WindowsSDKPath
{
    try
    {
        $windowsSDKRegKey = Get-ChildItem "HKLM:\SOFTWARE\Microsoft\Microsoft SDKs\Windows"
        [double]$highestSdkVersion = 0.0
        $highestSdkInstallPath = $null
        foreach($subKeyItem in $windowsSDKRegKey)
        {
            [double]$sdkVersion = [System.Double]::Parse($subKeyItem.PSChildName.Substring(1,3))
            if ($sdkVersion -gt $highestSdkVersion)
            {
                $highestSdkVersion = $sdkVersion
                $highestSdkInstallPath = $subKeyItem.GetValue("InstallationFolder")
            }
        }
        return $highestSdkInstallPath
    }
    catch [System.Exception]
    {
        Write-Host "Windows SDK is not installed. Please install the latest Windows SDK before using these commandlets."
    }
}

function Get-UserPrincipalName
{
    Param
    (
       [Parameter(Mandatory=$true, HelpMessage="Enter the location of the config file to load.")][string]$configFilePath
    )

	try
	{
		$configFilePath = [IO.Path]::Combine($scriptLocation, $configFilePath)
		[string]$userPrincipalName = Get-ZentityServiceUriForService UserPrincipalName
		if (![string]::IsNullOrWhiteSpace($userPrincipalName))
		{
			return $userPrincipalName
		}

		$configFileMap = New-Object System.Configuration.ExeConfigurationFileMap
		$configFileMap.ExeConfigFilename = $configFilePath
		$config = [System.Configuration.ConfigurationManager]::OpenMappedExeConfiguration($configFileMap, "None")
		[System.ServiceModel.Configuration.ServiceModelSectionGroup]$serviceConfig = $config.GetSectionGroup("system.serviceModel")
		Add-ZentityServiceToConfig "UserPrincipalName" $serviceConfig.Client.Endpoints[0].Identity.UserPrincipalName.Value
		return $serviceConfig.Client.Endpoints[0].Identity.UserPrincipalName.Value
	}
	finally
	{
		if ([System.IO.File]::Exists($configFilePath)) 
		{
			[System.IO.File]::Delete($configFilePath)
		}
	}
}

function Get-ClientCertificate
{
	try
	{
		if(Check-ZentityServiceExists ZentityCertificateDetails)
		{
			$certName = (Get-ZentityServiceUriForService ZentityCertificateDetails).Split(",")
			$store = New-Object System.Security.Cryptography.X509Certificates.X509Store($certName[2], $certName[1])
			$store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadOnly)
			$zenCertificate = $store.Certificates.Find($certName[3], $certName[0], $false)
			if($zenCertificate.Count -eq 0)
			{
				return null
			}
			else
			{
				return $zenCertificate[0]
			}
		}
	}
	catch [System.Exception]
	{
        Write-Host "Exception: "
        Write-Host $_.Exception.ToString()
	}
}

function Update-ZentityDataModels
{
    [string]$zentityConfigureDataModel = [IO.Path]::Combine($scriptLocation, "Zentity-ConfigureDataModel.ps1")
    . $zentityConfigureDataModel
    Write-Host "Update is in Progress"
    Update-ZentityDataModelClientSetup
}

function GetWsHttpBinding
{
	# Create the binding and endpoint object
	$tempWsHttpBinding = New-Object System.ServiceModel.WSHttpBinding
	$tempWsHttpBinding.Security.Mode = "Message"
	$tempWsHttpBinding.MaxBufferPoolSize = 10485760 
	$tempWsHttpBinding.MaxReceivedMessageSize = 10485760 
	$tempWsHttpBinding.ReceiveTimeout = [System.Timespan]::FromMinutes(10)
	$tempWsHttpBinding.SendTimeout = [System.Timespan]::FromMinutes(10)
	return $tempWsHttpBinding
}

function SetWindowsIdentity
{
	Param
    (
       [Parameter(Mandatory=$true, HelpMessage="Enter the service uri.")][string]$serviceUri,
	   [Parameter(Mandatory=$true, HelpMessage="Enter the config file name.")][string]$configFileName
    )

	$userPrincipalName = Get-UserPrincipalName $configFileName
	$endpointIdentity = [System.ServiceModel.EndpointIdentity]::CreateUpnIdentity($userPrincipalName)
	return New-Object System.ServiceModel.EndpointAddress((New-Object System.Uri $serviceUri), $endpointIdentity)
}

function SetCertificateIdentity
{
	Param
    (
       [Parameter(Mandatory=$true, HelpMessage="Enter the service uri.")][string]$serviceUri
    )
	$certificate = Get-ClientCertificate
	if($certificate -eq $null)
	{
		Write-Host "Certificate not found. Please enter the correct certificate details in ZentityConsole.config under ZentityCertificateDetails." -ForegroundColor Red
		Exit
	}

	$endpointIdentity = [System.ServiceModel.EndpointIdentity]::CreateX509CertificateIdentity($certificate)
	return  New-Object System.ServiceModel.EndpointAddress((New-Object System.Uri $serviceUri), $endpointIdentity)
}

function GetPublishingServiceProxy
{
	Param
	(
		[Parameter(Mandatory=$true, HelpMessage="Enter the publishing service uri.")][string]$publishingServiceUri,
		[Parameter(Mandatory=$false, HelpMessage="Enter the publishing service client object.")][Zentity.Services.Web.Pivot.PublishingServiceClient]$pubServiceObject
	)
	
	$wsHttpBinding = GetWsHttpBinding
	$authType = Get-ZentityServiceUriForService AuthenticationType
	if($pubServiceObject -eq $null)
	{
		if($authType -eq "Windows")
		{
			$publishingServiceEndpoint = SetWindowsIdentity $publishingServiceUri "PublishingService.config"
			$wsHttpBinding.Security.Message.ClientCredentialType = "Windows"
			return New-Object Zentity.Services.Web.Pivot.PublishingServiceClient($wsHttpBinding, $publishingServiceEndpoint)
		}
		else
		{
			$publishingServiceEndpoint = SetCertificateIdentity $publishingServiceUri
			$wsHttpBinding.Security.Message.ClientCredentialType = "UserName"
			Write-Host "Enter Zentity user name: "
			$userName = Read-Host
			Write-Host "Enter password: "
			$password = Read-Host
			$tempPubServiceClient = New-Object Zentity.Services.Web.Pivot.PublishingServiceClient($wsHttpBinding, $publishingServiceEndpoint)
			$tempPubServiceClient.ClientCredentials.UserName.UserName = $userName
			$tempPubServiceClient.ClientCredentials.UserName.Password = $password
			return $tempPubServiceClient
		}
	}
	elseif($pubServiceObject.State -eq [System.ServiceModel.CommunicationState]::Faulted -or $pubServiceObject.State -eq [System.ServiceModel.CommunicationState]::Closed)
	{
		if($authType -eq "Windows")
		{
			$publishingServiceEndpoint = SetWindowsIdentity $publishingServiceUri "PublishingService.config"
			$wsHttpBinding.Security.Message.ClientCredentialType = "Windows"
			return New-Object Zentity.Services.Web.Pivot.PublishingServiceClient($wsHttpBinding, $publishingServiceEndpoint)
		}
		else
		{
			$publishingServiceEndpoint = SetCertificateIdentity $publishingServiceUri
			$wsHttpBinding.Security.Message.ClientCredentialType = "UserName"
			$tempPubServiceClient = New-Object Zentity.Services.Web.Pivot.PublishingServiceClient($wsHttpBinding, $publishingServiceEndpoint)
			if($global:pubServiceClientFlag)
			{
				Write-Host "Enter Zentity user name: "
				$userName = Read-Host
				Write-Host "Enter password: "
				$password = Read-Host
				$tempPubServiceClient.ClientCredentials.UserName.UserName = $userName
				$tempPubServiceClient.ClientCredentials.UserName.Password = $password
			}
			else
			{
				$tempPubServiceClient.ClientCredentials.UserName.UserName = $pubServiceObject.ClientCredentials.UserName.UserName
				$tempPubServiceClient.ClientCredentials.UserName.Password = $pubServiceObject.ClientCredentials.UserName.Password
			}

			return $tempPubServiceClient
		}
	}
	else
	{
		return $pubServiceObject
	}
}

function GetDataModelServiceProxy
{
	Param
	(
		[Parameter(Mandatory=$true, HelpMessage="Enter the data model service uri.")][string]$dataModelServiceUri,
		[Parameter(Mandatory=$false, HelpMessage="Enter the data model service client object.")][Zentity.Services.Web.Data.DataModelServiceClient]$dataModelServiceObject
	)
	
	$wsHttpBinding = GetWsHttpBinding
	$authType = Get-ZentityServiceUriForService AuthenticationType
	if($dataModelServiceObject -eq $null)
	{
		if($authType -eq "Windows")
		{
			$dataModelServiceEndpoint = SetWindowsIdentity $dataModelServiceUri "DataModelService.config"
			$wsHttpBinding.Security.Message.ClientCredentialType = "Windows"
			return New-Object Zentity.Services.Web.Data.DataModelServiceClient($wsHttpBinding, $dataModelServiceEndpoint)
		}
		else
		{
			$dataModelServiceEndpoint = SetCertificateIdentity $dataModelServiceUri
			$wsHttpBinding.Security.Message.ClientCredentialType = "UserName"
			Write-Host "Enter Zentity user name: "
			$userName = Read-Host
			Write-Host "Enter password: "
			$password = Read-Host
			$tempDataModelServiceClient = New-Object Zentity.Services.Web.Data.DataModelServiceClient($wsHttpBinding, $dataModelServiceEndpoint)
			$tempDataModelServiceClient.ClientCredentials.UserName.UserName = $userName
			$tempDataModelServiceClient.ClientCredentials.UserName.Password = $password
			return $tempDataModelServiceClient
		}
	}
	elseif($dataModelServiceObject.State -eq [System.ServiceModel.CommunicationState]::Faulted -or $dataModelServiceObject.State -eq [System.ServiceModel.CommunicationState]::Closed)
	{
		if($authType -eq "Windows")
		{
			$dataModelServiceEndpoint = SetWindowsIdentity $dataModelServiceUri "DataModelService.config"
			$wsHttpBinding.Security.Message.ClientCredentialType = "Windows"
			return New-Object Zentity.Services.Web.Data.DataModelServiceClient($wsHttpBinding, $dataModelServiceEndpoint)
		}
		else
		{
			$dataModelServiceEndpoint = SetCertificateIdentity $dataModelServiceUri 
			$wsHttpBinding.Security.Message.ClientCredentialType = "UserName"
			$tempDataModelServiceClient = New-Object Zentity.Services.Web.Data.DataModelServiceClient($wsHttpBinding, $dataModelServiceEndpoint)
			if($global:dataModelServiceClientFlag)
			{
				Write-Host "Enter Zentity user name: "
				$userName = Read-Host
				Write-Host "Enter password: "
				$password = Read-Host
				$tempDataModelServiceClient.ClientCredentials.UserName.UserName = $userName
				$tempDataModelServiceClient.ClientCredentials.UserName.Password = $password
			}
			else
			{
				$tempDataModelServiceClient.ClientCredentials.UserName.UserName = $dataModelServiceObject.ClientCredentials.UserName.UserName
				$tempDataModelServiceClient.ClientCredentials.UserName.Password = $dataModelServiceObject.ClientCredentials.UserName.Password
			}

			return $tempDataModelServiceClient
		}
	}
	else
	{
		return $dataModelServiceObject
	}
}

function GetPublishingConfigServiceProxy
{
	Param
	(
		[Parameter(Mandatory=$true, HelpMessage="Enter the publishing config service uri.")][string]$pubConfigServiceUri,
		[Parameter(Mandatory=$false, HelpMessage="Enter the publishing config service client object.")][Zentity.Services.Web.Pivot.ConfigurationServiceClient]$pubConfigServiceObject
	)
	
	$wsHttpBinding = GetWsHttpBinding
	$authType = Get-ZentityServiceUriForService AuthenticationType
	if($pubConfigServiceObject -eq $null)
	{
		if($authType -eq "Windows")
		{
			$pubConfigServiceEndpoint = SetWindowsIdentity $pubConfigServiceUri "PublishingConfigService.config"
			$wsHttpBinding.Security.Message.ClientCredentialType = "Windows"
			return New-Object Zentity.Services.Web.Pivot.ConfigurationServiceClient($wsHttpBinding, $pubConfigServiceEndpoint)
		}
		else
		{
			$pubConfigServiceEndpoint = SetCertificateIdentity $pubConfigServiceUri
			$wsHttpBinding.Security.Message.ClientCredentialType = "UserName"
			Write-Host "Enter Zentity user name: "
			$userName = Read-Host
			Write-Host "Enter password: "
			$password = Read-Host
			$temppubConfigServiceClient = New-Object Zentity.Services.Web.Pivot.ConfigurationServiceClient($wsHttpBinding, $pubConfigServiceEndpoint)
			$temppubConfigServiceClient.ClientCredentials.UserName.UserName = $userName
			$temppubConfigServiceClient.ClientCredentials.UserName.Password = $password
			return $temppubConfigServiceClient
		}
	}
	elseif($pubConfigServiceObject.State -eq [System.ServiceModel.CommunicationState]::Faulted -or $pubConfigServiceObject.State -eq [System.ServiceModel.CommunicationState]::Closed)
	{
		if($authType -eq "Windows")
		{
			$pubConfigServiceEndpoint = SetWindowsIdentity $pubConfigServiceUri "PublishingConfigService.config"
			$wsHttpBinding.Security.Message.ClientCredentialType = "Windows"
			return New-Object Zentity.Services.Web.Pivot.ConfigurationServiceClient($wsHttpBinding, $pubConfigServiceEndpoint)
		}
		else
		{
			$pubConfigServiceEndpoint = SetCertificateIdentity $pubConfigServiceUri
			$wsHttpBinding.Security.Message.ClientCredentialType = "UserName"
			$temppubConfigServiceClient = New-Object Zentity.Services.Web.Pivot.ConfigurationServiceClient($wsHttpBinding, $pubConfigServiceEndpoint)
			if($global:publishingConfigServiceClientFlag)
			{
				Write-Host "Enter Zentity user name: "
				$userName = Read-Host
				Write-Host "Enter password: "
				$password = Read-Host
				$temppubConfigServiceClient.ClientCredentials.UserName.UserName = $userName
				$temppubConfigServiceClient.ClientCredentials.UserName.Password = $password
			}
			else
			{
				$temppubConfigServiceClient.ClientCredentials.UserName.UserName = $pubConfigServiceObject.ClientCredentials.UserName.UserName
				$temppubConfigServiceClient.ClientCredentials.UserName.Password = $pubConfigServiceObject.ClientCredentials.UserName.Password
			}
			return $temppubConfigServiceClient
		}
	}
	else
	{
		return $pubConfigServiceObject
	}
}

function GetResourceTypeServiceProxy
{
	Param
	(
		[Parameter(Mandatory=$true, HelpMessage="Enter the resource type service uri.")][string]$resTypeServiceUri,
		[Parameter(Mandatory=$false, HelpMessage="Enter the resource type service client object.")][Zentity.Services.Web.Data.ResourceTypeServiceClient]$resTypeServiceObject
	)
	
	$wsHttpBinding = GetWsHttpBinding
	$authType = Get-ZentityServiceUriForService AuthenticationType
	if($resTypeServiceObject -eq $null)
	{
		if($authType -eq "Windows")
		{
			$resTypeServiceEndpoint = SetWindowsIdentity $resTypeServiceUri "ResourceTypeService.config"
			$wsHttpBinding.Security.Message.ClientCredentialType = "Windows"
			return New-Object Zentity.Services.Web.Data.ResourceTypeServiceClient($wsHttpBinding, $resTypeServiceEndpoint)
		}
		else
		{
			$resTypeServiceEndpoint = SetCertificateIdentity $resTypeServiceUri
			$wsHttpBinding.Security.Message.ClientCredentialType = "UserName"
			Write-Host "Enter Zentity user name: "
			$userName = Read-Host
			Write-Host "Enter password: "
			$password = Read-Host
			$tempresTypeServiceClient = New-Object Zentity.Services.Web.Data.ResourceTypeServiceClient($wsHttpBinding, $resTypeServiceEndpoint)
			$tempresTypeServiceClient.ClientCredentials.UserName.UserName = $userName
			$tempresTypeServiceClient.ClientCredentials.UserName.Password = $password
			return $tempresTypeServiceClient
		}
	}
	elseif($resTypeServiceObject.State -eq [System.ServiceModel.CommunicationState]::Faulted -or $resTypeServiceObject.State -eq [System.ServiceModel.CommunicationState]::Closed)
	{
		if($authType -eq "Windows")
		{
			$resTypeServiceEndpoint = SetWindowsIdentity $resTypeServiceUri "ResourceTypeService.config"
			$wsHttpBinding.Security.Message.ClientCredentialType = "Windows"
			return New-Object Zentity.Services.Web.Data.ResourceTypeServiceClient($wsHttpBinding, $resTypeServiceEndpoint)
		}
		else
		{
			$resTypeServiceEndpoint = SetCertificateIdentity $resTypeServiceUri
			$wsHttpBinding.Security.Message.ClientCredentialType = "UserName"
			$tempresTypeServiceClient = New-Object Zentity.Services.Web.Data.ResourceTypeServiceClient($wsHttpBinding, $resTypeServiceEndpoint)
			if($global:resourceTypeServiceClientFlag)
			{
				Write-Host "Enter Zentity user name: "
				$userName = Read-Host
				Write-Host "Enter password: "
				$password = Read-Host
				$tempresTypeServiceClient.ClientCredentials.UserName.UserName = $userName
				$tempresTypeServiceClient.ClientCredentials.UserName.Password = $password
			}
			else
			{
				$tempresTypeServiceClient.ClientCredentials.UserName.UserName = $resTypeServiceObject.ClientCredentials.UserName.UserName
				$tempresTypeServiceClient.ClientCredentials.UserName.Password = $resTypeServiceObject.ClientCredentials.UserName.UserName
			}

			return $tempresTypeServiceClient
		}
	}
	else
	{
		return $resTypeServiceObject
	}
}

function GetAdminConfigServiceProxy
{
	Param
	(
		[Parameter(Mandatory=$true, HelpMessage="Enter the admin config service uri.")][string]$adminConfigServiceUri,
		[Parameter(Mandatory=$false, HelpMessage="Enter the admin config service client object.")][Zentity.Services.Web.Admin.ConfigurationServiceClient]$adminConfigServiceObject
	)
	
	$wsHttpBinding = GetWsHttpBinding
	$authType = Get-ZentityServiceUriForService AuthenticationType
	if($adminConfigServiceObject -eq $null)
	{
		if($authType -eq "Windows")
		{
			$adminConfigServiceEndpoint = SetWindowsIdentity $adminConfigServiceUri "AdminConfigService.config"
			$wsHttpBinding.Security.Message.ClientCredentialType = "Windows"
			return New-Object Zentity.Services.Web.Admin.ConfigurationServiceClient($wsHttpBinding, $adminConfigServiceEndpoint)
		}
		else
		{
			$adminConfigServiceEndpoint = SetCertificateIdentity $adminConfigServiceUri
			$wsHttpBinding.Security.Message.ClientCredentialType = "UserName"
			Write-Host "Enter Zentity user name: "
			$userName = Read-Host
			Write-Host "Enter password: "
			$password = Read-Host
			$tempAdminConfigServiceClient = New-Object Zentity.Services.Web.Admin.ConfigurationServiceClient($wsHttpBinding, $adminConfigServiceEndpoint)
			$tempAdminConfigServiceClient.ClientCredentials.UserName.UserName = $userName
			$tempAdminConfigServiceClient.ClientCredentials.UserName.Password = $password
			return $tempAdminConfigServiceClient
		}
	}
	elseif($adminConfigServiceObject.State -eq [System.ServiceModel.CommunicationState]::Faulted -or $adminConfigServiceObject.State -eq [System.ServiceModel.CommunicationState]::Closed)
	{
		if($authType -eq "Windows")
		{
			$adminConfigServiceEndpoint = SetWindowsIdentity $adminConfigServiceUri "AdminConfigService.config"
			$wsHttpBinding.Security.Message.ClientCredentialType = "Windows"
			return New-Object Zentity.Services.Web.Admin.ConfigurationServiceClient($wsHttpBinding, $adminConfigServiceEndpoint)
		}
		else
		{
			$adminConfigServiceEndpoint = SetCertificateIdentity $adminConfigServiceUri
			$wsHttpBinding.Security.Message.ClientCredentialType = "UserName"
			$tempAdminConfigServiceClient = New-Object Zentity.Services.Web.Admin.ConfigurationServiceClient($wsHttpBinding, $adminConfigServiceEndpoint)
			if($global:adminConfigServiceClientFlag)
			{
				Write-Host "Enter Zentity user name: "
				$userName = Read-Host
				Write-Host "Enter password: "
				$password = Read-Host
				$tempAdminConfigServiceClient.ClientCredentials.UserName.UserName = $userName
				$tempAdminConfigServiceClient.ClientCredentials.UserName.Password = $password
			}
			else
			{
				$tempAdminConfigServiceClient.ClientCredentials.UserName.UserName = $adminConfigServiceObject.ClientCredentials.UserName.UserName
				$tempAdminConfigServiceClient.ClientCredentials.UserName.Password = $adminConfigServiceObject.ClientCredentials.UserName.UserName
			}
			return $tempAdminConfigServiceClient
		}
	}
	else
	{
		return $adminConfigServiceObject
	}
}