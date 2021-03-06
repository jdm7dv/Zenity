# Powershell command parameter to stop execution if an error is encountered
$script:ErrorActionPreference = "Stop"

# Initialize variables for the script
[string]$scriptLocation = [IO.Path]::GetDirectoryName($myinvocation.mycommand.path)
[string]$resourceTypeServiceProxy = [IO.Path]::Combine($scriptLocation, "ResourceTypeService.cs")
[string]$resourceTypeServiceAssembly = [IO.Path]::Combine($scriptLocation, "ResourceTypeService.dll")
[string]$resourceTypeServiceUri
[string]$commonScript = [IO.Path]::Combine($scriptLocation, "Common.ps1")
[bool]$global:resourceTypeServiceClientFlag = $false

# Import Common.ps1
. $commonScript 

# Check whether the service uri is present in config file
if(!(Check-ZentityServiceExists "ResourceTypeService"))
{
	# Read that the service uri from the user
	$resourceTypeServiceUri = Read-Host "Zentity : Please enter the endpoint uri for the Resource Type service."
	if (!$resourceTypeServiceUri -or [string]::IsNullOrWhiteSpace($resourceTypeServiceUri))
	{
		 throw (New-Object System.ArgumentNullException("Zentity : The endpoint uri value for the Resource Type service was missing."))
	}
	if(Add-ZentityServiceToConfig "ResourceTypeService" $resourceTypeServiceUri)
	{
		$resourceTypeServiceUri = Get-ZentityServiceUriForService "ResourceTypeService"
	}
}
else
{
	$resourceTypeServiceUri = Get-ZentityServiceUriForService "ResourceTypeService"
}

# Check if service proxy assembly is generated. If not, then re-create them 
# by downloading the WSDL and then compiling the source code
if (!(test-path $resourceTypeServiceAssembly))
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
	$wsdlUri = $resourceTypeServiceUri + "?wsdl"

	#Create the namespace for the proxy classes
	$resourceTypeServiceNamespace = "*,Zentity.Services.Web.Data"

	# Create the proxy class for the service
		svcutil.exe /n:$resourceTypeServiceNamespace $wsdlUri /out:$resourceTypeServiceProxy /ct:System.Collections.Generic.List``1 /config:ResourceTypeService.config
	
	if (!(test-path $resourceTypeServiceProxy))
	{
		throw (New-Object System.ApplicationException("Unable to create the proxy class from the service uri : $wsdlUri"))
	}

	# Generate the assembly from the proxy class
	csc /t:library /out:$resourceTypeServiceAssembly $resourceTypeServiceProxy
	
	if (!(test-path $resourceTypeServiceAssembly))
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
[Reflection.Assembly]::LoadFrom($resourceTypeServiceAssembly)
[Reflection.Assembly]::LoadWithPartialName("System.ServiceModel")
[Reflection.Assembly]::LoadWithPartialName("System.Configuration")

$global:resourceTypeService = GetResourceTypeServiceProxy $resourceTypeServiceUri $null

