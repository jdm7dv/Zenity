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

namespace Zentity.Core
{
    /// <summary>
    /// The <see cref="Zentity.Core.AssociationEndMultiplicity"/> enumeration contains constant 
    /// values that specify the multiplicity of an end of an association. 
    /// </summary>
    public enum AssociationEndMultiplicity
    {
        /// <summary>
        /// Indicates that zero or more entities can participate at this end of the association.
        /// </summary>
        Many,

        /// <summary>
        /// Indicates that exactly one entity can participate at this end of the association.
        /// </summary>
        One,

        /// <summary>
        /// Indicates that zero or one entity can participate at this end of the association.
        /// </summary>
        ZeroOrOne
    }
}
