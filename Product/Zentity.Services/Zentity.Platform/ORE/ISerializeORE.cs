// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************





namespace Zentity.Platform
{
    #region Using Namespaces
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    #endregion

    #region ISerializeORE interface
    interface ISerializeORE
    {
        string Serialize(string deployedAt);
    }
    #endregion
}