# Lists all resource types available within a particular data model(model namespace).
function Get-ZentityResourceTypesByNamespace
{
	<#  
	.SYNOPSIS  
		Zentity Console : Get-ZentityResourceTypesByNamespace
	.DESCRIPTION  
		Lists all resource types available within a particular data model(model namespace).
	.NOTES  
		Author : Microsoft
	.LINK  
		http://research.microsoft.com
	.EXAMPLE
		Get-ZentityResourceTypesByNamespace Zentity.ScholarlyWorks
		Output:
			ExtensionData        : System.Runtime.Serialization.ExtensionDataObject
			BaseType             : Zentity.Core.ResourceType
			Description          : Represents a letter.
			FullName             : Zentity.ScholarlyWorks.Letter
			Id                   : 3916037e-40ee-4eef-a4ee-0b071217c266
			Name                 : Letter
			NavigationProperties : {}
			ScalarProperties     : {}
			Uri                  : urn:zentity/module/zentity-scholarly-works/resource-type/letter

			ExtensionData        : System.Runtime.Serialization.ExtensionDataObject
			BaseType             : Zentity.Core.ResourceType
			Description          : Represents an image.
			FullName             : Zentity.ScholarlyWorks.Image
			Id                   : e82b8970-583f-42b5-83c4-0ff7135bb0e7
			Name                 : Image
			NavigationProperties : {}
			ScalarProperties     : {}
			Uri                  : urn:zentity/module/zentity-scholarly-works/resource-type/image

			ExtensionData        : System.Runtime.Serialization.ExtensionDataObject
			BaseType             : Zentity.Core.ResourceType
			Description          : Represents some binary data.
			FullName             : Zentity.ScholarlyWorks.Data
			Id                   : 346958a3-2ccb-4bdd-a810-1085c2ce3e65
			Name                 : Data
			NavigationProperties : {}
			ScalarProperties     : {}
			Uri                  : urn:zentity/module/zentity-scholarly-works/resource-type/data
	#>

	Param
	(
		[Parameter(Mandatory=$true,HelpMessage="Enter the Model Namespace.")][string]$modelNamespace
	)
	try
	{
		$global:resourceTypeService = GetResourceTypeServiceProxy $resourceTypeServiceUri $global:resourceTypeService
		$global:resourceTypeService.GetAllResourceTypesByNamespace($modelNamespace)
		$global:resourceTypeServiceClientFlag = $false
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:resourceTypeServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
	catch 
	{
		Write-Error $_.Exception.Message
		return
	}
}

# Lists all scalar properties available within a particular resource type for a data model(model namespace).
function Get-ZentityScalarPropertiesForResourceType
{
	<#  
	.SYNOPSIS  
		Zentity Console : Get-ZentityScalarPropertiesForResourceType
	.DESCRIPTION  
		Lists all scalar properties available within a particular resource type for a data model(model namespace).
	.NOTES  
		Author : Microsoft
	.LINK  
		http://research.microsoft.com
	.EXAMPLE
		Get-ZentityScalarPropertiesForResourceType Zentity.ScholarlyWorks Book
		Output:         
			ExtensionData     : System.Runtime.Serialization.ExtensionDataObject
			DataType          : String
			Description       : Gets or sets the change history of this book. E.g. the datetime information of when this book was created, edited etc.
			FullName          : Zentity.ScholarlyWorks.Book.ChangeHistory
			Id                : a23f94c6-6344-4ba6-ae90-63e49743a0d1
			IsFullTextIndexed : False
			MaxLength         : -1
			Name              : ChangeHistory
			Nullable          : True
			Precision         : 0
			Scale             : 0
			Uri               : urn:zentity/module/zentity-scholarly-works/resource-type/book/property/changehistory

			ExtensionData     : System.Runtime.Serialization.ExtensionDataObject
			DataType          : String
			Description       : Gets or sets the International Standard Book Number of this book.
			FullName          : Zentity.ScholarlyWorks.Book.ISBN
			Id                : bc13df5d-58dc-4fc7-8010-ec7eb04ac0c0
			IsFullTextIndexed : False
			MaxLength         : 256
			Name              : ISBN
			Nullable          : True
			Precision         : 0
			Scale             : 0
			Uri               : urn:zentity/module/zentity-scholarly-works/resource-type/book/property/isbn
	#>

	Param
	(
		[Parameter(Mandatory=$true,HelpMessage="Enter the Model Namespace Name.")][string]$modelNamespace,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Resource Type Name.")][string]$resourceTypeName
	)
	try
	{
		$global:resourceTypeService = GetResourceTypeServiceProxy $resourceTypeServiceUri $global:resourceTypeService
		$global:resourceTypeService.GetAllScalarPropertiesForResourceType($modelNamespace, $resourceTypeName)
		$global:resourceTypeServiceClientFlag = $false
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:resourceTypeServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
	catch 
	{
		Write-Error $_.Exception.Message
		return
	}
}

# Lists all navigation properties available within a particular resource type for a data model(model namespace).
function Get-ZentityNavigationPropertiesForResourceType
{
	<#  
	.SYNOPSIS  
		Zentity Console : Get-ZentityNavigationPropertiesForResourceType
	.DESCRIPTION  
		Lists all navigation properties available within a particular resource type for a data model(model namespace).
	.NOTES  
		Author : Microsoft
	.LINK  
		http://research.microsoft.com
	.EXAMPLE
		Get-ZentityNavigationPropertiesForResourceType Zentity.ScholarlyWorks Contact
		Output:        
			ExtensionData : System.Runtime.Serialization.ExtensionDataObject
			Description   : Gets a collection of related ScholarlyWork objects.
			Direction     : Object
			FullName      : Zentity.ScholarlyWorks.Contact.ContributionInWorks
			Id            : 830a7ee3-9ccb-45c1-9f26-0409e7ea3335
			Name          : ContributionInWorks
			Uri           : urn:zentity/module/zentity-scholarly-works/resource-type/contact/navigation-property/contributioninworks

			ExtensionData : System.Runtime.Serialization.ExtensionDataObject
			Description   : Gets a collection of related ScholarlyWork objects.
			Direction     : Object
			FullName      : Zentity.ScholarlyWorks.Contact.PresentedWorks
			Id            : 2b34beed-ee33-4241-878e-0dfbddb6a846
			Name          : PresentedWorks
			Uri           : urn:zentity/module/zentity-scholarly-works/resource-type/contact/navigation-property/presentedworks

			ExtensionData : System.Runtime.Serialization.ExtensionDataObject
			Description   : Gets a collection of related ScholarlyWork objects.
			Direction     : Object
			FullName      : Zentity.ScholarlyWorks.Contact.EditedWorks
			Id            : 47ae7643-7fe8-4b62-a6c3-1575efe19e5a
			Name          : EditedWorks
			Uri           : urn:zentity/module/zentity-scholarly-works/resource-type/contact/navigation-property/editedworks

			ExtensionData : System.Runtime.Serialization.ExtensionDataObject
			Description   : Gets a collection of related ScholarlyWorkItem objects.
			Direction     : Object
			FullName      : Zentity.ScholarlyWorks.Contact.AddedItems
			Id            : 77fee07f-3a0b-430c-8a39-bc5afb780557
			Name          : AddedItems
			Uri           : urn:zentity/module/zentity-scholarly-works/resource-type/contact/navigation-property/addeditems

			ExtensionData : System.Runtime.Serialization.ExtensionDataObject
			Description   : Gets a collection of related ScholarlyWork objects.
			Direction     : Object
			FullName      : Zentity.ScholarlyWorks.Contact.AuthoredWorks
			Id            : 569197b2-5501-404b-899b-f0f15a8b91f3
			Name          : AuthoredWorks
			Uri           : urn:zentity/module/zentity-scholarly-works/resource-type/contact/navigation-property/authoredworks
	#>

	Param
	(
		[Parameter(Mandatory=$true,HelpMessage="Enter the Model Namespace Name.")][string]$modelNamespace,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Resource Type Name.")][string]$resourceTypeName
	)
	try
	{
		$global:resourceTypeService = GetResourceTypeServiceProxy $resourceTypeServiceUri $global:resourceTypeService
		$global:resourceTypeService.GetAllNavigationPropertiesForResourceType($modelNamespace, $resourceTypeName)
		$global:resourceTypeServiceClientFlag = $false
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:resourceTypeServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
	catch 
	{
		Write-Error $_.Exception.Message
		return
	}
}

# Will update an existing data model resource type by deleting a scalar property.
function Delete-ZentityScalarPropertyOfResourceType
{
	<#  
	.SYNOPSIS  
		Zentity Console : Get-Delete-ZentityScalarPropertyOfResourceType
	.DESCRIPTION  
		Will update an existing data model resource type by deleting a scalar property.
	.NOTES  
		Author : Microsoft
	.LINK  
		http://research.microsoft.com
	.EXAMPLE
		Delete-ZentityScalarPropertyOfResourceType Zentity.ScholarlyWorks Book ISBN
		Output: True
	#>

	Param
	(
		[Parameter(Mandatory=$true,HelpMessage="Enter the Model Namespace Name.")][string]$modelNamespace,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Resource Type Name.")][string]$resourceTypeName,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Property Name.")][string]$propertyName
	)
	try
	{
		$global:resourceTypeService = GetResourceTypeServiceProxy $resourceTypeServiceUri $global:resourceTypeService
		$global:resourceTypeService.DeleteScalarPropertyOfResourceType($modelNamespace, $resourceTypeName, $propertyName)
		$global:resourceTypeServiceClientFlag = $false
		Write-Host $true
		Write-Host "A new Data Model has been added and an Update is required. Press any key to continue" -ForegroundColor red
		[System.Console]::ReadKey($true) | Out-Null
		Update-ZentityDataModels
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:resourceTypeServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
	catch 
	{
		Write-Error $_.Exception.Message
		return
	}
}

# Will update an existing data model resource type by deleting a navigation property. 
function Delete-ZentityNavigationPropertyOfResourceType
{
	<#  
	.SYNOPSIS  
		Zentity Console : Delete-ZentityNavigationPropertyOfResourceType
	.DESCRIPTION  
		Will update an existing data model resource type by deleting a navigation property.
	.NOTES  
		Author : Microsoft
	.LINK  
		http://research.microsoft.com
	.EXAMPLE
		Delete-ZentityNavigationPropertyOfResourceType Zentity.ScholarlyWorks Contact EditedItems
		Output: True
	#>

	Param
	(
		[Parameter(Mandatory=$true,HelpMessage="Enter the Model Namespace Name.")][string]$modelNamespace,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Resource Type Name.")][string]$resourceTypeName,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Property Name.")][string]$propertyName
	)
	try
	{
		$global:resourceTypeService = GetResourceTypeServiceProxy $resourceTypeServiceUri $global:resourceTypeService
		$global:resourceTypeService.DeleteNavigationPropertyOfResourceType($modelNamespace, $resourceTypeName, $propertyName)
		$global:resourceTypeServiceClientFlag = $false
		Write-Host $true
		Write-Host "A new Data Model has been added and an Update is required. Press any key to continue" -ForegroundColor red
		[System.Console]::ReadKey($true) | Out-Null
		Update-ZentityDataModels
	}
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:resourceTypeServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
	catch 
	{
		Write-Error $_.Exception.Message
		return
	}
}

