@echo off
set inputDir=%~f1
set sourceDir=%~f1
IF EXIST "%sourceDir%" (ECHO Directory exists! 
) ELSE (
ECHO Bad Path Entered. Please specify one argument for the batch file.
Exit /B
)
IF "%2" == "" (set config="Debug"
) ELSE (
set config=%~2
)

set destDir="%inputDir%\Build_Target\Server\Database Scripts\"
ECHO --Copying Database scripts
xcopy "%sourceDir%\Product\Database Scripts" %destDir% /r /y /q /s /i
ECHO --Copying Database scripts completed


set destDir="%inputDir%\Build_Target\Server\Documentation\"
ECHO --Copying Documentation files 
xcopy "%sourceDir%\Documentation" %destDir% /r /y /q /s /i
ECHO --Copying Documentation files finished


set destDir="%inputDir%\Build_Target\Server\External Libraries\"
ECHO --Copying External Libraries 
xcopy "%sourceDir%\Product\External Libraries\GuanxiMapCore.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\External Libraries\GuanxiMap.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services\Zentity.Platform\bin\%config%\Microsoft.Practices.EnterpriseLibrary.Common.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services\Zentity.Platform\bin\%config%\Microsoft.Practices.EnterpriseLibrary.Caching.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services\Zentity.Platform\bin\%config%\Microsoft.Practices.EnterpriseLibrary.Data.dll" %destDir% /r /y /q
ECHO --Copying External Libraries finished


set destDir="%inputDir%\Build_Target\Server\Pivot.Services\Notification\"
ECHO --Copying Notification Service Binaries 
xcopy "%sourceDir%\Product\Zentity.Services.Windows\bin\%config%\Microsoft.Practices.EnterpriseLibrary.Common.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.Windows\bin\%config%\Microsoft.Practices.EnterpriseLibrary.Logging.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.Windows\bin\%config%\Microsoft.Practices.ServiceLocation.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.Windows\bin\%config%\Microsoft.Practices.Unity.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.Windows\bin\%config%\Microsoft.Practices.Unity.Interception.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.Windows\bin\%config%\Zentity.Services.Windows.exe" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.Windows\bin\%config%\Zentity.Services.Windows.exe.config" %destDir% /r /y /q
ECHO --Copying Notification Service Binaries finished


set destDir="%inputDir%\Build_Target\Server\Pivot.Services\Publishing\"
ECHO --Copying Publishing service binaries
xcopy "%sourceDir%\Product\Zentity.ScholarlyWorks\bin\%config%\Zentity.ScholarlyWorks.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\DeepZoomTools.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\Microsoft.Practices.EnterpriseLibrary.Common.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\Microsoft.Practices.EnterpriseLibrary.Logging.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\Microsoft.Practices.ServiceLocation.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\Microsoft.Practices.Unity.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\Microsoft.Practices.Unity.Interception.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\Zentity.Core.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\Zentity.Pivot.DeepZoomCreator.exe" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\Zentity.Pivot.DeepZoomCreator.exe.config" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\Zentity.Pivot.CollectionCreator.exe" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\Zentity.Pivot.CollectionCreator.exe.config" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\Zentity.Rdf.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\Zentity.Services.External.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\Zentity.Services.ServiceHost.exe" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\Zentity.Services.ServiceHost.exe.config" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\Zentity.Services.Web.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.ServiceHost\bin\%config%\DefaultDeepZoom" %destDir%\DefaultDeepZoom /r /y /q /s /i
ECHO --Copying Publishing service binaries completed


set destDir="%inputDir%\Build_Target\Server\Zentity.Data.Service\DataService\"
ECHO --Copying Data service binaries
xcopy "%sourceDir%\Product\External Libraries\Zentity.Metadata.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\External Libraries\Zentity.ScholarlyWorks.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.Data\bin\Microsoft.Practices.EnterpriseLibrary.Common.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.Data\bin\Microsoft.Practices.EnterpriseLibrary.Logging.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.Data\bin\Microsoft.Practices.ServiceLocation.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.Data\bin\Microsoft.Practices.Unity.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.Data\bin\Microsoft.Practices.Unity.Interception.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.Data\bin\Zentity.Core.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.Data\bin\Zentity.Rdf.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.Data\bin\Zentity.Services.Data.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.Data\ZentityDataService.svc" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services.Data\Web.config" %destDir% /r /y /q
ECHO --Copying Data service binaries completed


