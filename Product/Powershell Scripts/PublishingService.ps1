# Powershell command parameter to stop execution if an error is encountered
$script:ErrorActionPreference = "Stop"

# Initialize variables for the script
[string]$scriptLocation = [IO.Path]::GetDirectoryName($myinvocation.mycommand.path)
[string]$publishingServiceProxy = [IO.Path]::Combine($scriptLocation, "PublishingService.cs")
[string]$publishingServiceAssembly = [IO.Path]::Combine($scriptLocation, "PublishingService.dll")
[string]$publishingServiceUri
[string]$commonScript = [IO.Path]::Combine($scriptLocation, "Common.ps1")
[bool]$global:pubServiceClientFlag = $false
    
# Load PublishStatus.Format.ps1xml into the session if it is not loaded
if((Get-FormatData Zentity.Services.Web.Pivot.PublishStatus) -eq $null)
{
	[string]$formatFilePath = [System.IO.Path]::Combine($scriptLocation, "PublishStatus.Format.ps1xml")
	Update-FormatData -AppendPath $formatFilePath
}

# Import Common.ps1
. $commonScript 

# Check whether the service uri is present in config file
if(!(Check-ZentityServiceExists "PublishingService"))
{
    # Read that the service uri from the user
    $publishingServiceUri = Read-Host "Zentity : Please enter the endpoint uri for the Publishing service."
    if (!$publishingServiceUri -or [string]::IsNullOrWhiteSpace($publishingServiceUri))
    {
         throw (New-Object System.ArgumentNullException("Zentity : The endpoint uri value for the Publishing service was missing."))
    }
    if(Add-ZentityServiceToConfig "PublishingService" $publishingServiceUri)
    {
        $publishingServiceUri = Get-ZentityServiceUriForService "PublishingService"
    }
}
else
{
    $publishingServiceUri = Get-ZentityServiceUriForService "PublishingService"
}

# Check if service proxy assembly is generated. If not, then re-create them 
# by downloading the WSDL and then compiling the source code
if (!(test-path $publishingServiceAssembly))
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
    $wsdlUri = $publishingServiceUri + "?wsdl"

	#Create the namespace for the proxy classes
	$publishingServiceNamespace = "*,Zentity.Services.Web.Pivot"

    # Create the proxy class for the service
    svcutil.exe /n:$publishingServiceNamespace $wsdlUri /out:$publishingServiceProxy /ct:System.Collections.Generic.List``1 /config:PublishingService.config
    
    if (!(test-path $publishingServiceProxy))
    {
        throw (New-Object System.ApplicationException("Unable to create the proxy class from the service uri : $wsdlUri"))
    }
    
    # Generate the assembly from the proxy class
    csc /t:library /out:$publishingServiceAssembly $publishingServiceProxy
    
    if (!(test-path $publishingServiceAssembly))
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
[Reflection.Assembly]::LoadFrom($publishingServiceAssembly)
[Reflection.Assembly]::LoadWithPartialName("System.ServiceModel")
[Reflection.Assembly]::LoadWithPartialName("System.Configuration")

$global:publishingService = GetPublishingServiceProxy $publishingServiceUri $null

