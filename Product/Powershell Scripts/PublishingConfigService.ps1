# Powershell command parameter to stop execution if an error is encountered
$script:ErrorActionPreference = "Stop"

# Initialize variables for the script
[string]$scriptLocation = [IO.Path]::GetDirectoryName($myinvocation.mycommand.path)
[string]$publishingConfigServiceProxy = [IO.Path]::Combine($scriptLocation, "PublishingConfigurationService.cs")
[string]$publishingConfigServiceAssembly = [IO.Path]::Combine($scriptLocation, "PublishingConfigurationService.dll")
[string]$publishingConfigServiceUri
[string]$commonScript = [IO.Path]::Combine($scriptLocation, "Common.ps1")
[bool]$global:publishingConfigServiceClientFlag = $false
    
# Import Common.ps1
. $commonScript 

# Check whether the service uri is present in config file
if(!(Check-ZentityServiceExists "PublishingConfigurationService"))
{
    # Read that the service uri from the user
    $publishingConfigServiceUri = Read-Host "Zentity : Please enter the endpoint uri for the Publishing Configuration service."
    if (!$publishingConfigServiceUri -or [string]::IsNullOrWhiteSpace($publishingConfigServiceUri))
    {
         throw (New-Object System.ArgumentNullException("Zentity : The endpoint uri value for the Publishing Configuration service was missing."))
    }
    if(Add-ZentityServiceToConfig "PublishingConfigurationService" $publishingConfigServiceUri)
    {
        $publishingConfigServiceUri = Get-ZentityServiceUriForService "PublishingConfigurationService"
    }
}
else
{
    $publishingConfigServiceUri = Get-ZentityServiceUriForService "PublishingConfigurationService"
}


# Check if service proxy assembly is generated. If not, then re-create them 
# by downloading the WSDL and then compiling the source code
if (!(test-path $publishingConfigServiceAssembly))
{
  try
  {
    # Initialize the paths for the 4.0 .Net SDK and .Net Framework
    $netSDKPath = [IO.Path]::Combine((Get-WindowsSDKPath), "bin")

    if (!(test-path $netSDKPath))
    {
        throw (New-Object System.IO.DirectoryNotFoundException("The Microsoft .Net 4.0 SDK folder ($netSDKPath) is not available."))
    }
    
    $netFrameworkPath = 'C:\Windows\microsoft.net\framework\v4.0.30319'
    if (!(test-path $netFrameworkPath))
    {
        throw (New-Object System.IO.DirectoryNotFoundException("The Microsoft .Net 4.0 Framework folder ($netFrameworkPath) is not available."))
    }

    # Set the environment path to point to the above paths.
    $env:Path += ';' + $netSDKPath + ';' + $netFrameworkPath

    # Concatenate the user argument to form the WSDL uri
    $wsdlUri = $publishingConfigServiceUri + "?wsdl"

	#Create the namespace for the proxy classes
	$publishingConfigServiceNamespace = "*,Zentity.Services.Web.Pivot"

    # Create the proxy class for the service
    svcutil.exe /n:$publishingConfigServiceNamespace $wsdlUri /out:$publishingConfigServiceProxy /ct:System.Collections.Generic.List``1 /config:PublishingConfigService.config
    
    if (!(test-path $publishingConfigServiceProxy))
    {
        throw (New-Object System.ApplicationException("Unable to create the proxy class from the service uri : $wsdlUri"))
    }

    # Generate the assembly from the proxy class
    csc /t:library /out:$publishingConfigServiceAssembly $publishingConfigServiceProxy
    
    if (!(test-path $publishingConfigServiceAssembly))
    {
        throw (New-Object System.ApplicationException("Unable to generate the assembly from the proxy class : $adminConfigServiceProxy"))
    }
  }
  catch [System.Exception]
  {
    Write-Host "Exception: "
    Write-Host $_.Exception.ToString()
    Write-Host "Press any key to exit"
        [System.Console]::ReadKey($true) | Out-Null
    Exit
  }
}

# Load the required assembly and the proxy assembly
[Reflection.Assembly]::LoadFrom($publishingConfigServiceAssembly)
[Reflection.Assembly]::LoadWithPartialName("System.ServiceModel")
[Reflection.Assembly]::LoadWithPartialName("System.Configuration")

$global:publishingConfigService = GetPublishingConfigServiceProxy $publishingConfigServiceUri $null

