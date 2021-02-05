// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zentity.Services.Windows.Pivot;

namespace Zentity.Services.Windows
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts a ChangeMessageRecovery into a ResourceChangeMessage.
        /// </summary>
        /// <param name="changeMessage">The change message.</param>
        /// <returns>Resource change message.</returns>
        internal static ResourceChangeMessage ToResourceChangeMessage(this ChangeMessageRecovery changeMessage)
        {
            if (changeMessage == null)
            {
                return null;
            }

            ResourceChangeMessage resChangeMessage = new ResourceChangeMessage
            {
                ResourceId = changeMessage.ResourceId,
                ResourceTypeId = changeMessage.ResourceTypeId,
                ResourceTypeName = changeMessage.ResourceTypeName,
                DataModelNamespace = changeMessage.DataModelNamespace,
                ChangeType = (ResourceChangeType) Enum.Parse(typeof (ResourceChangeType), changeMessage.ChangeType.ToString()),
                DateAdded = changeMessage.DateAdded,
                DateModified = changeMessage.DateModified
            };

            return resChangeMessage;
        }

        /// <summary>
        /// Converts a ResourceChangeMessage into a ChangeMessageRecovery
        /// </summary>
        /// <param name="resChangeMessage">The res change message.</param>
        /// <returns>Change message recovery.</returns>
        internal static ChangeMessageRecovery ToChangeMessage(this ResourceChangeMessage resChangeMessage)
        {
            if (resChangeMessage == null)
            {
                return null;
            }

            ChangeMessageRecovery changeMessage = new ChangeMessageRecovery
            {
                Id = Guid.NewGuid(),
                ResourceId = resChangeMessage.ResourceId,
                ResourceTypeId = resChangeMessage.ResourceTypeId,
                ResourceTypeName = resChangeMessage.ResourceTypeName,
                DataModelNamespace = resChangeMessage.DataModelNamespace,
                DateAdded = resChangeMessage.DateAdded,
                DateModified = resChangeMessage.DateModified,
                ChangeType = (short) resChangeMessage.ChangeType
            };

            return changeMessage;
        }

        /// <summary>
        /// Converts a list of ChangeMessageRecovery into a ResourceChangeMessage list
        /// </summary>
        /// <param name="changeMessageList">The change message list.</param>
        /// <returns>List of <see cref="ResourceChangeMessage"/>.</returns>
        internal static IEnumerable<ResourceChangeMessage> ToResourceChangeMessageList(this IEnumerable<ChangeMessageRecovery> changeMessageList)
        {
            List<ResourceChangeMessage> resChangeMessageList = new List<ResourceChangeMessage>();
            if (changeMessageList == null)
            {
                return resChangeMessageList;
            }

            foreach (var changeMessage in changeMessageList)
            {
                var resChangeMessage = changeMessage.ToResourceChangeMessage();
                if (resChangeMessage != null)
                {
                    resChangeMessageList.Add(resChangeMessage);
                }
            }

            return resChangeMessageList;
        }

        /// <summary>
        /// Converts a list of ResourceChangeMessage into a ChangeMessageRecovery list
        /// </summary>
        /// <param name="resChangeMessageList">The res change message list.</param>
        /// <returns>List of <see cref="ChangeMessageRecovery"/>.</returns>
        internal static IEnumerable<ChangeMessageRecovery> ToChangeMessageList(this IEnumerable<ResourceChangeMessage> resChangeMessageList)
        {
            List<ChangeMessageRecovery> changeMessageList = new List<ChangeMessageRecovery>();
            if (resChangeMessageList == null)
            {
                return changeMessageList;
            }

            foreach (var resChangeMessage in resChangeMessageList)
            {
                var changeMessage = resChangeMessage.ToChangeMessage();
                if (changeMessage != null)
                {
                    changeMessageList.Add(changeMessage);
                }
            }

            return changeMessageList;
        }
    }
}