# Function to create Pivot Collection for a Resource Type
function Create-ZentityPivotCollectionFromResourceType
{
    <#  
    .SYNOPSIS  
        Zentity Console : Create-ZentityPivotCollectionFromResourceType
    .DESCRIPTION  
        Function to create Pivot Collection for a Resource Type
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE 
        Create-ZentityPivotCollectionFromResourceType Zentity.ScholarlyWorks Manual
        Output:
        Request sent to publishing service. Please wait...
        --------------------------------------------------
        Instance ID :  60957cc8-9177-4503-bc2b-662961c473e2
        --------------------------------------------------
        Retrieving resource records from the database. Please wait...
        Resource Items fetched :  13
        Processing resource items. Please wait...
          -- completed  0.00 %
          -- completed  0.00 %
          -- completed  0.00 %
          -- operation completed.
        Creating images for each resource item. Images to create :  13
          -- Image creation in progress. Please wait...
          -- completed  0.00 %
          -- completed  100%
        Creating DeepZoom images. DeepZoom images to create :  13
          -- DeepZoom images creation in progress. Please wait...
          -- completed  0.00 %
          -- completed 100%
        Creating DeepZoom image collection. Please wait...
          -- operation completed.
        Completed collection creation for resource type.
    #>

    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the model namespace containing the resource type.")][string]$modelNamespace,
        [Parameter(Mandatory=$true,HelpMessage="Enter the resource type for which cxml is to be generated.")][string]$resourceType
    )
    try 
    {
		$global:publishingService = GetPublishingServiceProxy $publishingServiceUri $global:publishingService 
        $instanceId = $global:publishingService.CreateCollection($modelNamespace, $resourceType)
        Write-Host "Request sent to publishing service. Please wait..."
        Write-Host "--------------------------------------------------"
        Write-Host "Instance ID : " $instanceId.ToString()
        Write-Host "--------------------------------------------------"
		$global:pubServiceClientFlag = $false
    }
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:pubServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
    [Zentity.Services.Web.Pivot.PublishStatus]$currentStatus = $null
    [Zentity.Services.Web.Pivot.PublishStage]$prevStage = 'Initiating'
    [int]$retryCount = 0

    try 
    {
        [System.Threading.Thread]::Sleep([System.TimeSpan]::FromSeconds(5))
		$global:publishingService = GetPublishingServiceProxy $publishingServiceUri $global:publishingService 
        $currentStatus = $global:publishingService.GetQueuedRequestByInstanceID($instanceId)
		$global:pubServiceClientFlag = $false
        if ($currentStatus -ne $null)
        {
            Write-Host
            Write-Host "The publishing request with instance Id - " $instanceId " has been successfully queued in publishing service. The request will be processed later."
            Write-Host "You may check for the status of the above request via the following commandlets :-"
            Write-Host "  => Queued Request    : Get-ZentityQueuedRequestByInstanceID " $instanceId
            Write-Host "  => Processed Request : Get-ZentityPublishingStatusByInstanceID " $instanceId
            return;
        }
    }
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:pubServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch [System.TimeoutException]
    {
        Write-Error $_.Exception.Message
        if ($retryCount -le 10)
        {
            Write-Host "Retrying..."
            $retryCount = $retryCount + 1
            continue;
        }
        else
        {
            return;
        }
    }
    catch
    {
        Write-Error $_.Exception.Message
        return;
    }
        
    do
    {
        [System.Threading.Thread]::Sleep([System.TimeSpan]::FromSeconds(10))
        try 
        {
			$global:publishingService = GetPublishingServiceProxy $publishingServiceUri $global:publishingService 
            $currentStatus = $global:publishingService.GetPublishingStatusByInstanceID($instanceId)
			$global:pubServiceClientFlag = $false
            if ($currentStatus -eq $null)
            {
                Write-Error "The status for the publishing request with instance Id - " $instanceId " is not available."
                return;
            }
        }
		catch [System.ServiceModel.Security.MessageSecurityException]
		{
			$global:pubServiceClientFlag = $true
			Write-Host $global:authenticationError -Foreground Red
		}
        catch [System.TimeoutException]
        {
            Write-Error $_.Exception.Message
            if ($retryCount -le 10)
            {
                Write-Host "Retrying..."
                $retryCount = $retryCount + 1
                continue;
            }
            else
            {
                return;
            }
        }
        catch
        {
            Write-Error $_.Exception.Message
            return;
        }
        
        switch ($currentStatus.CurrentStage)
        {
            Initiating
            {
                Write-Host "Initiating collection creation for resource type..."
            }
            FetchingResourceItems
            {
                if ($prevStage -ne [Zentity.Services.Web.Pivot.PublishStage]::FetchingResourceItems)
                {
                    Write-Host "Retrieving resource records from the database. Please wait..."
                }
                $prevStage = 'FetchingResourceItems'
            }
            ProcessingResourceItems
            {
                if ($prevStage -ne [Zentity.Services.Web.Pivot.PublishStage]::ProcessingResourceItems)
                {
                    Write-Host "Resource Items fetched : " $currentStatus.ResourceItems.Total
                    Write-Host "Processing resource items. Please wait..."
                }
                Write-Host "  -- completed " (($currentStatus.ResourceItems.Completed * 100) / $currentStatus.ResourceItems.Total).ToString("0.00")"%"
                $prevStage = 'ProcessingResourceItems'
            }
            PublishIntermediateCollection
            {
                if ($prevStage -ne [Zentity.Services.Web.Pivot.PublishStage]::PublishIntermediateCollection)
                {
                    Write-Host "  -- completed  100%"
                    Write-Host "Publishing intermediate collection (with default deep zoom image). Please wait..."
                }
                $prevStage = 'PublishIntermediateCollection'
            }
            CreatingImages
            {
                if ($prevStage -ne [Zentity.Services.Web.Pivot.PublishStage]::CreatingImages)
                {
                    Write-Host "  -- operation completed."
                    Write-Host "Creating images for each resource item. Images to create : " $currentStatus.Images.Total
                    Write-Host "  -- Image creation in progress. Please wait..."
                }
                Write-Host "  -- completed " (($currentStatus.Images.Completed * 100) / $currentStatus.Images.Total).ToString("0.00")"%"
                $prevStage = 'CreatingImages'
            }
            CreatingDeepZoomImages
            {
                if ($prevStage -ne [Zentity.Services.Web.Pivot.PublishStage]::CreatingDeepZoomImages)
                {
                    Write-Host "  -- completed  100%"
                    Write-Host "Creating DeepZoom images. DeepZoom images to create : " $currentStatus.DeepZoomImages.Total
                    Write-Host "  -- DeepZoom images creation in progress. Please wait... "
                }
                Write-Host "  -- completed " (($currentStatus.DeepZoomImages.Completed * 100) / $currentStatus.DeepZoomImages.Total).ToString("0.00")"%"
                $prevStage = 'CreatingDeepZoomImages'
            }
            CreatingDeepZoomCollection
            {
                if ($prevStage -ne [Zentity.Services.Web.Pivot.PublishStage]::CreatingDeepZoomCollection)
                {
                    Write-Host "  -- completed 100%"
                    Write-Host "Creating DeepZoom image collection. Please wait..."
                }
                $prevStage = 'CreatingDeepZoomCollection'
            }
            DeletingExistingCollection
            {
                if ($prevStage -ne [Zentity.Services.Web.Pivot.PublishStage]::DeletingExistingCollection)
                {
                    Write-Host "  -- operation completed."
                    Write-Host "Deleting existing collection from output folder. Please wait..."
                }
                $prevStage = 'DeletingExistingCollection'
            }
            CopyingNewCollection
            {
                if ($prevStage -ne [Zentity.Services.Web.Pivot.PublishStage]::CopyingNewCollection)
                {
                    Write-Host "  -- operation completed."
                    Write-Host "Copying new collection to output folder. Please wait..."
                }
                $prevStage = 'CopyingNewCollection'
            }
            PerformingCleanup
            {
                if ($prevStage -ne [Zentity.Services.Web.Pivot.PublishStage]::PerformingCleanup)
                {
                    Write-Host "  -- operation completed."
                    Write-Host "Performing cleanup of collection and generated images from temporary working folder. Please wait..."
                }
                $prevStage = 'PerformingCleanup'
            }
            Completed
            {
                if ($prevStage -ne [Zentity.Services.Web.Pivot.PublishStage]::Completed)
                {
                    Write-Host "  -- operation completed."
                    Write-Host "Completed collection creation for resource type."
                }
                $prevStage = 'Completed'
            }
            AbortedOnError
            {
                Write-Error "Publishing was cancelled due to an error. Please check the publishing service logs for more information."
                return
            }
            AbortedOnDemand
            {
                Write-Error "Publishing was cancelled due to user's request."
                return
            }
        }
        
        if ($prevStage -eq [Zentity.Services.Web.Pivot.PublishStage]::Completed)
        {
            break;
        }
    } while($true)
}

