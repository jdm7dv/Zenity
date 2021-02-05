// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Services.Web.Pivot
{
    using System.ServiceModel;
    using Configuration.Pivot;

    /// <summary>
    /// Interface for Configuration Service
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.NotAllowed)]
    public interface IConfigurationService
    {
        /// <summary>
        /// Gets the Model Configuration if specified or gets the default one.
        /// </summary>
        /// <param name="modelNamespace">Model Namespace</param>
        /// <returns>Return the Module Setting</returns>
        [OperationContract]
        ModuleSetting GetModelConfiguration(string modelNamespace);

        /// <summary>
        /// Gets the Resource Type Configuration if specified or gets the default one.
        /// </summary>
        /// <param name="modelNamespace">Model Namespace</param>
        /// <param name="resourceTypeName">Resource Type Name</param>
        /// <returns>Returns the Resource Type Setting</returns>
        [OperationContract]
        ResourceTypeSetting GetResourceTypeConfiguration(string modelNamespace, string resourceTypeName);

        /// <summary>
        /// Sets the Module Configurtion
        /// </summary>
        /// <param name="modelNamespace">Model Namespace</param>
        /// <param name="modelConfiguration">Module Setting Configuraion</param>
        [OperationContract]
        [FaultContract(typeof(System.ArgumentNullException))]
        [FaultContract(typeof(System.ArgumentException))]
        void SetModelConfiguration(string modelNamespace, ModuleSetting modelConfiguration);

        /// <summary>
        /// Sets the Resource Type Configuration
        /// </summary>
        /// <param name="modelNamespace">Model Namespace</param>
        /// <param name="resourceTypeName">Resource Type Name</param>
        /// <param name="resourceTypeConfig">Resource Type Setting Configuration</param>
        [OperationContract]
        [FaultContract(typeof(System.ArgumentException))]
        void SetResourceTypeConfiguration(string modelNamespace, string resourceTypeName, ResourceTypeSetting resourceTypeConfig);
    }
}
