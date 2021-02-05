// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
#region Spelling related
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", 
    MessageId = "Zentity", Scope = "namespace", Target = "Zentity.Web.UI.ToolKit",
    Justification="Zentity is the product name.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zentity", Scope = "type", Target = "Zentity.Web.UI.ToolKit.ZentityBase")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zentity", Scope = "type", Target = "Zentity.Web.UI.ToolKit.ZentityDataGridView")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zentity", Scope = "type", Target = "Zentity.Web.UI.ToolKit.ZentityGridEventArgs")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zentity", Scope = "type", Target = "Zentity.Web.UI.ToolKit.ZentityGridView")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zentity", Scope = "type", Target = "Zentity.Web.UI.ToolKit.ZentityTable")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zentity", Scope = "type", Target = "Zentity.Web.UI.ToolKit.ZentityGridViewColumn")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zentity", Scope = "type", Target = "Zentity.Web.UI.ToolKit.ZentityTableDesigner")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zentity")]

#endregion

#region Assembly settings releated
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", 
    MessageId = "System.Data.Entity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 
    Justification="We support .Net framework 3.5 and higher only.")]
#endregion

#region Collection related
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", 
    Target = "Zentity.Web.UI.ToolKit.CategoryNodeAssociation.#SelectedIdList", 
    Justification="This will require major changes to the implementation")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "Zentity.Web.UI.ToolKit.Association.#GetSourceResources(System.String)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ResourceListView.#UserPermissions",
    Justification = "This will require major changes to implementation")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", 
    Target = "Zentity.Web.UI.ToolKit.Association.#DestinationList")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "Zentity.Web.UI.ToolKit.Association.#SourceList")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ResourceListView.#UserPermissions",
    Justification = "The setter property is not removed since it makes the property consistent with other properties")]
#endregion

#region Custom configuration sections related
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface", Scope = "type", 
    Target = "Zentity.Web.UI.ToolKit.ImagePropertyCollection")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface", Scope = "type", Target = "Zentity.Web.UI.ToolKit.ResourcePropertiesConfigElementCollection", 
    Justification="This is the standard way of implementing a custom configuration section")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface", Scope = "type", 
    Target = "Zentity.Web.UI.ToolKit.DateRangePropertiesCollection")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface", Scope = "type", 
    Target = "Zentity.Web.UI.ToolKit.EmailPropertyCollection")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface", Scope = "type", 
    Target = "Zentity.Web.UI.ToolKit.OrderPropertyConfigElementCollection")]
#endregion

#region Url property/parameter naming related
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", 
    Scope = "member", Target = "Zentity.Web.UI.ToolKit.Association.#CreateHyperLink(System.String,System.String)",
    Justification = "We want to support relative Uri, and use of  '~' for application path.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ZentityTable.#CreateHyperlink(System.String,System.String)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ZentityTable.#CreateHyperlinkCell(System.String,System.String)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.Association.#SubjectItemDestinationPageUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.EntitySearch.#EditAssociationUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.EntitySearch.#EditUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.EntitySearch.#ViewUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ZentityDataGridView.#EditAssociationUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ZentityDataGridView.#EditUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ZentityDataGridView.#ViewUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.MostActiveAuthors.#TitleDestinationPageUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.MostUsedTags.#NameDestinationPageUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.MostUsedTags.#UriHeader")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.RecentResources.#ContributorsDestinationPageUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.RecentResources.#TitleDestinationPageUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.RecentResources.#AuthorsDestinationPageUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.TagCloud.#TagClickDestinationPageUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Scope = "member", Target = "Zentity.Web.UI.ToolKit.Association.#CreateHyperlink(System.String,System.String)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.CategoryNodeAssociation.#SubjectNavigationUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.CategoryNodeAssociation.#TreeNodeNavigationUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ChangeHistory.#NavigateUrlForCategory")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ChangeHistory.#NavigateUrlForResource")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ChangeHistory.#NavigateUrlForTag")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.RelatedResourcesCloud.#ViewUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ReportingView.#ViewUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ResourceDetailView.#EditCategoryAssociationUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ResourceDetailView.#EditTagAssociationUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ResourceDetailView.#BibTexImportUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ResourceDetailView.#ViewChangeHistoryUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ResourceDetailView.#ViewUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ResourceListView.#ViewUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ZentityDataGridView.#PermissionUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Zentity.Web.UI.ToolKit.CategoryHierarchy.#TreeNodeNavigationUrl")]
#endregion

#region Getter methods related
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member", 
    Target = "Zentity.Web.UI.ToolKit.Association.#GetDestinationItems()",
    Justification = "This method fetches data from the database hence should be method only and not property.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member", 
    Target = "Zentity.Web.UI.ToolKit.Association.#GetSourceCategories()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member", Target = "Zentity.Web.UI.ToolKit.Association.#GetSourceItems()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member", Target = "Zentity.Web.UI.ToolKit.Association.#GetSourceTags()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ZentityGridView.#GetRecords()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ResourceProperties.#GetResourceDetails()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member", Target = "Zentity.Web.UI.ToolKit.CategoryNodeAssociation.#GetScholarlyWorkItem()")]
#endregion

#region Unused code (but used) related
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Scope = "type", 
    Target = "Zentity.Web.UI.ToolKit.TagCloudEntry",
    Justification = "We instantiate this class using LINQ")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", 
    Target = "Zentity.Web.UI.ToolKit.ResourceProperties.#IsOfType`1(System.Object)",
    Justification = "We instantiate this class using reflection")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ResourceDataAccess.#GetResource`1(System.Guid)")]
#endregion

#region Others

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope = "type", Target = "Zentity.Web.UI.ToolKit.ResourceDataAccess")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope = "member", Target = "Zentity.Web.UI.ToolKit.ResourceDataAccess.#.cctor()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ToolKit", 
    Scope = "namespace", Target = "Zentity.Web.UI.ToolKit", 
    Justification="This will be a major change.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ToolKit")]

#endregion