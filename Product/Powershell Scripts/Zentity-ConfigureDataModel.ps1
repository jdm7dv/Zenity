# Powershell command parameter to stop execution if an error is encountered
$script:ErrorActionPreference = "Stop"

# Initialize variables for the script
[string]$scriptLocation = [IO.Path]::GetDirectoryName($myinvocation.mycommand.path)
[string]$dataModelServiceScript = [IO.Path]::Combine($scriptLocation, "DataModelService.ps1")
[string]$resourceTypeServiceScript = [IO.Path]::Combine($scriptLocation, "ResourceTypeService.ps1")
[string]$tempPath = [System.IO.Path]::GetTempPath() + [System.Guid]::NewGuid()
[string]$global:installPath = [string]::Empty
[Reflection.Assembly]::LoadWithPartialName("System.Web")
$publishingServiceName = "PublishingService"

# Load the DataModelService for Generating the Hierarchical and Flattened assemblies
. $dataModelServiceScript

$serviceController = New-Object System.ServiceProcess.ServiceController($publishingServiceName, $global:dataModelServerName)

function Create-ZentityDataModelClientSetup
{ 
    <#  
    .SYNOPSIS  
        Zentity Console : Create-ZentityDataModelClientSetup
    .DESCRIPTION  
        Function to setup zentity clients with a data model
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Create-ZentityDataModelClientSetup 'C:\RDF\rdf.xml' 'C:\RDF\schema.xsd' Zentity.Sample
        Output: 
            Generation of Assemblies done.
            Copying of Assemblies Done.
            Configuration files Update Complete.
            Zentity Clients are ready to be used by the new Assemblies
    #>
    Param
    (
        [Parameter(Mandatory=$true,HelpMessage="Enter the RDFS Xml document string.")][string]$rdfsXmlFileLocation,
        [Parameter(Mandatory=$true,HelpMessage="Enter the Schema Xml document string.")][string]$schemaXmlFileLocation,
        [Parameter(Mandatory=$true,HelpMessage="Enter the namespace of the model.")][string]$modelNamespace
    )
    
    if((Create-ZentityDataModelFromXmlFiles $rdfsXmlFileLocation $schemaXmlFileLocation $modelNamespace) -eq $true)
    {
        Write-Host "Data Model Generation Done."
    }
    else
    {
        Write-Host "Error while creating Data model. This might be due to the xml files not found or empty."

		if($serviceController -ne $null)
		{
			if($serviceController.Status -eq [System.ServiceProcess.ServiceControllerStatus]::Stopped)
			{
				Write-Host "Starting the Publishing Service. Please wait"
				$serviceController.Start()
				$serviceController.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running)
				$global:dataModelService = GetDataModelServiceProxy $dataModelServiceUri $null
			}
		}
    }
} 


