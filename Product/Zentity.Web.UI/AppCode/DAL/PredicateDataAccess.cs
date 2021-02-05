// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Zentity.Core;
using Zentity.ScholarlyWorks;
using Zentity.Web.UI;
using System.Data.Objects;
using System.Globalization;

namespace Zentity.Web.UI
{
    /// <summary>
    /// Summary description for PredicateDAL
    /// </summary>
    public sealed class PredicateDataAccess : IDisposable
    {
        #region Constants

        #region Private

        #region E-Sql Queries

        static Func<ZentityContext, string, IQueryable<Predicate>> _getPredicateByUriCompiledQuery =
               CompiledQuery.Compile((ZentityContext context, string predicateUri) =>
                   context.Predicates.Where(p => p.Uri.Contains(predicateUri)));

        static Func<ZentityContext, Guid, IQueryable<Predicate>> _getPredicateByIdCompiledQuery =
            CompiledQuery.Compile((ZentityContext context, Guid predicateId) =>
            context.Predicates.Where(p => p.Id == predicateId));

        #endregion

        #endregion

        #endregion

        #region Member Variables

        #region Private

        ZentityContext _context;
        private bool isDisposed;

        #endregion

        #endregion

        #region Constructors & finalizers

        #region public

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PredicateDataAccess()
        {
            _context = Utility.CreateContext();
        }

        #endregion

        #endregion

        #region Properties
        #region Public

        public List<Predicate> Predicates
        {
            get
            {
                return _context.Predicates.ToList();
            }
        }

        #endregion
        #endregion

        #region Methods

        #region public

        /// <summary>
        /// Fetched predicate object using predicate string.
        /// </summary>
        /// <param name="predicate">Predicate string</param>
        /// <returns>Predicate object</returns>
        public Predicate GetPredicate(string predicate)
        {
            return _getPredicateByUriCompiledQuery.Invoke(_context, predicate).FirstOrDefault();
        }

        /// <summary>
        /// Fetched predicate object using predicate string.
        /// </summary>
        ///<param name="Id">id of predicate to retrieve</param>
        /// <param name="context">Instance of <see cref="ZentityContext"/></param>
        /// <returns>Predicate object</returns>
        public static Predicate GetPredicate(Guid Id, ZentityContext context)
        {
            return _getPredicateByIdCompiledQuery.Invoke(context, Id).FirstOrDefault();
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!isDisposed && this._context != null)
            {
                this._context.Dispose();
                isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        #endregion

        #endregion

        #endregion
    }
}