set destDir="%inputDir%\Build_Target\Server\Zentity.OData.Translator\ODataToCXmlTranslator\"
ECHO --Copying ODataToCxmlTranslator service binaries 
xcopy "%sourceDir%\Product\ODataToCxmlTranslator\Bin\Microsoft.Practices.EnterpriseLibrary.Common.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\ODataToCxmlTranslator\Bin\Microsoft.Practices.EnterpriseLibrary.Logging.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\ODataToCxmlTranslator\Bin\Microsoft.Practices.ServiceLocation.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\ODataToCxmlTranslator\Bin\Microsoft.Practices.Unity.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\ODataToCxmlTranslator\Bin\Microsoft.Practices.Unity.Interception.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\ODataToCxmlTranslator\Bin\ODataToCxmlTranslator.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\ODataToCxmlTranslator\clientaccesspolicy.xml" %destDir% /r /y /q
xcopy "%sourceDir%\Product\ODataToCxmlTranslator\crossdomain.xml" %destDir% /r /y /q
xcopy "%sourceDir%\Product\ODataToCxmlTranslator\Web.config" %destDir% /r /y /q
ECHO --Copying ODataToCxmlTranslator service binaries finished


set destDir="%inputDir%\Build_Target\Server\Zentity.Web.UI\"
ECHO --Copying WebUI  service binaries 
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Microsoft.Practices.EnterpriseLibrary.Caching.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Microsoft.Practices.EnterpriseLibrary.Common.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Microsoft.Practices.EnterpriseLibrary.Data.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Microsoft.Practices.EnterpriseLibrary.Logging.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Microsoft.Practices.ServiceLocation.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Microsoft.Practices.Unity.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Microsoft.Practices.Unity.Interception.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\System.Data.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\System.Data.Entity.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Zentity.Core.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Zentity.Platform.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Zentity.Rdf.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Zentity.ScholarlyWorks.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Zentity.ScholarlyWorksAndAuthorization.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Zentity.Security.Authentication.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Zentity.Security.AuthenticationProvider.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Zentity.Security.Authorization.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Zentity.Security.AuthorizationHelper.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Zentity.Web.UI.ToolKit.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Zentity.Web.UI.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Services\Zentity.Services\bin\%config%\Zentity.Services.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\bin\Zentity.Zip.dll" %destDir%\bin\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\App_Themes" %destDir%\App_Themes\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\App_GlobalResources" %destDir%\App_GlobalResources\ /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\Web.config" %destDir% /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\Global.asax" %destDir% /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\*.aspx" %destDir% /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\*.ascx" %destDir% /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\*.htm*" %destDir% /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\*.master" %destDir% /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\*.js" %destDir% /r /y /q /s /i
xcopy "%sourceDir%\Product\Zentity.Web.UI\Search" %destDir%\Search /r /y /q /s /i
ECHO --Copying WebUI service binaries finished


set destDir="%inputDir%\Build_Target\Server\Zentity.Services\WebHost\AtomPub\"
ECHO --Copying AtomPub  service binaries 
xcopy "%sourceDir%\Product\Zentity.Services\WebHost\AtomPub" %destDir% /r /y /q /s /i
ECHO --Copying AtomPub service binaries finished


set destDir="%inputDir%\Build_Target\Server\Zentity.Services\WebHost\OaiOre\"
ECHO --Copying OaiOre service binaries 
xcopy "%sourceDir%\Product\Zentity.Services\WebHost\OaiOre" %destDir% /r /y /q /s /i
ECHO --Copying OaiOre service binaries finished


set destDir="%inputDir%\Build_Target\Server\Zentity.Services\WebHost\OaiPmh\"
ECHO --Copying OaiPmh service binaries 
xcopy "%sourceDir%\Product\Zentity.Services\WebHost\OaiPmh" %destDir% /r /y /q /s /i
ECHO --Copying OaiPmh service binaries finished


set destDir="%inputDir%\Build_Target\Server\Zentity.Services\WebHost\Syndication\"
ECHO --Copying Syndication service binaries 
xcopy "%sourceDir%\Product\Zentity.Services\WebHost\Syndication" %destDir% /r /y /q /s /i
ECHO --Copying Syndication service binaries finished

