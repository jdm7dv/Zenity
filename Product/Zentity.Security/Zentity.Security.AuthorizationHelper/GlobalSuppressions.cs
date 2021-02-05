// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zentity",
    Justification = "Zentity is the product name")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework",
    MessageId = "System.Data.Entity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
    Justification = "The product supports .Net framework 3.5 and above only")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
    Scope = "member", Target = "Zentity.Security.AuthorizationHelper.AuthorizationManager.#GetPermissions`1(System.Collections.Generic.IEnumerable`1<!!0>,Zentity.Core.ZentityContext,Zentity.Security.Authentication.AuthenticatedToken)",
    Justification = "This is required for allowing applications to get resources of specific type and inherited types only.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "Zentity.Security.AuthorizationHelper.AuthorizationManager.#GetPermissions`1(System.Data.Objects.ObjectQuery`1<!!0>,Zentity.Core.ZentityContext,Zentity.Security.Authentication.AuthenticatedToken)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Scope = "member", Target = "Zentity.Security.AuthorizationHelper.AuthorizationManager.#GetPermissions`1(System.Linq.IQueryable`1<!!0>,Zentity.Core.ZentityContext,Zentity.Security.Authentication.AuthenticatedToken)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
    Target = "Zentity.Security.AuthorizationHelper.PermissionManager.#GetInversePermissionsHierarchy()",
    Justification = @"Properties provide access to data members, and having static data members exposed publicly may lead to concurrent access exceptions. Hence it is left to the applications
    to store the results of the method")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member", Target = "Zentity.Security.AuthorizationHelper.PermissionManager.#GetPermissionsHierarchy()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member", Target = "Zentity.Security.AuthorizationHelper.PermissionManager.#GetRepositoryLevelPermissions()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member", Target = "Zentity.Security.AuthorizationHelper.PermissionManager.#GetSecurityPredicates()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "Zentity", Scope = "resource", Target = "Zentity.Security.AuthorizationHelper.ConstantStrings.resources")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tsql", Scope = "type", Target = "Zentity.Security.AuthorizationHelper.TsqlAuthorization",
    Justification = "Tsql is a standard short form for Transact SQL.")]