# Function to delete an existing Pivot Collection for a Resource Type
function Delete-ZentityPivotCollectionForResourceType
{
    <#  
    .SYNOPSIS  
        Zentity Console : Delete-ZentityPivotCollectionForResourceType
    .DESCRIPTION  
        Function to delete an existing Pivot Collection for a Resource Type
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE 
        Delete-ZentityPivotCollectionForResourceType Zentity.ScholarlyWorks Booklet
    #>

    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the model namespace containing the resource type.")][string]$modelNamespace,
        [Parameter(Mandatory=$true,HelpMessage="Enter the resource type for which cxml is to be deleted.")][string]$resourceType
    )  
	
	try
	{
		$global:publishingService = GetPublishingServiceProxy $publishingServiceUri $global:publishingService
		$global:publishingService.DeletePublishedCollection($modelNamespace, $resourceType)
		$global:pubServiceClientFlag = $false
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:pubServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to cancel the create/update collection operation for a Resource Type. 
function Cancel-ZentityPublishingByResourceType
{
    <#  
    .SYNOPSIS  
        Zentity Console : Cancel-ZentityPublishingByResourceType
    .DESCRIPTION  
        Function to cancel the create/update collection operation for a Resource Type. 
        Return Type : True/False
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE 
        Cancel-ZentityPublishingByResourceType Zentity.ScholarlyWorks Person
        Output: 
			True

			# You can get the publishing status and check the CurrentStage. 
			# User cancelation will set the CurrentStage to AbortedOnDemand
			PS C:\Program Files (x86)\Zentity\PowerShell scripts> Get-ZentityCxmlPublishingStatusByResourceType Zentity.ScholarlyWorks Person
			ResourceType   : Zentity.ScholarlyWorks.Person
            InstanceId     : 96c7f27d-6740-4425-9e5d-404967993a38
            CurrentStage   : AbortedOnDemand
            StartTime      : 8/30/2010 4:07:39 PM
            EndTime        : 8/30/2010 4:10:25 PM
            ResourceItems  : Total     : 0
                             Completed : 0
            Images         : Total     : 0
                             Completed : 0
            DeepZoomImages : Total     : 0
                             Completed : 0
    #>
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the model namespace containing the resource type.")][string]$modelNamespace,
        [Parameter(Mandatory=$true,HelpMessage="Enter the resource type for which publishing has to be canceled.")][string]$resourceType
    ) 
	
	try
	{
		$global:publishingService = GetPublishingServiceProxy $publishingServiceUri $global:publishingService 
		$tempVar = $global:publishingService.CancelPublishRequestByResourceType($modelNamespace, $resourceType)
		$global:pubServiceClientFlag = $false
		return $tempVar
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:pubServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to cancel the create/update collection operation for an Instance Id. 
function Cancel-ZentityPublishingByInstanceID
{
    <#  
    .SYNOPSIS  
        Zentity Console : Cancel-ZentityPublishingByInstanceID
    .DESCRIPTION  
        Function to cancel the create/update collection operation for an Instance Id. 
        Return Type : True/False
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE 
        Cancel-ZentityPublishingByInstanceID c2a590e9-02f1-4ad9-bb32-764a756f2c0b
        Output: 
			True

			# You can get the publishing status and check the CurrentStage. 
			# User cancelation will set the CurrentStage to AbortedOnDemand
			PS C:\Program Files (x86)\Zentity\PowerShell scripts> Get-ZentityCxmlPublishingStatusByResourceType Contact
			ResourceType   : Zentity.ScholarlyWorks.Contact
            InstanceId     : c2a590e9-02f1-4ad9-bb32-764a756f2c0b
            CurrentStage   : AbortedOnDemand
            StartTime      : 8/30/2010 4:08:05 PM
            EndTime        : 8/30/2010 4:10:27 PM
            ResourceItems  : Total     : 0
                             Completed : 0
            Images         : Total     : 0
                             Completed : 0
            DeepZoomImages : Total     : 0
                             Completed : 0
    #>
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the instance Id of the request for which publishing has to be canceled.")][System.Guid]$instanceId
    ) 
	
	try
	{
		$global:publishingService = GetPublishingServiceProxy $publishingServiceUri $global:publishingService 
		$tempVar = $global:publishingService.CancelPublishRequestByInstanceID($instanceId)
		$global:pubServiceClientFlag = $false
		return $tempVar
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:pubServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get the status the of all create/update collection requests being currently processed.
function Get-ZentityPublishingStatus
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityPublishingStatus
    .DESCRIPTION  
        Function to get status the of all create/update collection requests being currently processed.
        Return Type : System.Collections.Generic.List[Zentity.Services.Web.Pivot.PublishStatus]
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE 
        Get-ZentityPublishingStatus
        Output:
        ResourceType   : Zentity.ScholarlyWorks.Manual
        InstanceId     : 60957cc8-9177-4503-bc2b-662961c473e2
        CurrentStage   : Completed
        StartTime      : 8/30/2010 6:27:10 PM
        EndTime        : 8/30/2010 6:28:33 PM
        ResourceItems  : Total     : 13
                         Completed : 13
        Images         : Total     : 13
                         Completed : 13
        DeepZoomImages : Total     : 13
                         Completed : 13
        
        ResourceType   : Zentity.ScholarlyWorks.Book
        InstanceId     : b29eea31-71d6-4fd6-a23b-849b2052a40e
        CurrentStage   : AbortedOnDemand
        StartTime      : 8/30/2010 4:08:29 PM
        EndTime        : 8/30/2010 4:09:59 PM
        ResourceItems  : Total     : 0
                         Completed : 0
        Images         : Total     : 0
                         Completed : 0
        DeepZoomImages : Total     : 0
                         Completed : 0

	#>
	
	try
	{
		$global:publishingService = GetPublishingServiceProxy $publishingServiceUri $global:publishingService
		$tempVar = $global:publishingService.GetAllPublishingStatus()
		$global:pubServiceClientFlag = $false
		return $tempVar
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:pubServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get the status of a create/update collection request being currently processed for a Resource Type.
function Get-ZentityPublishingStatusByResourceType
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityPublishingStatusByResourceType
    .DESCRIPTION  
        Function to get the status of a create/update collection request being currently processed for a Resource Type.
        Return Type : [Zentity.Services.Web.Pivot.PublishStatus]
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE 
        Get-ZentityPublishingStatusByResourceType Zentity.ScholarlyWorks Manual
        Output:
            ResourceType   : Zentity.ScholarlyWorks.Manual
            InstanceId     : 60957cc8-9177-4503-bc2b-662961c473e2
            CurrentStage   : Completed
            StartTime      : 8/30/2010 6:27:10 PM
            EndTime        : 8/30/2010 6:28:33 PM
            ResourceItems  : Total     : 13
                             Completed : 13
            Images         : Total     : 13
                             Completed : 13
            DeepZoomImages : Total     : 13
                             Completed : 13
	#>
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the model namespace containing the resource type.")][string]$modelNamespace,
        [Parameter(Mandatory=$true,HelpMessage="Enter the resource type for which publishing status needs to be fetched.")][string]$resourceType
    ) 
	
	try
	{
		$global:publishingService = GetPublishingServiceProxy $publishingServiceUri $global:publishingService 
		$tempVar = $global:publishingService.GetPublishingStatusByResourceType($modelNamespace, $resourceType)
		$global:pubServiceClientFlag = $false
		return $tempVar
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:pubServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get the status of a create/update collection request being currently processed for an Instance Id.
function Get-ZentityPublishingStatusByInstanceID
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityPublishingStatusByInstanceId
    .DESCRIPTION  
        Function to get the status of a create/update collection request being currently processed for an Instance Id.
        Return Type : [Zentity.Services.Web.Pivot.PublishStatus]
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE 
        Get-ZentityPublishingStatusByInstanceId 60957cc8-9177-4503-bc2b-662961c473e2
        Output:
            ResourceType   : Zentity.ScholarlyWorks.Manual
            InstanceId     : 60957cc8-9177-4503-bc2b-662961c473e2
            CurrentStage   : Completed
            StartTime      : 8/30/2010 6:27:10 PM
            EndTime        : 8/30/2010 6:28:33 PM
            ResourceItems  : Total     : 13
                             Completed : 13
            Images         : Total     : 13
                             Completed : 13
            DeepZoomImages : Total     : 13
                             Completed : 13
    #>
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the instance Id of the request for which publishing status needs to be fetched.")][System.Guid]$instanceId
    ) 
    
	try
	{
		$global:publishingService = GetPublishingServiceProxy $publishingServiceUri $global:publishingService 
		$tempVar = $global:publishingService.GetPublishingStatusByInstanceID($instanceId)
		$global:pubServiceClientFlag = $false
		return $tempVar
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:pubServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get the status of all create/update collection requests that are queued and not being processed.
function Get-ZentityQueuedRequests
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityQueuedRequests
    .DESCRIPTION  
        Function to get the status of all create/update collection requests that are queued and not being processed.
        Return Type : System.Collections.Generic.List[Zentity.Services.Web.Pivot.PublishStatus]
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE 
        Get-ZentityQueuedRequests
        Output:
        ResourceType   : Zentity.ScholarlyWorks.Person
        InstanceId     : 4ea4289a-3d92-466f-a51f-08dd4f8a2ea6
        CurrentStage   : NotStarted
        StartTime      : 8/30/2010 6:32:34 PM
        EndTime        : 1/1/0001 12:00:00 AM
        ResourceItems  : Total     : 0
                         Completed : 0
        Images         : Total     : 0
                         Completed : 0
        DeepZoomImages : Total     : 0
                         Completed : 0
        
        ResourceType   : Zentity.ScholarlyWorks.Contact
        InstanceId     : b29eea31-71d6-4fd6-a23b-849b2052a40e
        CurrentStage   : NotStarted
        StartTime      : 8/30/2010 6:32:34 PM
        EndTime        : 1/1/0001 12:00:00 AM
        ResourceItems  : Total     : 0
                         Completed : 0
        Images         : Total     : 0
                         Completed : 0
        DeepZoomImages : Total     : 0
                         Completed : 0

	#>
	
	try
	{
		$global:publishingService = GetPublishingServiceProxy $publishingServiceUri $global:publishingService 
		$tempVar = $global:publishingService.GetAllQueuedRequests()
		$global:pubServiceClientFlag = $false
		return $tempVar
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:pubServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get the status of all create/update collection requests that are queued and not being processed for a Resource Type.
function Get-ZentityQueuedRequestsByResourceType
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityQueuedRequestsByResourceType
    .DESCRIPTION  
	    Function to get the status the all create/update collection requests that are queued and not being processed for a Resource Type.
        Return Type : System.Collections.Generic.List[Zentity.Services.Web.Pivot.PublishStatus]
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
	.EXAMPLE
		Get-ZentityQueuedRequestsByResourceType Zentity.ScholarlyWorks Person
		Output:
        ResourceType   : Zentity.ScholarlyWorks.Person
        InstanceId     : 4ea4289a-3d92-466f-a51f-08dd4f8a2ea6
        CurrentStage   : NotStarted
        StartTime      : 8/30/2010 6:32:34 PM
        EndTime        : 1/1/0001 12:00:00 AM
        ResourceItems  : Total     : 0
                         Completed : 0
        Images         : Total     : 0
                         Completed : 0
        DeepZoomImages : Total     : 0
                         Completed : 0
    #>
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the model namespace containing the resource type.")][string]$modelNamespace,
        [Parameter(Mandatory=$true,HelpMessage="Enter the resource type for which queued requests need to be fetched.")][string]$resourceType
    ) 
	
	try
	{
		$global:publishingService = GetPublishingServiceProxy $publishingServiceUri $global:publishingService 
		$tempVar = $global:publishingService.GetQueuedRequestsByResourceType($modelNamespace, $resourceType)
		$global:pubServiceClientFlag = $false
		return $tempVar
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:pubServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

# Function to get the status of a create/update collection request that is queued and not being processed for an Instance ID.
function Get-ZentityQueuedRequestByInstanceID
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityQueuedRequestByInstanceID
    .DESCRIPTION  
	    Function to get the status of a create/update collection request that is queued and not being processed for an Instance ID.
        Return Type : [Zentity.Services.Web.Pivot.PublishStatus]
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
	.EXAMPLE
		Get-ZentityQueuedRequestByInstanceID 4ea4289a-3d92-466f-a51f-08dd4f8a2ea6
        Output:
        ResourceType   : Zentity.ScholarlyWorks.Person
        InstanceId     : 4ea4289a-3d92-466f-a51f-08dd4f8a2ea6
        CurrentStage   : NotStarted
        StartTime      : 8/30/2010 6:32:34 PM
        EndTime        : 1/1/0001 12:00:00 AM
        ResourceItems  : Total     : 0
                         Completed : 0
        Images         : Total     : 0
                         Completed : 0
        DeepZoomImages : Total     : 0
                         Completed : 0
    #>
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the instance Id of the queued request for which publishing status needs to be fetched.")][System.Guid]$instanceId
    ) 
	
	try
	{
		$global:publishingService = GetPublishingServiceProxy $publishingServiceUri $global:publishingService 
		$tempVar = $global:publishingService.GetQueuedRequestByInstanceID($instanceId)
		$global:pubServiceClientFlag = $false
		return $tempVar
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:pubServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

function Get-ZentityQueuedPositionByInstanceID
{

    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityQueuedPositionByInstanceID
    .DESCRIPTION  
	    Function to get the position in the Queue for the specified Instance ID.
            
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
	.EXAMPLE
		Get-ZentityQueuedPositionByInstanceID 4ea4289a-3d92-466f-a51f-08dd4f8a2ea6
        Output:
	Specified instance is in the Queue at position : 2
        
    #>
	Param
	(
 	[Parameter(Mandatory=$true,HelpMessage="Please enter Valid Instance ID")][System.Guid]$instanceId
	)	
 
	try
	{
 		$a = Get-ZentityQueuedRequestByInstanceID $instanceId
 		if($a -eq $null)
 		{
  	 		$b = Get-ZentityPublishingStatusByInstanceID $instanceId
  	 		if($b -eq $null)
  	 		{
       				Write-Host "Specified Instance Id is not valid"
       				return
   	 		}
   			Write-Host "Entered Instance is already sent for Publishing. Please find the current status as below"
   			Write-Host "Current Publishing Status : " 
  			$b
   			return
 		}
	
		$global:publishingService = GetPublishingServiceProxy $publishingServiceUri $global:publishingService 
		$x =  $global:publishingService.GetQueuedPositionByInstanceID($instanceId)

		if($x -eq -1)
		{
			Write-Host "There are no queued requests for the instance id provided."
		}
		else
		{
			Write-Host "Specified instance is in the Queue at position :" $x
			return
		}
		
		$global:pubServiceClientFlag = $false
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:pubServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
    catch 
    {
        Write-Error $_.Exception.Message
        return
    }
}

write-host
write-host -ForegroundColor yellow "Zentity Console : Publishing Service"
write-host
write-host -ForegroundColor yellow "       -- function list --           "
write-host
get-command -noun zentity* | write-host -ForegroundColor green
