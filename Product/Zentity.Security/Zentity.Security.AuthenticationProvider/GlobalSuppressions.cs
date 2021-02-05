// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zentity",
    Scope = "namespace", Target = "Zentity.Security.SqlAuthenticationProvider", Justification = "Zentity is the product name")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = "Zentity", Justification = "Zentity is the product name")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = "Zentity", Scope = "namespace", Target = "Zentity.Security.SqlAuthentication", Justification = "Zentity is the product name")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = "Zentity", Scope = "type", Target = "Zentity.Security.SqlAuthentication.ZentityUser", Justification = "Zentity is the product name")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = "Zentity", Scope = "namespace", Target = "Zentity.Security.AuthenticationProvider.PasswordManagement",
    Justification = "Zentity is the product name")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = "Zentity", Scope = "type", Target = "Zentity.Security.AuthenticationProvider.ZentityAuthenticatedToken",
    Justification = "Zentity is the product name")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = "Zentity", Scope = "type", Target = "Zentity.Security.AuthenticationProvider.ZentityAuthenticationProvider",
    Justification = "Zentity is the product name")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = "Zentity", Scope = "type", Target = "Zentity.Security.AuthenticationProvider.ZentityUser",
    Justification = "Zentity is the product name")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = "Zentity", Scope = "type", Target = "Zentity.Security.AuthenticationProvider.ZentityUserAdmin",
    Justification = "Zentity is the product name")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = "Zentity", Scope = "type", Target = "Zentity.Security.AuthenticationProvider.ZentityUserManager",
    Justification = "Zentity is the product name")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace",
    Target = "Zentity.Security.AuthenticationProvider.DigestAuthentication", Justification = "The classes in this namespace are logically separate from other classes")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = "Zentity", Scope = "namespace", Target = "Zentity.Security.AuthenticationProvider.DigestAuthentication",
    Justification = "Zentity is the product name")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes",
    Scope = "namespace", Target = "Zentity.Security.AuthenticationProvider.PasswordManagement", Justification = "The classes in this namespace are logically separate from other classes")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
    Target = "Zentity.Security.AuthenticationProvider.ZentityUser.#GetRemainingDaysToPasswordExpiry()",
    Justification = "This method accesses database and hence cannot be a property.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
    Scope = "member", Target = "Zentity.Security.AuthenticationProvider.ZentityUser.#GetUserId()",
    Justification = "This method accesses database and hence cannot be a property.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline",
    Scope = "member", Target = "Zentity.Security.AuthenticationProvider.PasswordManagement.PasswordPolicyProvider.#.cctor()",
    Justification = "This cannot be avoided since the static constructor needs to read config section, which cannot be done in initialization fields.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
    Scope = "member", Target = "Zentity.Security.AuthenticationProvider.ZentityUserAdmin.#Delete(System.String)",
    Justification = @"This is by design. The constructor takes admin credentials as input and allows instance creation only if the user is identified 
    to be an administrator. All methods of this class are to be made accessible to an administrator only. Hence the methods cannot be made static.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
    Target = "Zentity.Security.AuthenticationProvider.ZentityUserAdmin.#GetAccountStatusValues()",
    Justification = @"This is by design. The constructor takes admin credentials as input and allows instance creation only if the user is identified 
    to be an administrator. All methods of this class are to be made accessible to an administrator only. Hence the methods cannot be made static.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
    Target = "Zentity.Security.AuthenticationProvider.ZentityUserAdmin.#GetAccountStatusValues()",
    Justification = @"This is by design. The constructor takes admin credentials as input and allows instance creation only if the user is identified 
    to be an administrator. All methods of this class are to be made accessible to an administrator only. Hence the methods cannot be made static.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
    Target = "Zentity.Security.AuthenticationProvider.ZentityUserAdmin.#GetUserProfile(System.String)",
    Justification = @"This is by design. The constructor takes admin credentials as input and allows instance creation only if the user is identified 
    to be an administrator. All methods of this class are to be made accessible to an administrator only. Hence the methods cannot be made static.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
    Target = "Zentity.Security.AuthenticationProvider.ZentityUserAdmin.#GetUsers(System.Int32,System.Int32)",
    Justification = @"This is by design. The constructor takes admin credentials as input and allows instance creation only if the user is identified 
    to be an administrator. All methods of this class are to be made accessible to an administrator only. Hence the methods cannot be made static.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
    Target = "Zentity.Security.AuthenticationProvider.ZentityUserAdmin.#ResetPassword(System.String)",
    Justification = @"This is by design. The constructor takes admin credentials as input and allows instance creation only if the user is identified 
    to be an administrator. All methods of this class are to be made accessible to an administrator only. Hence the methods cannot be made static.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
    Target = "Zentity.Security.AuthenticationProvider.ZentityUserAdmin.#SetAccountStatus(System.String,System.String)",
    Justification = @"This is by design. The constructor takes admin credentials as input and allows instance creation only if the user is identified 
    to be an administrator. All methods of this class are to be made accessible to an administrator only. Hence the methods cannot be made static.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
    Target = "Zentity.Security.AuthenticationProvider.ZentityUserAdmin.#SetAdmin(System.String)",
    Justification = @"This is by design. The constructor takes admin credentials as input and allows instance creation only if the user is identified 
    to be an administrator. All methods of this class are to be made accessible to an administrator only. Hence the methods cannot be made static.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
    Target = "Zentity.Security.AuthenticationProvider.ZentityUserAdmin.#UnsetAdmin(System.String)",
    Justification = @"This is by design. The constructor takes admin credentials as input and allows instance creation only if the user is identified 
    to be an administrator. All methods of this class are to be made accessible to an administrator only. Hence the methods cannot be made static.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zentity", Scope = "type", Target = "Zentity.Security.AuthenticationProvider.ZentityUserProfile")]
