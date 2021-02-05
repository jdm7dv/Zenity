// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
namespace Zentity.Services.Windows
{
    /// <summary>
    /// LINQ to SQL's DataContext class for accessing Zentity tables under Administration schema.
    /// </summary>
    public partial class ZentityAdministrationDataContext
    {
        /// <summary>
        /// Called when an instance of the DataContext is created.
        /// </summary>
        partial void OnCreated()
        {
            this.CommandTimeout = this.Connection.ConnectionTimeout;
        }
    }
}