function Update-ZentityDataModelClientSetup
{ 
    <#  
    .SYNOPSIS  
        Zentity Console : Update-ZentityDataModelClientSetup
    .DESCRIPTION  
        Function to setup zentity clients with a data model
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        Update-ZentityDataModelClientSetup
        Output: 
            Generation of Assemblies done.
            Copying of Assemblies Done.
            Configuration files Update Complete.
            Zentity Clients are ready to be used by the new Assemblies
    #>    
     try
    {
            [String[]]$dataModelsFromDB = Get-ZentityDataModels
            $localArrayList = New-Object System.Collections.ArrayList
            $localArrayList.AddRange($dataModelsFromDB)
            if($localArrayList.Contains("Zentity.Core")) { $localArrayList.Remove("Zentity.Core") }
            if($localArrayList.Contains("Zentity.Security.Authorization")) { $localArrayList.Remove("Zentity.Security.Authorization") }
            if(!$localArrayList.Contains("Zentity.ScholarlyWorks")) { $localArrayList.Add("Zentity.ScholarlyWorks") }
            $dataModelsFromDB = $localArrayList
            
            # Create Directories in Temp folder for the assemblies
            $hierarchicalAssemblyPath = $tempPath + "\Hierarchical\"
            $flattenedAssemblyPath = $tempPath + "\Flattened\"
            [System.IO.Directory]::CreateDirectory($hierarchicalAssemblyPath)
            [System.IO.Directory]::CreateDirectory($flattenedAssemblyPath)
            
            foreach($item in $dataModelsFromDB)
            {
                if(!$item.Equals("Zentity.ScholarlyWorks"))
                {
                    Generate-ZentityHierarchicalAssemblies $item $null $item $hierarchicalAssemblyPath
                }
                
                Generate-ZentityFlattenedAssemblies $item $null $item $flattenedAssemblyPath
            }
            
            Generate-ZentityHierarchicalMetadataAssembly $dataModelsFromDB $hierarchicalAssemblyPath
            Generate-ZentityFlattenedMetadataAssembly $dataModelsFromDB $flattenedAssemblyPath
            
            Write-Host "Generation of Assemblies done."
            
			if($serviceController.ServiceName -ne $null)
			{ 
				$serviceController.Refresh()
				if($serviceController.Status -eq [System.ServiceProcess.ServiceControllerStatus]::Running)
				{
					$serviceController.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running)
					Write-Host "Stopping the Publishing Service. Please wait"
					$serviceController.Stop()
					$serviceController.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Stopped)
					$serviceController.Refresh()
					
					if($serviceController.Status -eq [System.ServiceProcess.ServiceControllerStatus]::Stopped)
					{
						[System.Threading.Thread]::Sleep([System.TimeSpan]::FromSeconds(5))
						CopyAssemblies
						UpdateConfigurations
					}
					else
					{
						Write-Host "Could not update assemblies and config files since the service could not be stopped"
					}

					Write-Host "Starting the Publishing Service. Please wait"
					$serviceController.Start()
					$serviceController.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running)
				}
				if($serviceController.Status -eq [System.ServiceProcess.ServiceControllerStatus]::Stopped)
				{
					Write-Host "Starting the Publishing Service. Please wait"
					$serviceController.Start()
					$serviceController.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running)
				}

				$global:dataModelService = GetDataModelServiceProxy $dataModelServiceUri $null
			}
                
            Write-Host "Zentity Clients are ready to be used by the new Assemblies"
    }
    catch
    {
        Write-Host $_.Exception.Message -Foreground Red
		if($serviceController -ne $null)
		{
			if($serviceController.Status -eq [System.ServiceProcess.ServiceControllerStatus]::Stopped)
			{
				Write-Host "Starting the Publishing Service. Please wait"
				$serviceController.Start()
				$serviceController.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running)
			}
		}

		$global:dataModelService = GetDataModelServiceProxy $dataModelServiceUri $global:dataModelService
    }
    
} 

function CopyAssemblies
{
    try
    {
        #Read the Console config to get the installed path
        if(!(Check-ZentityServiceExists "InstallPath"))
        {
            # Read that the install path from the user
            $global:installPath = Read-Host "Zentity : Please enter the install root."
            if (!$global:installPath -or [string]::IsNullOrWhiteSpace($global:installPath) -or !([System.IO.Directory]::Exists($global:installPath)))
            {
                throw (New-Object System.ArgumentNullException("Zentity : The install path is missing or directory does not exists."))
            }
            if(Add-ZentityServiceToConfig "InstallPath" $global:installPath)
            {
                $global:installPath = Get-ZentityServiceUriForService "InstallPath"
            }
        }
        else
        {
            $global:installPath = Get-ZentityServiceUriForService "InstallPath"
        }
        
        [string]$localInstallPath = $global:installPath+"\ZentityWebsite\DataService\bin\"
        
        # Copy the Generated assemblies to the respective folders
        CopyFiles $flattenedAssemblyPath $localInstallPath
        $localInstallPath = $global:installPath+"\Pivot\Publishing\"
        CopyFiles $hierarchicalAssemblyPath $localInstallPath
        $localInstallPath = $global:installPath+"\ZentityWebsite\VisualExplorer\bin\"
        CopyFiles $hierarchicalAssemblyPath $localInstallPath
        
        Write-Host "Copying of Assemblies Done."
        
        #Clean up delete the temp folder
        [System.IO.Directory]::Delete($tempPath, $true)
    }
    catch
    {
        Write-Host $_.Exception.Message -Foreground Red
    }
}

