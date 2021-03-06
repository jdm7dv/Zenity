﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30128.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Zentity.Security.AuthenticationProvider {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ConstantStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ConstantStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Zentity.Security.AuthenticationProvider.ConstantStrings", typeof(ConstantStrings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User credentials are not verified or user is not administrator..
        /// </summary>
        internal static string AdminAuthenticationExceptionMessage {
            get {
                return ResourceManager.GetString("AdminAuthenticationExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred on authentication database. Please see inner exception for details..
        /// </summary>
        internal static string DatabaseExceptionMessage {
            get {
                return ResourceManager.GetString("DatabaseExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to All DigestSecurityToken properties.
        /// </summary>
        internal static string DigestTokenProperties {
            get {
                return ResourceManager.GetString("DigestTokenProperties", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Disabled.
        /// </summary>
        internal static string Disabled {
            get {
                return ResourceManager.GetString("Disabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Email format not valid..
        /// </summary>
        internal static string EmailFormatExceptionMessage {
            get {
                return ResourceManager.GetString("EmailFormatExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error has occurred. Please refer to the inner exception for details..
        /// </summary>
        internal static string ExceptionMessage {
            get {
                return ResourceManager.GetString("ExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Index values not in proper range. .
        /// </summary>
        internal static string IndexValuesExceptionMessage {
            get {
                return ResourceManager.GetString("IndexValuesExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid type of token is passed. &apos;DigestSecurityToken&apos; is expected..
        /// </summary>
        internal static string InvalidDigestToken {
            get {
                return ResourceManager.GetString("InvalidDigestToken", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The AuthenticatedToken passed is not valid. Please log in again to get a valid token..
        /// </summary>
        internal static string InvalidTokenMessage {
            get {
                return ResourceManager.GetString("InvalidTokenMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid credentialToken type. &apos;UserNameSecurityToken&apos; type is expected..
        /// </summary>
        internal static string InvalidTokenTypeMessage {
            get {
                return ResourceManager.GetString("InvalidTokenTypeMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This login name is not in valid format.
        /// </summary>
        internal static string LoginFormatExceptionMessage {
            get {
                return ResourceManager.GetString("LoginFormatExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Property cannot be null : {0}..
        /// </summary>
        internal static string MandatoryPropertyNotSetExceptionMessage {
            get {
                return ResourceManager.GetString("MandatoryPropertyNotSetExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Key cannot be null or empty..
        /// </summary>
        internal static string ParameterKeyNullExceptionMessage {
            get {
                return ResourceManager.GetString("ParameterKeyNullExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your password has expired. Please change password and then retry login..
        /// </summary>
        internal static string PasswordExpiredExceptionMessage {
            get {
                return ResourceManager.GetString("PasswordExpiredExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Password expiry cannot be less than 7 days or more than 100 days..
        /// </summary>
        internal static string PasswordExpiryErrorMessage {
            get {
                return ResourceManager.GetString("PasswordExpiryErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Maximum length of password cannot exceed 100..
        /// </summary>
        internal static string PasswordMaxLengthErrorMessage {
            get {
                return ResourceManager.GetString("PasswordMaxLengthErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Minimum length of password cannot be less than 6..
        /// </summary>
        internal static string PasswordMinLengthErrorMessage {
            get {
                return ResourceManager.GetString("PasswordMinLengthErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Exception while reading password policy. Cannot check policy conformance of new passwords..
        /// </summary>
        internal static string PasswordPolicyReadExceptionMessage {
            get {
                return ResourceManager.GetString("PasswordPolicyReadExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The password does not conform to password policy..
        /// </summary>
        internal static string PolicyConformanceExceptionMessage {
            get {
                return ResourceManager.GetString("PolicyConformanceExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The SqlDataReader object is closed. This method requires a reader in open state..
        /// </summary>
        internal static string ReaderClosedExceptionMessage {
            get {
                return ResourceManager.GetString("ReaderClosedExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Application is not configured correctly. Please check inner exception for details..
        /// </summary>
        internal static string TypeInitializationExceptionMessage {
            get {
                return ResourceManager.GetString("TypeInitializationExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ValidFrom must be less than ValidUpTo..
        /// </summary>
        internal static string ValidFromValidUpToExceptionMessage {
            get {
                return ResourceManager.GetString("ValidFromValidUpToExceptionMessage", resourceCulture);
            }
        }
    }
}
