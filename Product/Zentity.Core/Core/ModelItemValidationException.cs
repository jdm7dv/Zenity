// *********************************************************
// 
//     Copyright (c) Microsoft. All rights reserved.
//     This code is licensed under the Apache License, Version 2.0.
//     THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//     ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//     IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//     PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// 
// *********************************************************

using System;

namespace Zentity.Core
{
    /// <summary>
    /// Custom exception that is thrown if the data model fails validation tests.
    /// </summary>
    [Serializable]
    internal sealed class ModelItemValidationException : ZentityException
    {
        internal ModelItemValidationException()
            : base()
        {
        }

        internal ModelItemValidationException(string message)
            : base(message)
        {
        }
        
        internal ModelItemValidationException(string message, Exception inner) :
            base(message, inner)
        {
        }

    }
}