set destDir="%inputDir%\Build_Target\Server\Zentity.Services\"
ECHO --Copying Services binaries 
xcopy "%sourceDir%\Product\Zentity.Services\Zentity.Services\bin\%config%\Zentity.Rdf.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services\Zentity.Services\bin\%config%\Zentity.Core.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services\Zentity.Services\bin\%config%\Zentity.ScholarlyWorks.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.ScholarlyWorksAndAuthorization\Zentity.ScholarlyWorksAndAuthorization\bin\%config%\Zentity.ScholarlyWorksAndAuthorization.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services\Zentity.Services\bin\%config%\Zentity.Platform.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services\Zentity.Services\bin\%config%\Zentity.Services.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services\Zentity.Services\bin\%config%\Zentity.Zip.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services\Zentity.Services\bin\%config%\Zentity.Security.Authentication.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services\Zentity.Services\bin\%config%\Zentity.Security.AuthenticationProvider.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services\Zentity.Services\bin\%config%\Zentity.Security.Authorization.dll" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Services\Zentity.Services\bin\%config%\Zentity.Security.AuthorizationHelper.dll" %destDir% /r /y /q
ECHO --Copying Services binaries finished


set destDir="%inputDir%\Build_Target\Server\Zentity.Visual.Explorer\VisualExplorer\"
ECHO --Copying VisualExplorer binaries 
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\GuanxiMapCore.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\guanximap.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Microsoft.Practices.EnterpriseLibrary.Caching.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Microsoft.Practices.EnterpriseLibrary.Common.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Microsoft.Practices.EnterpriseLibrary.Data.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Microsoft.Practices.EnterpriseLibrary.Logging.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Microsoft.Practices.ServiceLocation.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Microsoft.Practices.Unity.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Microsoft.Practices.Unity.Interception.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Zentity.Core.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Zentity.Platform.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Zentity.ScholarlyWorks.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Zentity.Security.Authentication.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Zentity.Security.AuthenticationProvider.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Zentity.Security.Authorization.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Zentity.Security.AuthorizationHelper.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Zentity.Services.Web.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Zentity.Web.UI.Explorer.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Bin\Zentity.Zip.dll" %destDir%\Bin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\ClientBin\Zentity.Pivot.Web.Viewer.xap" %destDir%\ClientBin\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\ClientBin\Zentity.VisualExplorer.xap" %destDir%\ClientBin\ /r /y /q

REM xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\ClientBin\System.Windows.Controls.Input.zip" %destDir%\ClientBin\ /r /y /q
REM xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\ClientBin\System.Windows.Controls.Layout.Toolkit.zip" %destDir%\ClientBin\ /r /y /q
REM xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\ClientBin\System.Windows.Controls.Toolkit.zip" %destDir%\ClientBin\ /r /y /q
REM xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\ClientBin\System.Windows.Controls.zip" %destDir%\ClientBin\ /r /y /q
REM xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\ClientBin\System.Xml.Linq.zip" %destDir%\ClientBin\ /r /y /q
REM xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\ClientBin\System.Xml.Serialization.zip" %destDir%\ClientBin\ /r /y /q


xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\collections\web.config" %destDir%\collections\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\images\default-logo.png" %destDir%\images\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\images\no-horizon-bg.jpg" %destDir%\images\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Pivot\Gallery.aspx" %destDir%\Pivot\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Pivot\ODataViewer.aspx" %destDir%\Pivot\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Pivot\Viewer.aspx" %destDir%\Pivot\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Search\ExcludedPredicates.config" %destDir%\Search\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Search\ExcludedResourceTypes.config" %destDir%\Search\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Search\ImplicitProperties.config" %destDir%\Search\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Search\PredicateTokens.config" %destDir%\Search\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Search\SpecialTokens.config" %destDir%\Search\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Services\VisualExplorer.svc" %destDir%\Services\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Styles\body.css" %destDir%\Styles\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Styles\chrome.css" %destDir%\Styles\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Styles\Site.css" %destDir%\Styles\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Styles\templates.css" %destDir%\Styles\ /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\clientaccesspolicy.xml" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\crossdomain.xml" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Default.aspx" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Default.Master" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Redirect.aspx" %destDir% /r /y /q
xcopy "%sourceDir%\Product\Zentity.Web.UI.Explorer\Web.config" %destDir% /r /y /q
ECHO --Copying VisualExplorer binaries finished


set destDir="%inputDir%\Build_Target\Client\Documentation\"
ECHO --Copying Client Documentation files 
xcopy "%sourceDir%\Documentation" %destDir% /r /y /q /s /i
ECHO --Copying Client Documentation files finished


set destDir="%inputDir%\Build_Target\Client\Powershell Scripts\"
ECHO --Copying Powershell Scripts 
xcopy "%sourceDir%\Product\Powershell Scripts" %destDir% /r /y /q /s /i
ECHO --Copying Powershell Scripts finished


set destDir="%inputDir%\Build_Target\Client\Samples\"
ECHO --Copying Client Samples 
xcopy "%sourceDir%\Samples" %destDir% /r /y /q /s /i
ECHO --Copying Client Samples finished