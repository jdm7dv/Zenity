// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Runtime.Serialization;
using Zentity.Web.UI.ToolKit.Resources;
using System.Security.Permissions;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// Exception thrown when an entity with the specified entity type and entity id is not found.
    /// </summary>
    [Serializable]
    public class EntityNotFoundException : Exception
    {
        #region Constants
        private const string _entityIdKey = "EntityId";
        private const string _entityTypeKey = "EntityType";
        #endregion Constants

        #region Member Variables
        private Guid _entityId;
        private EntityType _entityType;
        #endregion Member Variables

        #region Properties

        /// <summary>
        /// Gets the entity id that caused the exception to be thrown.
        /// </summary>
        public Guid EntityId
        {
            get
            {
                return this._entityId;
            }
        }

        /// <summary>
        /// Gets the entity type that caused the exception to be thrown.
        /// </summary>
        public EntityType EntityType
        {
            get
            {
                return this._entityType;
            }
        }

        #endregion Properties

        #region Construction and Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException" /> class
        /// </summary>
        public EntityNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException" /> class with 
        /// a message.
        /// </summary>
        public EntityNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException" /> class with 
        /// a message and inner exception.
        /// </summary>
        public EntityNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException" /> class with
        /// a specified entity type and entity id.
        /// </summary>
        /// <param name="entityType">Entity type that was not found.</param>
        /// <param name="entityId">Entity id that was not found.</param>
        public EntityNotFoundException(EntityType entityType, Guid entityId)
            : this(entityType, entityId, GlobalResource.EntityNotFoundExceptionMessage)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException" /> class with a 
        /// specified entity type, entity id and an error message.
        /// </summary>
        /// <param name="entityType">Entity type that was not found.</param>
        /// <param name="entityId">Entity id that was not found.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public EntityNotFoundException(EntityType entityType, Guid entityId, string message)
            : base(message)
        {
            this._entityType = entityType;
            this._entityId = entityId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException" /> class with 
        /// a specified entity type, entity id, error message and a reference to the inner exception that is 
        /// the cause of this exception.
        /// </summary>
        /// <param name="entityType">Entity type that was not found.</param>
        /// <param name="entityId">Entity id that was not found.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null 
        /// reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public EntityNotFoundException(EntityType entityType, Guid entityId, string message, Exception inner)
            : base(message, inner)
        {
            this._entityType = entityType;
            this._entityId = entityId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException" /> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual
        /// information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The info parameter is null.</exception>
        /// <exception cref="SerializationException">The class name is null or 
        /// HResult is zero (0).</exception>        
        protected EntityNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info != null)
            {
                this._entityType = (EntityType)info.GetValue(_entityTypeKey, typeof(EntityType));
                this._entityId = (Guid)info.GetValue(_entityIdKey, typeof(Guid));
            }
        }

        #endregion Construction and Initialization

        #region Methods

        /// <summary>
        /// Sets the <see cref="SerializationInfo" /> with information about the exception. 
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized 
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual
        /// information about the source or destination.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            if (info != null)
            {
                info.AddValue(_entityTypeKey, this._entityType);
                info.AddValue(_entityIdKey, this._entityId);
            }
        }

        #endregion Methods
    }
}