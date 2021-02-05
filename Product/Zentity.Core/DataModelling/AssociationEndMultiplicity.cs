// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************


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
