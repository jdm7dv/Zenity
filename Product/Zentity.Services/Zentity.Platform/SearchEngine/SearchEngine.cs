// *******************************************************
//                                                        
//    Copyright (C) Microsoft. All rights reserved.       
//                                                        
// *******************************************************

namespace Zentity.Platform
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System;
    using System.Globalization;
    using Zentity.Core;
    using Zentity.Platform.Properties;
    using Zentity.Security.Authentication;

    /// <summary>
    /// Represents a Search Engine having APIs that help search the repository.
    /// </summary>
    public class SearchEngine
    {
        #region Private Fields

        private int maximumResultCount;

        #endregion 

        #region Properties

        /// <summary>
        /// Gets or sets the maximum number of matching results to fetch.
        /// </summary>
        public int MaximumResultCount
        {
            get { return this.maximumResultCount; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(
                        String.Format(
                        CultureInfo.CurrentCulture,
                        Resources.SEARCH_VALUE_LESS_THAN_ZERO, "MaximumResultCount"));
                }
                this.maximumResultCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the authenticated token representing the user.
        /// </summary>
        public AuthenticatedToken AuthenticatedToken
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the security is to be used.
        /// </summary>
        public bool IsSecurityAware
        {
            get;
            set;
        }

        #endregion 

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchEngine"/> class.
        /// </summary>
        public SearchEngine()
            : this(SearchConstants.DEFAULT_MAX_RESULT_COUNT, false, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchEngine"/> class.
        /// </summary>
        /// <param name="maximumResultCount">Maximum number of results to return
        /// in paged search.</param>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid.
        /// </exception>
        public SearchEngine(int maximumResultCount)
            : this(false, null)
        {
            if (maximumResultCount < 0)
            {
                throw new ArgumentException(
                    String.Format(
                    CultureInfo.CurrentCulture,
                    Resources.SEARCH_VALUE_LESS_THAN_ZERO, "maximumResultCount"));
            }
            MaximumResultCount = maximumResultCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchEngine"/> class.
        /// </summary>
        /// <param name="maximumResultCount">Maximum number of results to return
        /// in paged search.</param>
        /// <param name="isSecurityAware">If true, then only security would be 
        /// used.</param>
        /// <param name="authenticatedToken">Authenticated token representing 
        /// the user.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid.
        /// </exception>
        public SearchEngine(int maximumResultCount, bool isSecurityAware, 
            AuthenticatedToken authenticatedToken)
        {
            if (maximumResultCount < 0)
            {
                throw new ArgumentException(
                    String.Format(
                    CultureInfo.CurrentCulture,
                    Resources.SEARCH_VALUE_LESS_THAN_ZERO, "maximumResultCount"));
            }
            if (isSecurityAware && authenticatedToken == null)
            {
                throw new ArgumentNullException("authenticatedToken",
                    string.Format(CultureInfo.CurrentCulture, 
                    Resources.SEARCH_AUTHENTICATED_TOKEN_NULL,
                    "authenticatedToken", "isSecurityAware"));
            }
            MaximumResultCount = maximumResultCount;
            AuthenticatedToken = authenticatedToken;
            IsSecurityAware = isSecurityAware;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchEngine"/> class.
        /// </summary>
        /// <param name="isSecurityAware">If true, then only security would be 
        /// used.</param>
        /// <param name="authenticatedToken">Authenticated token representing 
        /// the user.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null.
        /// </exception>
        public SearchEngine(bool isSecurityAware, AuthenticatedToken authenticatedToken)
        {
            if (isSecurityAware && authenticatedToken == null)
            {
                throw new ArgumentNullException("authenticatedToken",
                    string.Format(CultureInfo.CurrentCulture, 
                    Resources.SEARCH_AUTHENTICATED_TOKEN_NULL,
                    "authenticatedToken", "isSecurityAware"));
            }
            AuthenticatedToken = authenticatedToken;
            IsSecurityAware = isSecurityAware;
            MaximumResultCount = SearchConstants.DEFAULT_MAX_RESULT_COUNT;
        }

        #endregion 

        #region Public Methods

        /// <summary>
        /// Searches for similar resources given a search criteria. 
        /// Results will be sorted according to the matching percentage 
        /// and paged according to input criteria.
        /// </summary>
        /// <param name="resourceType">Type of the similar resources to be 
        /// searched.</param>
        /// <param name="searchCriteria">Search criteria.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch
        /// data with.</param>
        /// <param name="currentCursor">The number of matching records to 
        /// skip.</param>
        /// <param name="totalRecords">The total number of records matching 
        /// the search criteria.</param>
        /// <returns>Matching resources and their percentage match.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid.
        /// </exception>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        /// Thrown when file specified in the application settings is not found 
        /// OR the file extension is not valid 
        /// OR the config file does not conform to its Xsd.</exception>
        /// <exception cref="SearchException">
        /// <paramref name="searchCriteria"/> is invalid.
        /// </exception>
        /// <exception cref="System.Data.SqlClient.SqlException">
        /// If Sql server throws an exception.</exception>
        /// <example>
        /// Search for similar records given a search criteria. 
        /// This example searches for similar resources in Zentity repository 
        /// matching the given <paramref name="searchCriteria"/> and 
        /// <paramref name="resourceType"/>.
        /// <code>
        /// using System;
        /// using System.Data.SqlClient;
        /// using System.Configuration;
        /// using System.Collections.Generic;
        /// using Zentity.Core;
        /// using Zentity.Platform;
        /// using Zentity.ScholarlyWorks;
        /// namespace Zentity.Platform.Samples
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///          {
        ///              try
        ///              {
        ///                  using(ZentityContext context = new ZentityContext())
        ///                  {
        ///                      // Load ScholarlyWork metadata.
        ///                      context.MetadataWorkspace.LoadFromAssembly(typeof(ScholarlyWork).Assembly);
        /// 
        ///                      string resourceType = "book";
        /// 
        ///                      List&lt;PropertyValuePair&gt; searchCriteria = new List&lt;PropertyValuePair&gt;();
        ///                      searchCriteria.Add(new PropertyValuePair("Title", "sql"));
        ///                      searchCriteria.Add(new PropertyValuePair("Description", "2008"));
        /// 
        ///                      // Specify the page size.
        ///                      int pageSize = 10;
        ///                      SearchEngine search = new SearchEngine(pageSize);
        ///  
        ///                      int currentCursor = 0;
        ///                      
        ///                      int totalMatchingRecords;
        ///  
        ///                      // Retrieve 1st 10 matching records.
        ///                      IEnumerable&lt;SimilarRecord> matchingItems = 
        ///                          search.SearchSimilarResources(
        ///                             resourceType, searchCriteria, context, currentCursor, out totalMatchingRecords);
        ///  
        ///                      Console.WriteLine("Total matching items: {0}", totalMatchingRecords);
        /// 
        ///                      foreach (SimilarRecord item in matchingItems)
        ///                      {
        ///                          Console.WriteLine(
        ///                              string.Format("\nId = {0} \nTitle = {1}", 
        ///                              item.MatchingResource.Id, item.MatchingResource));
        ///                          Console.WriteLine(string.Format("PercentageMatch = {0}", item.PercentageMatch));
        ///                      }
        ///  
        ///                      // Retrieve next 10 matching records.
        ///                      currentCursor += pageSize;
        ///                      if (currentCursor &lt; totalMatchingRecords)
        ///                      {
        ///                          matchingItems =
        ///                              search.SearchSimilarResources(
        ///                                 resourceType, searchCriteria, context, currentCursor, out totalMatchingRecords);
        /// 
        ///                          foreach (SimilarRecord item in matchingItems)
        ///                          {
        ///                              Console.WriteLine(
        ///                                  string.Format("\nId = {0} \nTitle = {1}", 
        ///                                  item.MatchingResource.Id, item.MatchingResource));
        ///                              Console.WriteLine(string.Format("PercentageMatch = {0}", item.PercentageMatch));
        ///                          }
        ///                      }
        ///  
        ///                      // If there are more number of records that are to be fetched then call Search function again to 
        ///                      // retrieve the next set of results.
        ///                      currentCursor += pageSize;
        ///                      while (currentCursor &lt; totalMatchingRecords)
        ///                      {
        ///                          matchingItems =
        ///                              search.SearchSimilarResources(
        ///                                 resourceType, searchCriteria, context, currentCursor, out totalMatchingRecords);
        ///                          foreach (SimilarRecord item in matchingItems)
        ///                          {
        ///                              Console.WriteLine(
        ///                                  string.Format("\nId = {0} \nTitle = {1}",
        ///                                  item.MatchingResource.Id, item.MatchingResource));
        ///                              Console.WriteLine(string.Format("PercentageMatch = {0}", item.PercentageMatch));
        ///                          }
        ///                          currentCursor += pageSize;
        ///                      }
        ///                  }
        ///              }
        ///              catch (ArgumentNullException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///              catch (ArgumentException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///              catch (SearchException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///              catch (ConfigurationErrorsException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///              catch (SqlException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///          }
        ///     }
        /// }
        /// </code>
        /// </example>
        public IEnumerable<SimilarRecord> SearchSimilarResources(
            string resourceType, IEnumerable<PropertyValuePair> searchCriteria,
            ZentityContext context, int currentCursor, out int totalRecords)
        {
            // Validate paramters.
            if (!AreValidParameters(searchCriteria, context, currentCursor))
            {
                totalRecords = 0;
                return Utility.CreateEmptyEnumerable<SimilarRecord>();
            }

            SearchTokens searchTokens = new SearchTokens(context);
            
            string searchQuery = 
                ValidateAndPrepareSearchQuery(searchCriteria, resourceType, 
                searchTokens);

            Parser parser = new Parser(searchTokens);
            Interpreter interpreter = 
                new Interpreter(searchTokens, MaximumResultCount);

            string resourceTypeFullName;
            TreeNode root = 
                parser.Parse(searchQuery, out resourceTypeFullName);

            Dictionary<Guid, float> matchingResourceInfo = 
                new Dictionary<Guid, float>();
            if (IsSecurityAware)
            {
                matchingResourceInfo = interpreter.Interpret(
                resourceTypeFullName, root, searchCriteria, AuthenticatedToken, 
                currentCursor, out totalRecords);
            }
            else
            {
                matchingResourceInfo = interpreter.Interpret(
                    resourceTypeFullName, root, searchCriteria, null, 
                    currentCursor, out totalRecords);
            }

            IEnumerable<Resource> resourceList =
                FetchMatchingResources<Resource>(context, resourceTypeFullName, 
                matchingResourceInfo.Keys, null);

            return resourceList
                .Join<Resource, KeyValuePair<Guid, float>, Guid, SimilarRecord>
                (matchingResourceInfo, res => res.Id, m => m.Key, (res, m) => 
                    new SimilarRecord { MatchingResource = res, PercentageMatch = m.Value })
                    .OrderByDescending(s => s.PercentageMatch);
        }

        /// <summary>
        /// Searches for files on a given search string. Results will be paged
        /// according to input criteria.
        /// </summary>
        /// <param name="searchCriteria">Search string.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch
        /// data with.</param>
        /// <param name="currentCursor">The number of matching records to skip.
        /// </param>
        /// <param name="totalRecords">The total number of records matching the 
        /// search criteria.</param>
        /// <returns>Files.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid.
        /// </exception>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        /// Thrown when file specified in the application settings is not found 
        /// OR the file extension is not valid 
        /// OR the config file does not conform to its Xsd.</exception>
        /// <exception cref="SearchException">
        /// <paramref name="searchCriteria"/> is invalid  
        /// OR Full text search is not enabled.
        /// </exception>
        /// <exception cref="System.Data.SqlClient.SqlException">
        /// If Sql server throws an exception.</exception>
        /// <example>
        /// This example searches for files in Zentity repository containing
        /// any of the words in the given <paramref name="searchCriteria"/>.
        /// <code>
        /// using System;
        /// using System.Data.SqlClient;
        /// using System.Configuration;
        /// using System.Collections.Generic;
        /// using Zentity.Core;
        /// using Zentity.Platform;
        /// namespace Zentity.Platform.Samples
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             try
        ///             {
        ///                 using(ZentityContext context = new ZentityContext())
        ///                 {
        ///                     string searchString = "programming compiler";
        ///                     // Specify the page size.
        ///                     int pageSize = 10;
        ///                     SearchEngine search = new SearchEngine(pageSize);
        ///         
        ///                     int currentCursor = 0;
        ///                     int totalMatchingRecords;
        ///         
        ///                     // Retrieve 1st 10 matching records.
        ///                     IEnumerable&lt;File&gt; matchingItems = 
        ///                         search.SearchContent(
        ///                         searchString, context, currentCursor, out totalMatchingRecords);
        ///
        ///                     Console.WriteLine("Total matching files: {0}", totalMatchingRecords);
        ///         
        ///                     foreach (Resource item in matchingItems)
        ///                     {
        ///                         Console.WriteLine(string.Format("\nId = {0} \nTitle = {1}", item.Id, item.Title));
        ///                     }
        ///
        ///                     // Retrieve next 10 matching records.
        ///                     currentCursor += pageSize;
        ///                     if (currentCursor &lt; totalMatchingRecords)
        ///                     {
        ///                         matchingItems =
        ///                             search.SearchContent(
        ///                             searchString, context, currentCursor, out totalMatchingRecords);
        ///         
        ///                         foreach (Resource item in matchingItems)
        ///                         {
        ///                             Console.WriteLine(string.Format("\nId = {0} \nTitle = {1}", item.Id, item.Title));
        ///                         }
        ///                     }
        ///
        ///                     // If there are more number of records that are to be fetched then call Search function again to 
        ///                     // retrieve the next set of results.
        ///                     currentCursor += pageSize;
        ///                     while (currentCursor &lt; totalMatchingRecords)
        ///                     {
        ///                         matchingItems =
        ///                         search.SearchContent(searchString, context, currentCursor, out totalMatchingRecords);
        ///                         foreach (Resource item in matchingItems)
        ///                         {
        ///                             Console.WriteLine(string.Format("\nId = {0} \nTitle = {1}", item.Id, item.Title));
        ///                         }
        ///                         currentCursor += pageSize;
        ///                     }
        ///                 }
        ///             }
        ///              catch (ArgumentNullException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///              catch (ArgumentException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///              catch (SearchException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///              catch (ConfigurationErrorsException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///              catch (SqlException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public IEnumerable<File> SearchContent(
                                            string searchCriteria, 
                                            ZentityContext context, 
                                            int currentCursor, 
                                            out int totalRecords)
        {
            return HandleContentSearch(searchCriteria, context, null, 
                currentCursor, out totalRecords);
        }

        /// <summary>
        /// Searches for files on a given search string. Results will be sorted and paged
        /// according to input criteria.
        /// </summary>
        /// <param name="searchCriteria">Search string.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to 
        /// fetch data with.</param>
        /// <param name="sortProperty">A property on which to sort and sort 
        /// direction.</param>
        /// <param name="currentCursor">The number of matching records to 
        /// skip.</param>
        /// <param name="totalRecords">The total number of records matching 
        /// the search criteria.</param>
        /// <returns>Files.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid.
        /// </exception>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        /// Thrown when file specified in the application settings is not found 
        /// OR the file extension is not valid 
        /// OR the config file does not conform to its Xsd.</exception>
        /// <exception cref="SearchException">
        /// <paramref name="searchCriteria"/> is invalid  
        /// OR Full text search is not enabled.
        /// </exception>
        /// <exception cref="System.Data.SqlClient.SqlException">
        /// If Sql server throws an exception.</exception>
        /// <example>
        /// This example searches for files in Zentity repository containing
        /// any of the words in the given <paramref name="searchCriteria"/>.
        /// <code>
        /// using System;
        /// using System.Data.SqlClient;
        /// using System.Configuration;
        /// using System.Collections.Generic;
        /// using Zentity.Core;
        /// using Zentity.Platform;
        /// namespace Zentity.Platform.Samples
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             try
        ///             {
        ///                 using(ZentityContext context = new ZentityContext())
        ///                 {
        ///                      string searchString = "programming compiler";
        ///                      // Specify the page size.
        ///                      int pageSize = 10;
        ///                      SearchEngine search = new SearchEngine(pageSize);
        ///         
        ///                      int currentCursor = 0;
        ///                      int totalMatchingRecords;
        ///
        ///                      // To sort result set according to 'Title' in descending order.
        ///                      SortProperty sortProperty = new SortProperty("Title", SortDirection.Descending);
        ///
        ///                      // Retrieve 1st 10 matching records.
        ///                      IEnumerable&lt;File&gt; matchingItems = 
        ///                          search.SearchContent(
        ///                          searchString, context, sortProperty, currentCursor, out totalMatchingRecords);
        ///
        ///                      Console.WriteLine("Total matching files: {0}", totalMatchingRecords);
        ///         
        ///                      foreach (Resource item in matchingItems)
        ///                      {
        ///                          Console.WriteLine(string.Format("\nId = {0} \nTitle = {1}", item.Id, item.Title));
        ///                      }
        ///
        ///                      // Retrieve next 10 matching records.
        ///                      currentCursor += pageSize;
        ///                      if (currentCursor &lt; totalMatchingRecords)
        ///                      {
        ///                          matchingItems =
        ///                              search.SearchContent(
        ///                              searchString, context, sortProperty, currentCursor, out totalMatchingRecords);
        ///         
        ///                          foreach (Resource item in matchingItems)
        ///                          {
        ///                              Console.WriteLine(string.Format("\nId = {0} \nTitle = {1}", item.Id, item.Title));
        ///                          }
        ///                      }
        ///
        ///                      // If there are more number of records that are to be fetched then call Search function again to 
        ///                      // retrieve the next set of results.
        ///                      currentCursor += pageSize;
        ///                      while (currentCursor &lt; totalMatchingRecords)
        ///                      {
        ///                          matchingItems =
        ///                          search.SearchContent(
        ///                          searchString, context, sortProperty, currentCursor, out totalMatchingRecords);
        ///                          foreach (Resource item in matchingItems)
        ///                          {
        ///                              Console.WriteLine(string.Format("\nId = {0} \nTitle = {1}", item.Id, item.Title));
        ///                          }
        ///                          currentCursor += pageSize;
        ///                      }
        ///                  }
        ///              }
        ///              catch (ArgumentNullException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///              catch (ArgumentException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///              catch (SearchException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///              catch (ConfigurationErrorsException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///              catch (SqlException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///          }
        ///     }
        /// }
        /// </code>
        /// </example>
        public IEnumerable<File> SearchContent(
                                            string searchCriteria, 
                                            ZentityContext context, 
                                            SortProperty sortProperty, 
                                            int currentCursor, 
                                            out int totalRecords)
        {
            if (sortProperty == null)
            {
                throw new ArgumentNullException("sortProperty");
            }
            return HandleContentSearch(searchCriteria, context, sortProperty, 
                currentCursor, out totalRecords);
        }

        /// <summary>
        /// Searches for resources on a given search string. Results will be paged
        /// according to input criteria.
        /// </summary>
        /// <param name="searchCriteria">Search string.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to fetch 
        /// data with.</param>
        /// <param name="currentCursor">The number of matching records to skip.
        /// </param>
        /// <param name="totalRecords">The total number of records matching the 
        /// search criteria.</param>
        /// <returns>Resources.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid.
        /// </exception>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        /// Thrown when file specified in the application settings is not found 
        /// OR the file extension is not valid 
        /// OR the config file does not conform to its Xsd.</exception>
        /// <exception cref="SearchException">
        /// <paramref name="searchCriteria"/> is invalid.
        /// </exception>
        /// <exception cref="System.Data.SqlClient.SqlException">
        /// If Sql server throws an exception.</exception>
        /// <example>
        /// This example searches for Resources in Zentity repository for the given 
        /// <paramref name="searchCriteria"/>.
        /// <code>
        /// using System;
        /// using System.Data.SqlClient;
        /// using System.Configuration;
        /// using System.Collections.Generic;
        /// using Zentity.Core;
        /// using Zentity.Platform;
        /// using Zentity.ScholarlyWorks;
        /// namespace Zentity.Platform.Samples
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             try
        ///             {
        ///                 using(ZentityContext context = new ZentityContext())
        ///                 {
        ///                     // Load ScholarlyWork metadata.
        ///                     context.MetadataWorkspace.LoadFromAssembly(typeof(ScholarlyWork).Assembly);
        ///                      
        ///                     string searchString = "Title:\"Lord of the rings\"";
        ///                     // Specify the page size.
        ///                     int pageSize = 10;
        ///                     SearchEngine search = new SearchEngine(pageSize);
        /// 
        ///                     int currentCursor = 0;
        ///                     int totalMatchingRecords;
        /// 
        ///                     // Retrieve 1st 10 matching records.
        ///                     IEnumerable&lt;Resource&gt; matchingItems = 
        ///                         search.SearchResources(
        ///                         searchString, context, currentCursor, out totalMatchingRecords);
        /// 
        ///                     Console.WriteLine("Total matching items: {0}", totalMatchingRecords);
        /// 
        ///                     foreach (Resource item in matchingItems)
        ///                     {
        ///                         Console.WriteLine(string.Format("\nId = {0} \nTitle = {1}", item.Id, item.Title));
        ///                     }
        /// 
        ///                     // Retrieve next 10 matching records.
        ///                     currentCursor += pageSize;
        ///                     if (currentCursor &lt; totalMatchingRecords)
        ///                     {
        ///                         matchingItems =
        ///                             search.SearchResources(
        ///                             searchString, context, currentCursor, out totalMatchingRecords);
        /// 
        ///                         foreach (Resource item in matchingItems)
        ///                         {
        ///                             Console.WriteLine(string.Format("\nId = {0} \nTitle = {1}", item.Id, item.Title));
        ///                         }
        ///                     }
        /// 
        ///                     // If there are more number of records that are to be fetched then call Search function again to 
        ///                     // retrieve the next set of results.
        ///                     currentCursor += pageSize;
        ///                     while (currentCursor &lt; totalMatchingRecords)
        ///                     {
        ///                         matchingItems =
        ///                         search.SearchResources(searchString, context, currentCursor, out totalMatchingRecords);
        ///                         foreach (Resource item in matchingItems)
        ///                         {
        ///                             Console.WriteLine(string.Format("\nId = {0} \nTitle = {1}", item.Id, item.Title));
        ///                         }
        ///                         currentCursor += pageSize;
        ///                     }
        ///                 }
        ///             }
        ///             catch (ArgumentNullException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///             catch (ArgumentException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///             catch (SearchException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///             catch (ConfigurationErrorsException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///             catch (SqlException exception)
        ///             {
        ///                 Console.WriteLine("Exception : {0}", exception.Message);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public IEnumerable<Resource> SearchResources(
                                                    string searchCriteria, 
                                                    ZentityContext context, 
                                                    int currentCursor, 
                                                    out int totalRecords)
        {
            return HandleResourceSearch(searchCriteria, context, null, 
                currentCursor, out totalRecords);
        }

        /// <summary>
        /// Searches for resources on a given search string. Results will be 
        /// sorted and paged according to input criteria.
        /// </summary>
        /// <param name="searchCriteria">Search string.</param>
        /// <param name="context"><see cref="ZentityContext" /> instance to 
        /// fetch data with.</param>
        /// <param name="sortProperty">A property on which to sort and sort 
        /// direction.</param>
        /// <param name="currentCursor">The number of matching records to skip.
        /// </param>
        /// <param name="totalRecords">The total number of records matching the
        /// search criteria.</param>
        /// <returns>Resources.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the given arguments are null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If any of the given arguments are not valid.
        /// </exception>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        /// Thrown when file specified in the application settings is not found 
        /// OR the file extension is not valid 
        /// OR the config file does not conform to its Xsd.</exception>
        /// <exception cref="SearchException">
        /// <paramref name="searchCriteria"/> is invalid.
        /// </exception>
        /// <exception cref="System.Data.SqlClient.SqlException">
        /// If Sql server throws an exception.</exception>
        /// <example>
        /// This example searches for Resources in Zentity repository for the given 
        /// <paramref name="searchCriteria"/>.
        /// <code>
        /// using System;
        /// using System.Data.SqlClient;
        /// using System.Configuration;
        /// using System.Collections.Generic;
        /// using Zentity.Core;
        /// using Zentity.Platform;
        /// using Zentity.ScholarlyWorks;
        /// namespace Zentity.Platform.Samples
        /// {
        ///     class Program
        ///     {
        ///         static void Main(string[] args)
        ///         {
        ///             try
        ///             {
        ///                 using(ZentityContext context = new ZentityContext())
        ///                 {
        ///                      // Load ScholarlyWork metadata.
        ///                      context.MetadataWorkspace.LoadFromAssembly(typeof(ScholarlyWork).Assembly);
        ///                      
        ///                      string searchString = "Title:\"Lord of the rings\"";
        ///                      // Specify the page size.
        ///                      int pageSize = 10;
        ///                      SearchEngine search = new SearchEngine(pageSize);
        ///  
        ///                      int currentCursor = 0;
        ///                      int totalMatchingRecords;
        /// 
        ///                      // To sort result set according to 'Title' in descending order.
        ///                      SortProperty sortProperty = new SortProperty("Title", SortDirection.Descending);
        /// 
        ///                      // Retrieve 1st 10 matching records.
        ///                      IEnumerable&lt;Resource&gt; matchingItems = 
        ///                          search.SearchResources(
        ///                          searchString, context, sortProperty, currentCursor, out totalMatchingRecords);
        ///  
        ///                      Console.WriteLine("Total matching items: {0}", totalMatchingRecords);
        ///  
        ///                      foreach (Resource item in matchingItems)
        ///                      {
        ///                          Console.WriteLine(string.Format("\nId = {0} \nTitle = {1}", item.Id, item.Title));
        ///                      }
        ///  
        ///                      // Retrieve next 10 matching records.
        ///                      currentCursor += pageSize;
        ///                      if (currentCursor &lt; totalMatchingRecords)
        ///                      {
        ///                          matchingItems =
        ///                              search.SearchResources(
        ///                              searchString, context, sortProperty, currentCursor, out totalMatchingRecords);
        ///  
        ///                          foreach (Resource item in matchingItems)
        ///                          {
        ///                              Console.WriteLine(string.Format("\nId = {0} \nTitle = {1}", item.Id, item.Title));
        ///                          }
        ///                      }
        ///  
        ///                      // If there are more number of records that are to be fetched then call Search function again to 
        ///                      // retrieve the next set of results.
        ///                      currentCursor += pageSize;
        ///                      while (currentCursor &lt; totalMatchingRecords)
        ///                      {
        ///                          matchingItems =
        ///                          search.SearchResources(searchString, context, sortProperty, currentCursor, out totalMatchingRecords);
        ///                          foreach (Resource item in matchingItems)
        ///                          {
        ///                              Console.WriteLine(string.Format("\nId = {0} \nTitle = {1}", item.Id, item.Title));
        ///                          }
        ///                          currentCursor += pageSize;
        ///                      }
        ///                  }
        ///              }
        ///              catch (ArgumentNullException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///              catch (ArgumentException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///              catch (SearchException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///              catch (ConfigurationErrorsException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///              catch (SqlException exception)
        ///              {
        ///                  Console.WriteLine("Exception : {0}", exception.Message);
        ///              }
        ///          }
        ///     }
        /// }
        /// </code>
        /// </example>
        public IEnumerable<Resource> SearchResources(
                                                    string searchCriteria, 
                                                    ZentityContext context, 
                                                    SortProperty sortProperty, 
                                                    int currentCursor, 
                                                    out int totalRecords)
        {
            if (sortProperty == null)
            {
                throw new ArgumentNullException("sortProperty");
            }
            return HandleResourceSearch(searchCriteria, context, sortProperty, 
                currentCursor, out totalRecords);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the content search.
        /// </summary>
        /// <param name="searchCriteria">The search criteria.</param>
        /// <param name="context">The context.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="currentCursor">The current cursor.</param>
        /// <param name="totalRecords">The total records.</param>
        /// <returns>List of <see cref="File"/>.</returns>
        private IEnumerable<File> HandleContentSearch(
                                                string searchCriteria, 
                                                ZentityContext context, 
                                                SortProperty sortProperty, 
                                                int currentCursor,
                                                out int totalRecords)
        {
            // Validate parameters.
            if (!AreValidParameters(searchCriteria, context, currentCursor))
            {
                totalRecords = 0;
                return Utility.CreateEmptyEnumerable<File>();
            }

            // Check whether FTS is enabled on DB or not.
            CheckFTS(context);

            searchCriteria = Utility.EscapeTSqlSingleQuoteValue(searchCriteria);

            string sortColumn;
            string transactionSqlQuery = CreateMatchingFilesTsql(
                                                                searchCriteria, 
                                                                context, 
                                                                sortProperty, 
                                                                out sortColumn).ToString();

            if (IsSecurityAware)
            {
                transactionSqlQuery = Utility.AppendAuthorizationCriteria(
                    AuthenticatedToken, transactionSqlQuery, context.StoreConnectionString);
            }
            IEnumerable<Guid> resourceIds = Utility.FetchIdsOfMatchingResources(
                transactionSqlQuery, context.StoreConnectionString, sortProperty,
                sortColumn, currentCursor, MaximumResultCount, out totalRecords);

            return FetchMatchingResources<File>(context, 
                SearchConstants.FILERESOURCETYPE, resourceIds, sortProperty);
        }

        /// <summary>
        /// Handles the resource search.
        /// </summary>
        /// <param name="searchCriteria">The search criteria.</param>
        /// <param name="context">The context.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="currentCursor">The current cursor.</param>
        /// <param name="totalRecords">The total records.</param>
        /// <returns>List of <see cref="Resource"/>.</returns>
        private IEnumerable<Resource> HandleResourceSearch(
                                                        string searchCriteria, 
                                                        ZentityContext context, 
                                                        SortProperty sortProperty, 
                                                        int currentCursor, 
                                                        out int totalRecords)
        {
            // Validate parameters.
            if (!AreValidParameters(searchCriteria, context, currentCursor))
            {
                totalRecords = 0;
                return Utility.CreateEmptyEnumerable<Resource>();
            }

            SearchTokens searchTokens = new SearchTokens(context);

            // Parse search query.
            Parser parser = new Parser(searchTokens);
            string resourceTypeFullName;
            TreeNode root = parser.Parse(searchCriteria, out resourceTypeFullName);

            AuthenticatedToken authenticatedToken = null;
            if (IsSecurityAware)
            {
                authenticatedToken = AuthenticatedToken;
            }

            //Interpret tree
            Interpreter interpreter = new Interpreter(searchTokens, MaximumResultCount);
            IEnumerable<Guid> resourceIds = Utility.CreateEmptyEnumerable<Guid>();
            if (sortProperty != null)
            {
                resourceIds = interpreter.Interpret(root, sortProperty, 
                    resourceTypeFullName, authenticatedToken, currentCursor,
                    out totalRecords);
            }
            else
            {
                resourceIds =
                    interpreter.Interpret(
                    root, authenticatedToken, currentCursor, out totalRecords);
            }
            return FetchMatchingResources<Resource>(context, resourceTypeFullName, 
                resourceIds, sortProperty);
        }

        /// <summary>
        /// Checks if the full text search is enabled.
        /// </summary>
        /// <param name="context">The context.</param>
        private static void CheckFTS(ZentityContext context)
        {
            bool ftsEnabled;
            if (Boolean.TryParse(
                context.GetConfiguration("IsFullTextSearchEnabled"), out ftsEnabled))
            {
                if (!ftsEnabled)
                {
                    throw new SearchException(Resources.SEARCH_FTS_NOT_ENABLED);
                }
            }
        }

        /// <summary>
        /// Determines if the parameters are valid.
        /// </summary>
        /// <param name="searchCriteria">The search criteria.</param>
        /// <param name="context">The context.</param>
        /// <param name="currentCursor">The current cursor.</param>
        /// <returns>
        ///     <c>true</c> if the parameters are valid; otherwise <c>false</c>.
        /// </returns>
        private bool AreValidParameters(
                                    string searchCriteria, 
                                    ZentityContext context, 
                                    int currentCursor)
        {
            if (string.IsNullOrEmpty(searchCriteria) 
                || string.IsNullOrEmpty(searchCriteria.Trim())
                || MaximumResultCount == 0)
            {
                return false;
            }

            return AreValidParameters(currentCursor, context);
        }

        /// <summary>
        /// Determines if the parameters are valid.
        /// </summary>
        /// <param name="searchCriteria">The search criteria.</param>
        /// <param name="context">The context.</param>
        /// <param name="currentCursor">The current cursor.</param>
        /// <returns>
        ///     <c>true</c> if the parameters are valid; otherwise <c>false</c>.
        /// </returns>
        private bool AreValidParameters(
                                    IEnumerable<PropertyValuePair> searchCriteria, 
                                    ZentityContext context, 
                                    int currentCursor)
        {
            if (searchCriteria == null || searchCriteria.Count() == 0
                || MaximumResultCount == 0)
            {
                return false;
            }

            return AreValidParameters(currentCursor, context);
        }

        /// <summary>
        /// Determines if the parameters are valid.
        /// </summary>
        /// <param name="currentCursor">The current cursor.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        ///     <c>true</c> if the parameters are valid; otherwise <c>false</c>.
        /// </returns>
        private bool AreValidParameters(int currentCursor, ZentityContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (currentCursor < 0)
            {
                throw new ArgumentException(
                    String.Format(
                    CultureInfo.CurrentCulture,
                    Resources.SEARCH_VALUE_LESS_THAN_ZERO, "currentCursor"));
            }

            if (IsSecurityAware && AuthenticatedToken == null)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, 
                    Resources.SEARCH_AUTHENTICATED_TOKEN_NULL, 
                    "AuthenticatedToken", "IsSecurityAware"));
            }

            return true;
        }

        /// <summary>
        /// Fetches the matching resources.
        /// </summary>
        /// <typeparam name="T">Resource type.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="resourceTypeFullName">Full name of the resource type.</param>
        /// <param name="ids">List of ids.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <returns>List of resource type.</returns>
        private static IEnumerable<T> FetchMatchingResources<T>(
                                                            ZentityContext context, 
                                                            string resourceTypeFullName, 
                                                            IEnumerable<Guid> ids, 
                                                            SortProperty sortProperty) where T : Resource
        {
            if (ids.Count() > 0)
            {
                StringBuilder commaSeparatedIds = new StringBuilder();

                foreach (Guid id in ids)
                {
                    AppendCriterionESql(commaSeparatedIds, id);
                    commaSeparatedIds.Append(SearchConstants.COMMA);
                }
                commaSeparatedIds.Remove(commaSeparatedIds.Length - 1, 1);

                return CreateESqlQuery<T>(context, resourceTypeFullName, 
                    commaSeparatedIds.ToString(), sortProperty);
            }
            else
            {
                return Utility.CreateEmptyEnumerable<T>();
            }
        }

        /// <summary>
        /// Creates the execute SQL query.
        /// </summary>
        /// <typeparam name="T">The query type.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="resourceTypeFullName">Full name of the resource type.</param>
        /// <param name="commaSeparatedIds">The comma separated ids.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <returns>List of the query type.</returns>
        private static IEnumerable<T> CreateESqlQuery<T>(
                                                        ZentityContext context,
                                                        string resourceTypeFullName, 
                                                        string commaSeparatedIds, 
                                                        SortProperty sortProperty) where T : Resource
        {
            StringBuilder query = new StringBuilder();
            query.AppendFormat(CultureInfo.CurrentCulture, SearchConstants.ESQL_RESOURCES,
                    Utility.EscapeColumnName(resourceTypeFullName));

            if (!string.IsNullOrEmpty(commaSeparatedIds))
            {
                query.AppendFormat(CultureInfo.CurrentCulture, 
                    SearchConstants.ESQL_RESOURCES_BY_ID_WHERE,
                    commaSeparatedIds);
            }

            // Append sort property and sort direction.
            if (sortProperty != null)
            {
                if (sortProperty.SortDirection == SortDirection.Ascending)
                {
                    query.AppendFormat(
                        CultureInfo.CurrentCulture, SearchConstants.ESQL_ORDER_BY,
                        sortProperty.PropertyName, SearchConstants.TSQL_ASC);
                }
                else
                {
                    query.AppendFormat(
                        CultureInfo.CurrentCulture, SearchConstants.ESQL_ORDER_BY,
                        sortProperty.PropertyName, SearchConstants.TSQL_DESC);
                }
            }
            return context.CreateQuery<T>(query.ToString());
        }

        /// <summary>
        /// Appends the criterion to the execute SQL.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="id">The id.</param>
        private static void AppendCriterionESql(StringBuilder builder, Guid id)
        {
            builder.Append(string.Format(
                CultureInfo.CurrentCulture, SearchConstants.ESQL_CAST_GUID, id));
        }

        /// <summary>
        /// Validates and prepares the search query.
        /// </summary>
        /// <param name="searchCriteria">The search criteria.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <param name="searchTokens">The search tokens.</param>
        /// <returns>The search query.</returns>
        private static string ValidateAndPrepareSearchQuery(
                                                        IEnumerable<PropertyValuePair> searchCriteria,
                                                        string resourceType, 
                                                        SearchTokens searchTokens)
        {
            string resourceTypeFullName = 
                searchTokens.FetchResourceTypeFullName(resourceType);
            StringBuilder query = new StringBuilder();
            foreach (PropertyValuePair pair in searchCriteria)
            {
                ScalarProperty scalarProperty =
                    searchTokens.FetchPropertyToken(
                    pair.PropertyName, resourceTypeFullName);

                if (scalarProperty == null)
                {
                    throw new ArgumentException(
                        String.Format(CultureInfo.CurrentCulture, 
                        Resources.SEARCH_INVALID_PROPERTY_TOKEN,
                        pair.PropertyName));
                }
                else
                {
                    query.Append(
                        pair.PropertyName + SearchConstants.COLON 
                        + SearchConstants.SINGLE_QUOTE
                        + pair.PropertyValue
                        + SearchConstants.SINGLE_QUOTE);
                    query.Append(SearchConstants.SPACE);
                }
            }
            query.Append(SearchConstants.RESOURCETYPE + SearchConstants.COLON + resourceType);

            return query.ToString();
        }

        /// <summary>
        /// Creates the matching files TSQL.
        /// </summary>
        /// <param name="searchCriteria">The search criteria.</param>
        /// <param name="context">The context.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <returns>The query.</returns>
        private static StringBuilder CreateMatchingFilesTsql(
                                                            string searchCriteria,
                                                            ZentityContext context, 
                                                            SortProperty sortProperty, 
                                                            out string sortColumn)
        {
            StringBuilder transactionSqlBuilder = new StringBuilder();
            sortColumn = null;
            SearchTokens searchTokens = new SearchTokens(context);

            // 'Id' column is already in selected columns.
            if (sortProperty == null
                || String.Equals(sortProperty.PropertyName, SearchConstants.TSQL_ID,
                    StringComparison.OrdinalIgnoreCase))
            {
                transactionSqlBuilder.AppendFormat(CultureInfo.InvariantCulture,
                    SearchConstants.TSQL_CONTENT_QUERY_SORTING, 
                    String.Empty, searchCriteria);
            }
            else
            {
                // Get scalar property column name.
                sortColumn =
                    Utility.GetColumnName(sortProperty.PropertyName, searchTokens,
                    SearchConstants.FILERESOURCETYPE);

                transactionSqlBuilder.AppendFormat(CultureInfo.InvariantCulture,
                    SearchConstants.TSQL_CONTENT_QUERY_SORTING,
                    SearchConstants.COMMA + SearchConstants.SPACE + sortColumn,
                    searchCriteria);
            }

            transactionSqlBuilder.Append(
                SearchConstants.SPACE + SearchConstants.AND + SearchConstants.SPACE);
            Utility.AppendResourceTypesTSql(
                transactionSqlBuilder, SearchConstants.FILERESOURCETYPE, searchTokens);

            return transactionSqlBuilder;
        }

        #endregion 
    }
}