# Will update an existing data model resource type by adding a scalar property.
function Add-ZentityScalarPropertyToResourceType
{
	<#  
	.SYNOPSIS  
		Zentity Console : Add-ZentityScalarPropertyToResourceType
	.DESCRIPTION  
		Will update an existing data model resource type by adding a scalar property.
	.NOTES  
		Author : Microsoft
	.LINK  
		http://research.microsoft.com
	.EXAMPLE
		Add-ZentityScalarPropertyToResourceType Zentity.Sample Book Price Double $null 2 $null
		Output: True
	#>

	Param
	(
		[Parameter(Mandatory=$true,HelpMessage="Enter the Model Namespace Name.")][string]$modelNamespace,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Resource Type Name.")][string]$resourceTypeName,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Property Name.")][string]$propertyName,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Data Type.")][Zentity.Services.Web.Data.DataTypes]$dataType,
		[Parameter(Mandatory=$false,HelpMessage="Enter the Max length for the property.")][int]$maxLength,
		[Parameter(Mandatory=$false,HelpMessage="Enter the precision for the property.")][int]$precision,
		[Parameter(Mandatory=$false,HelpMessage="Enter the scale for the property.")][int]$scale
	)
	
   try
   {
		$global:resourceTypeService = GetResourceTypeServiceProxy $resourceTypeServiceUri $global:resourceTypeService
		$global:resourceTypeService.AddScalarPropertyToResourceType($modelNamespace, $resourceTypeName, $propertyName, $dataType, $maxLength, $precision, $scale)
		$global:resourceTypeServiceClientFlag = $false
		Write-Host $true
		Write-Host "A new Data Model has been added and an Update is required. Press any key to continue" -ForegroundColor red
		[System.Console]::ReadKey($true) | Out-Null
		Update-ZentityDataModels
   }
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:resourceTypeServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
	catch 
	{
		Write-Error $_.Exception.Message
		return
	}
}