function UpdateConfigurations
{
    try
    {
        [string]$iisWebsite = [string]::Empty
        #Read the Console config to get the installed path
        if(!(Check-ZentityServiceExists "ZentityWebsite"))
        {
            # Read that the install path from the user
            $iisWebsite = Read-Host "Zentity : Please enter the install root."
            if (!$iisWebsite -or [string]::IsNullOrWhiteSpace($iisWebsite))
            {
                throw (New-Object System.ArgumentNullException("Zentity : The Zentity Website name is null or empty."))
            }
            if(Add-ZentityServiceToConfig "ZentityWebsite" $iisWebsite)
            {
                $iisWebsite = Get-ZentityServiceUriForService "ZentityWebsite"
            }
        }
        else
        {
            $iisWebsite = Get-ZentityServiceUriForService "ZentityWebsite"
        }
        
        # Update the configuration files for the services
        # UpdateConfig "/DataService" $iisWebsite
        UpdateConfig "/VisualExplorer" $iisWebsite
        
        [string]$zentityServicesHostInstallPath = $global:installPath+"\Pivot\Publishing\Zentity.Services.ServiceHost.exe"
        UpdateConfigExe $zentityServicesHostInstallPath

		[string]$collectionCreatorInstallPath = $global:installPath+"\Pivot\Publishing\Zentity.Pivot.CollectionCreator.exe"
        UpdateConfigExe $collectionCreatorInstallPath
        
        Write-Host "Configuration files Update Complete."    
    }
    catch
    {
        Write-Host $_.Exception.Message -Foreground Red
    }
}

function CopyFiles
{
    Param
    (
        [Parameter(Mandatory=$true)][string]$sourceFolder,
        [Parameter(Mandatory=$true)][string]$destinationFolder
    )
    
    try
    {
        if(([System.IO.Directory]::Exists($sourceFolder)) -and ([System.IO.Directory]::Exists($destinationFolder)))
        {
            [String[]]$files = [System.IO.Directory]::GetFiles($sourceFolder)
            if(($files -ne $null) -or ($files.Length -gt 0))
            {
                foreach($s in $files)
                {
                    $fileName = [System.IO.Path]::GetFileName($s)
                    $destFile = [System.IO.Path]::Combine($destinationFolder, $fileName)
                    [System.IO.File]::Copy($s, $destFile, $true)
                }
            }
        }
    }
    catch 
	{
        Write-Host $_.Exception.Message -Foreground Red
    }
}

function UpdateConfig
{
    Param
    (
        [Parameter(Mandatory=$true)][string]$path,
        [Parameter(Mandatory=$true)][string]$site
    )
    
    try
    {
        [System.Configuration.Configuration] $configuration = [System.Web.Configuration.WebConfigurationManager]::OpenWebConfiguration($path, $site)
        if($configuration -ne $null)
        {
            [System.Configuration.ConnectionStringSettings] $connnectionSetting = $configuration.ConnectionStrings.ConnectionStrings["ZentityContext"]
            if($connnectionSetting -ne $null)
            {
                [string]$connectionString = $connnectionSetting.ConnectionString
                [Reflection.Assembly]::LoadWithPartialName("System.Data.Entity")
                [System.Data.EntityClient.EntityConnectionStringBuilder] $builder = New-Object System.Data.EntityClient.EntityConnectionStringBuilder($connectionString)
                if ($builder -ne $null)
                {
                    $builder.Metadata = "res://Zentity.Metadata";
                }
                
                $connnectionSetting.ConnectionString = $builder.ToString()
                $configuration.Save([System.Configuration.ConfigurationSaveMode]::Modified);
                return $true;
            }
        }
    }
    catch
    {
        return $false
    }
}

function UpdateConfigExe
{
    Param
    (
        [Parameter(Mandatory=$true)][string]$pathOfExe
    )
    
    try
    {    
        [System.Configuration.Configuration] $configuration = [System.Configuration.ConfigurationManager]::OpenExeConfiguration($pathOfExe)
        if($configuration -ne $null)
        {
            [System.Configuration.ConnectionStringSettings] $connnectionSetting = $configuration.ConnectionStrings.ConnectionStrings["ZentityContext"]
            if($connnectionSetting -ne $null)
            {
                [string]$connectionString = $connnectionSetting.ConnectionString
                [Reflection.Assembly]::LoadWithPartialName("System.Data.Entity")
                [System.Data.EntityClient.EntityConnectionStringBuilder] $builder = New-Object System.Data.EntityClient.EntityConnectionStringBuilder($connectionString)
                if ($builder -ne $null)
                {
                    $builder.Metadata = "res://Zentity.Metadata";
                }
                
                $connnectionSetting.ConnectionString = $builder.ToString()
                $configuration.Save([System.Configuration.ConfigurationSaveMode]::Modified, $false);
                return $true;
            }
        }
    }
    catch
    {
        return $false
    }
}

write-host
write-host -ForegroundColor yellow "Zentity Console : Create-ZentityDataModel"
write-host
write-host -ForegroundColor yellow "       -- function list --           "
write-host
get-command -noun zentity* | write-host -ForegroundColor green
