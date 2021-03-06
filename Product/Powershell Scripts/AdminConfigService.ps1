# Powershell command parameter to stop execution if an error is encountered
$script:ErrorActionPreference = "Stop"

# Initialize variables for the script 
[string]$scriptLocation = [IO.Path]::GetDirectoryName($myinvocation.mycommand.path)
[string]$adminConfigServiceProxy = [IO.Path]::Combine($scriptLocation, "ConfigurationService.cs")
[string]$adminConfigServiceAssembly = [IO.Path]::Combine($scriptLocation, "AdminConfigurationService.dll")
[string]$adminConfigServiceUri
[string]$commonScript = [IO.Path]::Combine($scriptLocation, "Common.ps1")
[string]$serverName = [String]::Empty
[bool]$global:adminConfigServiceClientFlag = $false
    
# Import Common.ps1
. $commonScript 

# Check whether the service uri is present in config file
if(!(Check-ZentityServiceExists "AdminConfigurationService"))
{
    # Read that the service uri from the user
    $adminConfigServiceUri = Read-Host "Zentity : Please enter the endpoint uri for the Admin Configuration service."
    if (!$adminConfigServiceUri -or [string]::IsNullOrWhiteSpace($adminConfigServiceUri))
    {
         throw (New-Object System.ArgumentNullException("Zentity : The endpoint uri value for the Admin Configuration service was missing."))
    }
    if(Add-ZentityServiceToConfig "AdminConfigurationService" $adminConfigServiceUri)
    {
        $adminConfigServiceUri = Get-ZentityServiceUriForService "AdminConfigurationService"
    }
}
else
{
    $adminConfigServiceUri = Get-ZentityServiceUriForService "AdminConfigurationService"
}

$hostUri = New-Object System.Uri($adminConfigServiceUri)
$serverName = $hostUri.Host


# Check if service proxy assembly is generated. If not, then re-create them 
# by downloading the WSDL and then compiling the source code
if (!(test-path $adminConfigServiceAssembly))
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
    $wsdlUri = $adminConfigServiceUri + "?wsdl"

	#Create the namespace for the proxy classes
	$adminConfigServiceNamespace = "*,Zentity.Services.Web.Admin"

    # Create the proxy class for the service
    svcutil.exe /n:$adminConfigServiceNamespace $wsdlUri /out:$adminConfigServiceProxy /ct:System.Collections.Generic.List``1 /config:AdminConfigService.config
    
    if (!(test-path $adminConfigServiceProxy))
    {
        throw (New-Object System.ApplicationException("Unable to create the proxy class from the service uri : $wsdlUri"))
    }
    
    # Generate the assembly from the proxy class
    csc /t:library /out:$adminConfigServiceAssembly $adminConfigServiceProxy
    
    if (!(test-path $adminConfigServiceAssembly))
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
[Reflection.Assembly]::LoadFrom($adminConfigServiceAssembly)
[Reflection.Assembly]::LoadWithPartialName("System.ServiceModel")
[Reflection.Assembly]::LoadWithPartialName("System.ServiceProcess")
[Reflection.Assembly]::LoadWithPartialName("System.Configuration")

$global:adminConfigService = $null

[bool]$global:hostConfgiFile = $false
[bool]$global:notificationConfgiFile = $false
[bool]$global:pivotGalleryConfgiFile = $false

# Function to check the status of the service proxy client
function Find-ZentityAdminConfigServiceClient
{
    Param
    (
        [Parameter(Mandatory=$true)][string]$openMethodName
    ) 
    if (!$global:adminConfigService)
    {
        throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because [$openMethodName] was not called to prior to this."))
    }
}

function CheckConfigurationFileOpened
{
    Param
    (
        [Parameter(Mandatory=$true)][string]$openMethodName
    )
    switch($openMethodName)
    {
        "HostConfigurationFile" { return $global:hostConfigFile }
        "NotificationServiceConfigurationFile" { return $global:notificationConfgiFile }
        "PivotGalleryConfigurationFile" { return $global:pivotGalleryConfgiFile }
        default {}
    }
}