# Will update an existing data model resource type by adding a navigation property.
function Add-ZentityNavigationPropertyToResourceType
{
	<#  
	.SYNOPSIS  
		Zentity Console : Add-ZentityNavigationPropertyToResourceType
	.DESCRIPTION  
		Will update an existing data model resource type by adding a navigation property.
	.NOTES  
		Author : Microsoft
	.LINK  
		http://research.microsoft.com
	.EXAMPLE
		Add-ZentityNavigationPropertyToResourceType Zentity.Sample Book Person ReadBy AddedBy Authoring Many ZeroOrOne
		Output: True
	#>

	Param
	(
		[Parameter(Mandatory=$true,HelpMessage="Enter the Model Namespace Name.")][string]$modelNamespace,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Subject Resource Type Name.")][string]$subjectResourceType,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Object Resource Type Name.")][string]$objectResourceType,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Subject Navigation Property Name.")][string]$subjectNavigationPropertyName,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Object Navigation Property Name.")][string]$objectNavigationPropertyName,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Association Name.")][string]$associationName,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Subject Multiplicity.")][Zentity.Services.Web.Data.AssociationEndMultiplicity]$subjectMultiplicity,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Object Multiplicity.")][Zentity.Services.Web.Data.AssociationEndMultiplicity]$objectMultiplicity
	) 
	try
	{
		$global:resourceTypeService = GetResourceTypeServiceProxy $resourceTypeServiceUri $global:resourceTypeService
		$global:resourceTypeService.AddNavigationPropertyToResourceType($modelNamespace, $subjectResourceType, $objectResourceType, $subjectNavigationPropertyName, $objectNavigationPropertyName, $associationName, $subjectMultiplicity, $objectMultiplicity)
		$global:resourceTypeServiceClientFlag = $false
		Write-Host $true
		Write-Host "A new Data Model has been added and an Update is required. Press any key to continue" -ForegroundColor red
		[System.Console]::ReadKey($true) | Out-Null
		Update-ZentityDataModels
   }
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:resourceTypeServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
	catch 
	{
		Write-Error $_.Exception.Message
		return
	}
}

