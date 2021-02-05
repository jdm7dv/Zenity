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
    /// The <see cref="Zentity.Core.AssociationEndType"/> enumeration contains constant 
    /// values that specify the end type of a navigation property with respect to its 
    /// association. There is an implicit direction of <see cref="Zentity.Core.Association"/>
    /// from the Subject to Object <see cref="Zentity.Core.NavigationProperty"/>
    /// </summary>
    public enum AssociationEndType
    {
        /// <summary>
        /// The direction of <see cref="Zentity.Core.NavigationProperty"/> cannot be interpreted.
        /// </summary>
        Undefined,

        /// <summary>
        /// The <see cref="Zentity.Core.NavigationProperty"/> is the Subject of an association.
        /// </summary>
        Subject,

        /// <summary>
        /// The <see cref="Zentity.Core.NavigationProperty"/> is the Object of an association.
        /// </summary>
        Object
    }
}
