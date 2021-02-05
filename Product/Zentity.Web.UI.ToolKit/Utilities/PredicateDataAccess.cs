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
using Zentity.Web.UI.ToolKit;
using System.Data.Objects;
using System.Globalization;

namespace Zentity.Web.UI.ToolKit
{
    /// <summary>
    /// Summary description for PredicateDAL
    /// </summary>
    internal sealed class PredicateDataAccess
    {
        #region Constants

        #region Private

        #region Compiled Linq Queries
                                  

//        const string _predicatesQuery = @"select value p from
//                                ZentityContext.Predicates as p";


        static Func<ZentityContext, string, IQueryable<Predicate>> _getPredicateByUriCompiledQuery =
               CompiledQuery.Compile((ZentityContext context, string predicateUri) =>
                   context.Predicates.Where(p => p.Uri.Contains(predicateUri)));

        static Func<ZentityContext, Guid, IQueryable<Predicate>> _getPredicateByIdCompiledQuery =
               CompiledQuery.Compile((ZentityContext context, Guid predicateId) =>
                   context.Predicates.Where(p => p.Id==predicateId));

        #endregion

        #endregion

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Fetched predicate object using predicate string.
        /// </summary>
        /// <param name="predicate">Predicate string</param>
        /// <param name="context">Instance of <see cref="ZentityContext"/></param>
        /// <returns>Predicate object</returns>
        public static Predicate GetPredicate(string predicate, ZentityContext context)
        {                
            return _getPredicateByUriCompiledQuery.Invoke(context, predicate).FirstOrDefault();
        }

        /// <summary>
        /// Fetched predicate object using predicate string.
        /// </summary>
        ///<param name="Id">id of predicate to retrieve</param>
        /// <param name="context">Instance of <see cref="ZentityContext"/></param>
        /// <returns>Predicate object</returns>
        public static Predicate GetPredicate(Guid Id, ZentityContext context)
        {           
            return _getPredicateByIdCompiledQuery.Invoke(context,Id).FirstOrDefault();
        }

        
        ///// <summary>
        ///// Returns all the predicates.
        ///// </summary>
        ///// <param name="formatUri">Boolean value indicating whether to remove out predicate URI or not.
        ///// E.g. If formatUri = false, then return 'urn:predicates:authored-by',
        ///// else return only 'authored-by'.
        ///// </param>
        ///// <param name="context">Instance of <see cref="ZentityContext"/></param>
        ///// <returns></returns>
        //public static List<Predicate> GetPredicates(bool formatUri, ZentityContext context)
        //{

        //    ObjectQuery<Predicate> myPredicates = new ObjectQuery<Predicate>
        //                        (string.Format(CultureInfo.InvariantCulture, _predicatesQuery), context);

        //    List<Predicate> predicates = myPredicates.ToList();           
            
        //    if (formatUri)
        //    {
        //        foreach (Predicate predicate in predicates)
        //        {
        //            int startindex = predicate.Uri.LastIndexOf(Constants.Colon);
        //            if (startindex != -1)
        //            {
        //                predicate.Uri = predicate.Uri.Substring(startindex + 1);
        //            }
        //        }
        //    }

        //    return predicates;
        //}

        #endregion

        #endregion
    }
}