# Function to get the Model Configuration if specified or gets the default one
function Get-ZentityModelConfiguration
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityModelConfiguration
    .DESCRIPTION  
        Function to get the Model Configuration if specified or gets the default one
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        $modelConfiguration = Get-ZentityModelConfiguration Zentity.ScholarlyWorks
        $modelConfiguration
        Output: 
            ExtensionData                                     name                                              resourceTypeSettings
            -------------                                     ----                                              --------------------
            System.Runtime.Serialization.ExtensionDataObject  Zentity.ScholarlyWorks                            {Letter, Image, Data, Organization...}
    #>

    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the model namespace containing the resource type.")][string]$modelNamespace
    )
	
	try
	{
		$global:publishingConfigService = GetPublishingConfigServiceProxy $publishingConfigServiceUri $global:publishingConfigService 
		$tempVar = $global:publishingConfigService.GetModelConfiguration($modelNamespace)
		$global:publishingConfigServiceClientFlag = $false
		return $tempVar 
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:publishingConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get the Resource Type Configuration if specified or gets the default one
function Get-ZentityResourceTypeConfiguration
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityResourceTypeConfiguration
    .DESCRIPTION  
        Function to get the Resource Type Configuration if specified or gets the default one
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        $resourceTypeSetting = Get-ZentityResourceTypeConfiguration Zentity.ScholarlyWorks Book
        $resourceTypeSetting
        Output:         
            ExtensionData : System.Runtime.Serialization.ExtensionDataObject
            facets        : {Title, DateModified, Id, DateAdded...}
            link          : Zentity.Services.Configuration.Pivot.Link
            name          : Book
            visual        : Zentity.Services.Configuration.Pivot.Visual
    #>
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the model namespace containing the resource type.")][string]$modelNamespace,
        [Parameter(Mandatory=$true,HelpMessage="Enter the model namespace containing the resource type.")][string]$resourceTypeName
    )
	
	try
	{
		$global:publishingConfigService = GetPublishingConfigServiceProxy $publishingConfigServiceUri $global:publishingConfigService 
		$tempVar = $global:publishingConfigService.GetResourceTypeConfiguration($modelNamespace, $resourceTypeName)
		$global:publishingConfigServiceClientFlag = $false
		return $tempVar 
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:publishingConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to set the Module Configuration
function Set-ZentityModelConfiguration
{
    <#  
    .SYNOPSIS  
        Zentity Console : Set-ZentityModuleConfiguration
    .DESCRIPTION  
        Function to set the Module Configuration
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE 
        $modelConfiguration = Get-ZentityModelConfiguration Zentity.ScholarlyWorks
        --Change the $modelConfiguration object for custom configuration--
        Set-ZentityModelConfiguration Zentity.ScholarlyWorks $modelConfiguration 
        Output: True
    #>

    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the model namespace containing the resource type.")][string]$modelNamespace,
        [Parameter(Mandatory=$true,HelpMessage="Enter the model configuration.")][Zentity.Services.Web.Pivot.ModuleSetting]$modelConfiguration
    )
	
	try
	{
		$global:publishingConfigService = GetPublishingConfigServiceProxy $publishingConfigServiceUri $global:publishingConfigService 
		$global:publishingConfigService.SetModelConfiguration($modelNamespace, $modelConfiguration)
		$global:publishingConfigServiceClientFlag = $false
		return $true
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:publishingConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to set the Resource Type Configuration
function Set-ZentityResourceTypeConfiguration
{
    <#  
    .SYNOPSIS  
        Zentity Console : Set-ZentityResourceTypeConfiguration
    .DESCRIPTION  
        Function to set the Resource Type Configuration
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE 
        $resourceTypeSetting = Get-ZentityResourceTypeConfiguration Zentity.ScholarlyWorks Book
        --Change the $resourceTypeSetting object for custom configuration--
        Set-ZentityResourceTypeConfiguration Zentity.ScholarlyWorks Book $resourceTypeSetting 
        Output: True
    .EXAMPLE
        Set-ZentityResourceTypeConfiguration Zentity.ScholarlyWorks Book $null 
        Output: True
    #>

    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the model namespace containing the resource type.")][string]$modelNamespace,
        [Parameter(Mandatory=$true,HelpMessage="Enter the resource type name.")][string]$resourceTypeName,
        [Parameter(Mandatory=$false,HelpMessage="Enter the resource type configuration.")][Zentity.Services.Web.Pivot.ResourceTypeSetting]$resourceTypeConfig        
    )
	
	try
	{
		$global:publishingConfigService = GetPublishingConfigServiceProxy $publishingConfigServiceUri $global:publishingConfigService 
		$global:publishingConfigService.SetResourceTypeConfiguration($modelNamespace, $resourceTypeName, $resourceTypeConfig)
		$global:publishingConfigServiceClientFlag = $false
		return $true
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:publishingConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

write-host
write-host -ForegroundColor yellow "Zentity Console : Publishing Configuration Service"
write-host
write-host -ForegroundColor yellow "       -- function list --           "
write-host
get-command -noun zentity* | write-host -ForegroundColor green