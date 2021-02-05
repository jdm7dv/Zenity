# Powershell command parameter to stop execution if an error is encountered
$script:ErrorActionPreference = "Stop"

# Initialize variables for the script
[string]$scriptLocation = [IO.Path]::GetDirectoryName($myinvocation.mycommand.path)
[string]$tempPath = [IO.Path]::GetTempPath()
[string]$dataServicesProxy = [IO.Path]::Combine($tempPath, "DataServices.cs")
[string]$dataServicesAssembly = [IO.Path]::Combine($tempPath, "DataServices.dll")
[string]$commonScript = [IO.Path]::Combine($scriptLocation, "Common.ps1")
    
# Import Common.ps1
. $commonScript 

# Read that the service uri from the user
[string]$dataServicesUri = Read-Host "Zentity : Please enter the endpoint uri for the Data Service."
if (!$dataServicesUri -or [string]::IsNullOrWhiteSpace($dataServicesUri))
{
     throw (New-Object System.ArgumentNullException("Zentity : The endpoint uri value for the Data Model service was missing."))
}

# Re-Create service proxy assembly by downloading the WSDL and then compiling the source code
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
    $wsdlUri = $dataServicesUri
 
    # Create the proxy class for the service
    datasvcutil.exe /uri:$wsdlUri /out:$dataServicesProxy
    
    if (!(test-path $dataServicesProxy))
    {
        throw (New-Object System.ApplicationException("Unable to create the proxy class from the service uri : $wsdlUri"))
    }

    # Generate the assembly from the proxy class
    csc /t:library /out:$dataServicesAssembly $dataServicesProxy /r:"System.Data.Services.dll" /r:"System.Data.Services.Client.dll"
    
    if (!(test-path $dataServicesAssembly))
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

# Load the required assembly and the proxy assembly
[Reflection.Assembly]::LoadFrom($dataServicesAssembly)
[Reflection.Assembly]::LoadWithPartialName("System.ServiceModel")

$global:dataModelService = New-Object  Zentity.Core.ZentityContext($dataServicesUri)
$global:dataModelService.Credentials = [System.Net.CredentialCache]::DefaultCredentials

function Get-ZentityDataServiceContext
{
    <#  
    .SYNOPSIS  
        Zentity Console : Get-ZentityDataServiceContext
    .DESCRIPTION  
        Function to get the ZentityContext for a specified DataServiceUri
    .NOTES  
        Author : Microsoft
    .LINK  
        http://research.microsoft.com
    .EXAMPLE
        $zentityContext = Get-ZentityDataServiceContext    
         foreach($x in $zentityContext.Book) 
        { 
            Write-Host $x.Title 
        }
        
        Output:
        New Book
        Book_A
        NewBook000
        Type1_A
        New Book
    .EXAMPLE
        $zentityContext = Get-ZentityDataServiceContext    
        $newBook = [Zentity.ScholarlyWorks.Book]::CreateBook([System.Guid]::NewGuid())
        $b.Title = "Book added via Console"
        $b.Description = "This book has been added via a poweshell console using DataServices
        $zentityContext.AddToBook($b)
        $zentityContext.SaveChanges()
        
        Output:
        Descriptor                                         Headers                                                   StatusCode         Error                                
        ----------                                         -------                                                   ----------        ------
        System.Data.Services.Client.EntityD... {[DataServiceVersion, 1.0;], [Cache...                                    201                                      
    #>
    
    return $global:dataModelService
}

write-host
write-host -ForegroundColor yellow "Zentity Console : Data Modeling Service"
write-host
write-host -ForegroundColor yellow "       -- function list --           "
write-host
get-command -noun zentity* | write-host -ForegroundColor green