# Function to get Resource Count for a resource type
function Get-ZentityResourceCountForResourceType
{
	<#  
	.SYNOPSIS  
		Zentity Console : Get-ZentityResourceCountResourceType
	.DESCRIPTION  
		Will get Resource Count for a resource type.
	.NOTES  
		Author : Microsoft
	.LINK  
		http://research.microsoft.com
	.EXAMPLE
		Get-ZentityResourceCountResourceType Zentity.ScholarlyWorks Book
		Output:  250
	#>
   Param
   (
		[Parameter(Mandatory=$true,HelpMessage="Enter the Model Namespace Name.")][string]$modelNamespace,
		[Parameter(Mandatory=$true,HelpMessage="Enter the Resource Type Name.")][string]$resourceTypeName
   )
   try
   {
		$global:resourceTypeService = GetResourceTypeServiceProxy $resourceTypeServiceUri $global:resourceTypeService
		$global:resourceTypeService.GetResourceCountForResourceType($modelNamespace, $resourceTypeName)
		$global:resourceTypeServiceClientFlag = $false
   }
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:resourceTypeServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
	catch 
	{
		Write-Error $_.Exception.Message
		return
	}
}

# Function to get Resource Count for a data model
function Get-ZentityResourceCountForDataModel
{
	<#  
	.SYNOPSIS  
		Zentity Console : Get-ZentityResourceCountForDataModel
	.DESCRIPTION  
		Will get Resource Count for a data model.
	.NOTES  
		Author : Microsoft
	.LINK  
		http://research.microsoft.com
	.EXAMPLE
		Get-ZentityResourceCountForDataModel Zentity.Core
		Output:  
			Key                                           Value
			---                                           -----
			Zentity.Core.File                              1111
			Zentity.Core.Resource                         17518
			---------------------------------------------------
			Total:                         1111
				.EXAMPLE
					Get-ZentityResourceCountForDataModel {"Zentity.Core", "Zentity.ScholarlyWorks", "PivotDemo.Model"}
					Output:  
			Key                                           Value
			---                                           -----
			Zentity.ScholarlyWorks.Audio                     0
			Zentity.ScholarlyWorks.Book                    157
			Zentity.ScholarlyWorks.Booklet                   1
			Zentity.ScholarlyWorks.CategoryNode              0
			Zentity.ScholarlyWorks.Chapter                   0
			Zentity.ScholarlyWorks.Code                      0
			Zentity.ScholarlyWorks.Contact                7476
			Zentity.ScholarlyWorks.Data                      0
			Zentity.ScholarlyWorks.Download                  0
			Zentity.ScholarlyWorks.ElectronicSource          0
			Zentity.ScholarlyWorks.Email                     0
			Zentity.ScholarlyWorks.Experiment                0
			Zentity.ScholarlyWorks.Image                     0
			Zentity.ScholarlyWorks.Journal                   0
			Zentity.ScholarlyWorks.JournalArticle         1354
			Zentity.ScholarlyWorks.Lecture                   0
			Zentity.ScholarlyWorks.Letter                    0
			Zentity.ScholarlyWorks.Manual                   13
			Zentity.ScholarlyWorks.Media                     0
			--------------------------------------------------
			Total:                             12043
	#>
   Param
   (
		[Parameter(Mandatory=$true,HelpMessage="Enter the Model Namespace Name.")][string[]]$modelNamespaces
   )   
   try
   {
		[string[]] $localMMN = $modelNamespaces
		[int]$totalUniqueCount = 0
		$global:resourceTypeService = GetResourceTypeServiceProxy $resourceTypeServiceUri $global:resourceTypeService
		$global:resourceTypeService.GetResourceCountForDataModel([ref]$totalUniqueCount, $localMMN)
		$global:resourceTypeServiceClientFlag = $false
		Write-host "-------------------------------------------------------------------------------"
		Write-Host "Total: " $totalUniqueCount
   }
	catch [System.ServiceModel.Security.MessageSecurityException]
	{
		$global:resourceTypeServiceClientFlag = $true
		Write-Host $global:authenticationError -Foreground Red
	}
	catch 
	{
		Write-Error $_.Exception.Message
		return
	}
}

write-host
write-host -ForegroundColor yellow "Zentity Console : Resource Type Service"
write-host
write-host -ForegroundColor yellow "       -- function list --           "
write-host
get-command -noun zentity* | write-host -ForegroundColor green