# Function to open the host configuration file
function Open-ZentityHostConfigurationFile
{
    <#  
    .SYNOPSIS  
        Zentity Console : Open-ZentityHostConfigurationFile
    .DESCRIPTION  
        Function to open the host configuration file 
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Open-ZentityHostConfigurationFile
        Output: True
    #>
     
	 try
	 {
		$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
		$global:hostConfigFile = $true
		$global:adminConfigService.OpenHostConfigurationFile()
		$global:adminConfigServiceClientFlag = $false
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to open the notification service configuration file
function Open-ZentityNotificationServiceConfigurationFile
{
    <#  
    .SYNOPSIS  
        Zentity Console : Open-ZentityNotificationServiceConfigurationFile
    .DESCRIPTION  
        Function to open the notification service configuration file 
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Open-ZentityNotificationServiceConfigurationFile 'C:\Notification\Zentity.Services.Windows.exe'
        Output: True
    #>

    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the path of exe.")][string]$pathOfExe
    )  

	try
	{
		$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
		$global:notificationConfgiFile = $true
		$global:adminConfigService.OpenNotificationServiceConfigurationFile($pathOfExe)
		$global:adminConfigServiceClientFlag = $false
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to open the pivot gallery configuration file
function Open-ZentityPivotGalleryConfigurationFile
{
    <#  
    .SYNOPSIS  
        Zentity Console : Open-ZentityPivotGalleryConfigurationFile
    .DESCRIPTION  
        Function to open the pivot gallery configuration file
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Open-ZentityPivotGalleryConfigurationFile '/' 'Zentity Pivot Gallery' 'localhost'
        Output: True
    .EXAMPLE
        Open-ZentityPivotGalleryConfigurationFile '/VisualExplorer/Pivot' 'Zentity V2' 'localhost'
        Output: True
    #>

    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the path of configuration file.")][string]$path,
        [Parameter(Mandatory=$true,HelpMessage="Enter the pivot gallery site name.")][string]$zentityPivotGallerySite,
        [Parameter(Mandatory=$true,HelpMessage="Enter the remote server name.")][string]$remoteServer
    )
	
	try
	{
		$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
		$global:pivotGalleryConfgiFile = $true
		return $global:adminConfigService.OpenPivotGalleryConfigurationFile($path, $zentityPivotGallerySite, $remoteServer)
		$global:adminConfigServiceClientFlag = $false
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to save and close the host configuration file
function Close-ZentityHostConfigurationFile
{
    <#  
    .SYNOPSIS  
        Zentity Console : Close-ZentityHostConfigurationFile
    .DESCRIPTION  
        Saves the Host configuration file and closes it 
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Close-ZentityHostConfigurationFile
        Output: True
    #>

	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
		$returnValue = $global:adminConfigService.SaveHostConfigurationFile()
		$global:adminConfigServiceClientFlag = $false
		$global:hostConfigFile = $false
		$global:adminConfigService = $null
		$returnValue
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

function StartStopService
{    
   Param
   (
       [Parameter(Mandatory=$true,HelpMessage="Enter service name.")][string]$name
   )

   $serviceController = New-Object System.ServiceProcess.ServiceController($name, $serverName)
   $tempServerName = [string]::Concat("\\", $serverName)
   if($serviceController.ServiceName -ne $null)
   { 
       $serviceController.Refresh()
       if($serviceController.Status -eq [System.ServiceProcess.ServiceControllerStatus]::Running)
       {
        $serviceController.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running)
        sc.exe $tempServerName stop $name
        $serviceController.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Stopped)
        sc.exe $tempServerName start $name
        $serviceController.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running)
       }
       if($serviceController.Status -eq [System.ServiceProcess.ServiceControllerStatus]::Stopped)
       {
            sc.exe $tempServerName start $name
            $serviceController.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running)
       }
  }
}

# Function to save and close the notification configuration file
function Close-ZentityNotificationServiceConfigurationFile
{
    <#  
    .SYNOPSIS  
        Zentity Console : Close-ZentityNotificationServiceConfigurationFile
    .DESCRIPTION  
        Saves the Notification configuration file and closes it 
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Close-ZentityNotificationServiceConfigurationFile
        Output: True
    #>
	
	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityNotificationServiceConfigurationFile"
		$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
		$returnValue = $global:adminConfigService.SaveNotificationServiceConfigurationFile()
		$global:adminConfigServiceClientFlag = $false
		$global:notificationConfgiFile = $false
		$global:adminConfigService = $null
		$returnValue
		StartStopService "NotificationService"
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to save and close the pivot gallery configuration file
function Close-ZentityPivotGalleryConfigurationFile
{
    <#  
    .SYNOPSIS  
        Zentity Console : Close-ZentityPivotGalleryConfigurationFile
    .DESCRIPTION  
        Saves the Pivot gallery configuration file and closes it 
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Close-ZentityPivotGalleryConfigurationFile
        Output: True
    #>

	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityPivotGalleryConfigurationFile"
		$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
		$returnValue = $global:adminConfigService.SavePivotGalleryConfigurationFile()
		$global:adminConfigServiceClientFlag = $false
		$global:pivotGalleryConfgiFile = $false
		$global:adminConfigService = $null
		return $returnValue
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to set the image file path to be used for default DeepZoom image creation
function Set-ZentityDefaultDeepZoomTemplateLocation
{
    <#  
    .SYNOPSIS  
        Zentity Console : Set-ZentityDefaultDeepZoomTemplateLocation
    .DESCRIPTION  
        Sets the location of the default deep zoom image
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Set-ZentityDefaultDeepZoomTemplateLocation 'C:\Templaes\Default.html'
        Output: True
    #>

    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the path of the image file to be used for default DeepZoom image.")][string]$imageFilePath
    ) 

	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		if(CheckConfigurationFileOpened("HostConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.SetDefaultDeepZoomTemplateLocation($imageFilePath)
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Host Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to set the handler type for handling WebCapture image generation
function Set-ZentityWebCaptureProviderType
{
    <#  
    .SYNOPSIS  
        Zentity Console : Set-ZentityWebCaptureProviderType
    .DESCRIPTION  
        Sets the type of Web Capture to be used
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Set-ZentityWebCaptureProviderType 'Zentity.WebCapture.ImageCapture, ZentityWebCapture'
        Output: True
    #>
    
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the fully qualified type of the handler that implements IImageCapture interface.")][string]$handlerType
    )

	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		Write-Warning "Please ensure that the handler assembly is placed in the [bin] folder of the service host web application."
		if(CheckConfigurationFileOpened("HostConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.SetWebCaptureProviderType($handlerType)         
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Host Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to set the output type [Zentity.Services.Web.Admin.OutputType] for published collections .
function Set-ZentityOutputToType
{
    <#  
    .SYNOPSIS  
        Zentity Console : Set-ZentityOutputToType
    .DESCRIPTION  
        Sets the Output To setting to the type specified
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Set-ZentityOutputToType Relative
        Output: True
    #>
    
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Please enter the type of output [Zentity.Services.Web.Admin.OutputType] desired for published collections.")][Zentity.Services.Web.Admin.OutputType]$outputType
    )

	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		if(CheckConfigurationFileOpened("HostConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.SetOutputToType($outputType)   
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Host Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to set the Uri format type for the links.
function Set-ZentityUriFormatType
{
    <#  
    .SYNOPSIS  
        Zentity Console : Set-ZentityUriFormatType
    .DESCRIPTION  
        Sets the Uri Format to the specified format
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Set-ZentityUriFormatType Filestream
        Output: True
    #>
    
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Please enter the uri format type [Zentity.Services.Web.Admin.UriFormatType] for links in published collections.")][Zentity.Services.Web.Admin.UriFormatType]$uriType
    )

	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		if(CheckConfigurationFileOpened("HostConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.SetUriFormatType($uriType)
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Host Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to set the Uri format type for the links.
function Set-ZentityBaseUri
{
    <#  
    .SYNOPSIS  
        Zentity Console : Set-ZentityBaseUri
    .DESCRIPTION  
        Sets the Base uri for the generation of related collections links
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Set-ZentityBaseUri 'http://server:port/'
        Output: True
    #>
    
    Param
    (
        [Parameter(Mandatory=$false,HelpMessage="Please enter the base uri for links in published collections.")][string]$baseUri
    )
	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		if(CheckConfigurationFileOpened("HostConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.SetBaseUri($baseUri)
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Host Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to set the location where published collections and DeepZoom images are copied finally.
function Set-ZentitySplitSize
{
    <#  
    .SYNOPSIS  
        Zentity Console : Set-ZentitySplitSize
    .DESCRIPTION  
        Sets the Split Size of the Collections
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Set-ZentitySplitSize 5
        Output: True
    #>
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Please enter the split size of the Pivot collections.")][int]$splitSize
    )

	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		if(CheckConfigurationFileOpened("HostConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.SetSplitSize($splitSize)
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Host Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to set the location where published collections and DeepZoom images are copied finally.
function Set-ZentityEnableRelatedCollections
{
    <#  
    .SYNOPSIS  
        Zentity Console : Set-ZentityEnableRelatedCollections
    .DESCRIPTION  
        Enables or Disables the generation of related collections
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Set-ZentityEnableRelatedCollections $true
        Output: True
    #>
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter true/false whether to enable/disable generation of related collections.")][bool]$enable
    )

	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		if(CheckConfigurationFileOpened("HostConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.EnableRelatedCollections($enable)
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Host Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get the location where the default deep zoom image is present.
function Get-ZentityDefaultDeepZoomTemplateLocation
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityDefaultDeepZoomTemplateLocation
    .DESCRIPTION  
         Gets the location where the default deep zoom image is present
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Get-ZentityDefaultDeepZoomTemplateLocation
        Output: C:\Templates\Default.html
    #>
    
	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		if(CheckConfigurationFileOpened("HostConfigurationFile"))
		{
		   $global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
		   $location = $global:adminConfigService.GetDefaultDeepZoomTemplateLocation()
		$global:adminConfigServiceClientFlag = $false
		   if([string]::IsNullOrWhiteSpace($location))
		   {
				return "Setting is not set."
		   }
       
		   return $location
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Host Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to gets the WebCapture type.
function Get-ZentityWebCaptureProviderType
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityWebCaptureProviderType
    .DESCRIPTION  
         Gets the WebCapture type
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Get-ZentityWebCaptureProviderType
        Output: "ZentityWebCapture.ImageCapture, ZentityWebCapture"
    #>
    
	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		if(CheckConfigurationFileOpened("HostConfigurationFile"))
		{
		   $global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
		   $type = $global:adminConfigService.GetWebCaptureProviderType()
		   $global:adminConfigServiceClientFlag = $false
		   if([string]::IsNullOrWhiteSpace($type))
		   {
				return "Setting is not set."
		   }
       
		   return $type
       
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Host Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get the Output to type.
function Get-ZentityOutputToType
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityOutputToType
    .DESCRIPTION  
         Gets the Output to type
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Get-ZentityOutputToType
        Output: Filestream
    #>
    
	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		if(CheckConfigurationFileOpened("HostConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.GetOutputToType()
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Host Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get the Uri format type.
function Get-ZentityUriFormatType
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityUriFormatType
    .DESCRIPTION  
         Gets the Uri format type
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Get-ZentityUriFormatType
        Output: Relative
    #>
    
	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		if(CheckConfigurationFileOpened("HostConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.GetUriFormatType()
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Host Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get the Base Uri.
function Get-ZentityBaseUri
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityBaseUri
    .DESCRIPTION  
        Gets the Base Uri
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Get-ZentityBaseUri
        Output: "http://server:port/"
    #>
    
	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		if(CheckConfigurationFileOpened("HostConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$baseUri = $global:adminConfigService.GetBaseUri()
			$global:adminConfigServiceClientFlag = $false
			if([string]::IsNullOrWhiteSpace($baseUri))
			{
				return "Setting is not set."
			}
       
			return $baseUri
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Host Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get the Ouput folder location where the cxml's are to be generated.
function Get-ZentityOutputFolderLocation
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityOutputFolderLocation
    .DESCRIPTION  
        Gets the Ouput folder location where the cxml's are to be generated
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Get-ZentityOutputFolderLocation
        Output: "C:\Publishing\"
    #>
    
	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		if(CheckConfigurationFileOpened("HostConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$location = $global:adminConfigService.GetOutputFolderLocation()
			$global:adminConfigServiceClientFlag = $false
			if([string]::IsNullOrWhiteSpace($location))
			{
				return "Setting is not set."
			}
			return $location
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Host Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to Gets the Split size value.
function Get-ZentitySplitSize
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentitySplitSize
    .DESCRIPTION  
        Gets the Split size value
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Get-ZentitySplitSize
        Output: 5
    #>
    
	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		if(CheckConfigurationFileOpened("HostConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.GetSplitSize()
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Host Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get value indicating whether related collections are to be generated or not.
function Get-ZentityIsRelatedCollectionsEnabled
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityIsRelatedCollectionsEnabled
    .DESCRIPTION  
        Gets value indicating whether related collections are to be generated or not
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Get-ZentityIsRelatedCollectionsEnabled
        Output: True
    #>
    
	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		if(CheckConfigurationFileOpened("HostConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.IsRelatedCollectionsEnabled()
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Host Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to set the Batch Size.
function Set-ZentityBatchSize
{
    <#  
    .SYNOPSIS  
        Zentity Console : Set-ZentityBatchSize
    .DESCRIPTION  
        Sets the Batch Size
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Set-ZentityBatchSize
        Output: 250
    #>
    
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Please enter batch size for collections.")][int]$batchSize
    )
	
	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityNotificationServiceConfigurationFile"
		if(CheckConfigurationFileOpened("NotificationServiceConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.SetBatchSize($batchSize)
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Notification Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to set the Time out value.
function Set-ZentityTimeOut
{
    <#  
    .SYNOPSIS  
        Zentity Console : Set-ZentityTimeOut
    .DESCRIPTION  
        Sets the Time out value
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Set-ZentityTimeOut
        Output: 5000
    #>
    
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Please enter timeout for collections.")][int]$timeOut
    )

	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityNotificationServiceConfigurationFile"
		if(CheckConfigurationFileOpened("NotificationServiceConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.SetTimeOut($timeOut)
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Notification Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get the batch size.
function Get-ZentityBatchSize
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityBatchSize
    .DESCRIPTION  
        Gets the batch size
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Get-ZentityBatchSize
        Output: 250
    #>
    
	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityNotificationServiceConfigurationFile"
		if(CheckConfigurationFileOpened("NotificationServiceConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.GetBatchSize()
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Notification Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get the Time out value.
function Get-ZentityTimeOut
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityTimeOut
    .DESCRIPTION  
        Gets the Time out value
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Get-ZentityTimeOut
        Output: 5000
    #>
    
	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityNotificationServiceConfigurationFile"
		if(CheckConfigurationFileOpened("NotificationServiceConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.GetTimeOut()
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Notification Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to set the path prefix.
function Set-ZentityPathPrefix
{
    <#  
    .SYNOPSIS  
        Zentity Console : Set-ZentityPathPrefix
    .DESCRIPTION  
        Sets the path prefix
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Set-ZentityPathPrefix
        Output: 'collections'
    #>
    
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Please enter the path prefix for the collection.")][string]$pathPrefix
    )

	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityPivotGalleryConfigurationFile"
		if(CheckConfigurationFileOpened("PivotGalleryConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.SetPathPrefix($pathPrefix)
			$global:adminConfigServiceClientFlag = $false	
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Pivot Gallery Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get the collection file path.
function Get-ZentityCollectionFilePath
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityCollectionFilePath
    .DESCRIPTION  
        Gets the collection file path
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Get-ZentityCollectionFilePath
        Output: "C:\Publishing\"
    #>
    
	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityPivotGalleryConfigurationFile"
		if(CheckConfigurationFileOpened("PivotGalleryConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.GetCollectionFilePath()
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Pivot Gallery Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get the path prefix.
function Get-ZentityPathPrefix
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityPathPrefix
    .DESCRIPTION  
        Gets the path prefix
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Get-ZentityPathPrefix
        Output: "collections"
    #>
    
	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityPivotGalleryConfigurationFile"
		if(CheckConfigurationFileOpened("PivotGalleryConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.GetPathPrefix()
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Pivot Gallery Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to configure the output folder for Collections
function Set-ZentityCollectionsFilePath
{
    <#  
    .SYNOPSIS  
        Zentity Console : SetZentityCollectionsFilePath
    .DESCRIPTION  
        Sets the Collection file path
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Set-ZentityCollectionsFilePath '/VisualExplorer/Pivot' 'Zentity V2' 'localhost' 'C:\PublishedCollections'
        Output: 
    #>

	Param
	(
        	[Parameter(Mandatory=$true,HelpMessage="Enter the path of configuration file.")][string]$path,
        	[Parameter(Mandatory=$true,HelpMessage="Enter the pivot gallery site name.")][string]$zentityPivotGallerySite,
        	[Parameter(Mandatory=$true,HelpMessage="Enter the remote server name.")][string]$remoteServer,
        	[Parameter(Mandatory=$true,HelpMessage="Enter the new output folder for Pivot collections.")][string]$outputFolder
	)
    try
    {
		Open-ZentityHostConfigurationFile
		SetZentityOutputFolderLocation $outputFolder
		Close-ZentityHostConfigurationFile

		Open-ZentityPivotGalleryConfigurationFile $path $zentityPivotGallerySite $remoteServer
		SetZentityCollectionFilePath $outputFolder
		Close-ZentityPivotGalleryConfigurationFile

		$global:adminConfigServiceClientFlag = $false
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}


# Function to configure the output folder for Pivot collections
function SetZentityOutputFolderLocation
{
    <#  
    .SYNOPSIS  
        Zentity Console : SetZentityOutputFolderLocation
    .DESCRIPTION  
        Sets the Output Folder where the Cxml's are to be generated
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        SetZentityOutputFolderLocation 'C:\Publishing\'
        Output: True
    #>
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the new output folder for Pivot collections.")][string]$outputFolder
    ) 

	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityHostConfigurationFile"
		if(CheckConfigurationFileOpened("HostConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService
			$global:adminConfigService.SetOutputFolderLocation($outputFolder)
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Host Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to set the Collection file path.
function SetZentityCollectionFilePath
{
    <#  
    .SYNOPSIS  
        Zentity Console : SetZentityCollectionFilePath
    .DESCRIPTION  
        Sets the Collection file path
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        SetZentityCollectionFilePath 'C:\Publishing\'
        Output: 
    #>
    
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Please enter the file path for the collection.")][string]$filePath
    )

	try
	{
		Find-ZentityAdminConfigServiceClient "Open-ZentityPivotGalleryConfigurationFile"
		if(CheckConfigurationFileOpened("PivotGalleryConfigurationFile"))
		{
			$global:adminConfigService = GetAdminConfigServiceProxy $adminConfigServiceUri $global:adminConfigService	
			$global:adminConfigService.SetCollectionFilePath($filePath)
			$global:adminConfigServiceClientFlag = $false
		}
		else
		{
			throw (New-Object System.InvalidOperationException("Zentity : Cannot use this function because Pivot Gallery Configuration File is not opened."))
		}
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:adminConfigServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}


write-host
write-host -ForegroundColor yellow "Zentity Console : Admin Configuration Service"
write-host
write-host -ForegroundColor yellow "       -- function list --           "
write-host
get-command -noun zentity* | write-host -ForegroundColor green