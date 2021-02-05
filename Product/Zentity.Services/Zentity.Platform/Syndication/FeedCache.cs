using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;

namespace Microsoft.Famulus.Platform.Syndication
{
    internal class FeedCache
    {
        private static const string CACHE_NAME = "Cache";
        private static CacheManager _cacheManager = CacheFactory.GetCacheManager(CACHE_NAME);
        private int _cacheExpirationTime;
        private Boolean _enableCaching;
        private enum FeedBy
        {
            None = 0,
            Author,
            Contributor,
            Tag
        }

        public FeedCache(Boolean enableCaching, int expirationTime)
        {
            _enableCaching = enableCaching;
            _cacheExpirationTime = (expirationTime > 0 && expirationTime < 30) ? expirationTime : 30;
        }
        #region Utility Functions

        /// <summary>
        /// retrieves the type of resource for the given <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeparamref name="Microsoft.Famulus.Core.Resource"/></typeparam>
        /// <returns>The name of <typeparamref name="T"/></returns>
        internal static string TypeOfResource<T>() where T : Core.Resource
        {
            string typeOfValue = typeof(T).ToString();
            string[] splitValues = typeOfValue.Split(
                new char[] { '.' },
                StringSplitOptions.RemoveEmptyEntries);
            if (null != splitValues)
                return splitValues[splitValues.Length - 1];
            return typeOfValue;

        }

        #endregion
        /// <summary>
        /// Retrieves the Cache Key for the given Id
        /// </summary>
        /// <typeparam name="T">Type of Resource which derive from 
        /// <typeparamref name="Microsoft.Famulus.Core.Resource"/></typeparam>
        /// <param name="id">The id for the key</param>
        /// <param name="feedBy">The associated feedby object</param>
        /// <returns>string format for the key</returns>
        private string GetCacheKey<T>(string name, FeedBy feedBy)
                                   where T : Core.Resource
        {
            StringBuilder cacheKey = new StringBuilder(FeedHelper.TypeOfResource<T>());
            cacheKey.Append("@");
            cacheKey.Append(feedBy.ToString());
            cacheKey.Append("@");
            cacheKey.Append(name.ToString());
            return cacheKey.ToString();
        }
        private string GetCacheKey<T>(Guid id, FeedBy feedBy)
                                    where T : Core.Resource
        {
            return GetCacheKey<T>(id.ToString(), feedBy);
        }
        private string GetAuthorKey<T>(Guid id) where T:Core.ScholarlyWork
        {
            return GetCacheKey<T>(id, FeedBy.Author);
        }
        private string GetTagKey<T>(string label) where T : Core.Resource
        {
            return GetCacheKey<T>(label, FeedBy.None);
        }
        private string GetTagKey<T>(Guid id) where T : Core.Resource
        {
            return GetTagKey<T>(id.ToString());
        }
        private string GetContributorKey<T>(Guid id) where T:Core.ScholarlyWork
        {
            return GetCacheKey<T>(id, FeedBy.Contributor);
        }
        public KeyValuePair<Core.Contact, List<T>> AddContact<T>(Guid id, KeyValuePair<Core.Contact, List<T>> value)
            where T : Core.ScholarlyWork
        {
            if (_enableCaching)
            {
                _cacheManager.Add(GetAuthorKey<T>(id), value);
            }
            return value;
        }
        public KeyValuePair<Core.Contact, List<T>> AddContact<T>(Guid id, KeyValuePair<Core.Contact, List<T>> value, 
            ICacheItemRefreshAction cacheRefreshAction)
            where T : Core.ScholarlyWork
        {
            if (_enableCaching)
            {
                _cacheManager.Add(GetAuthorKey<T>(id), value, 
                    CacheItemPriority.High, cacheRefreshAction,
                    new AbsoluteTime(DateTime.Now.AddMinutes(_cacheExpirationTime)));
            }
            return value;
        }
        public KeyValuePair<Core.Person, List<T>> AddPerson<T>(Guid id, KeyValuePair<Core.Person, List<T>> value)
            where T : Core.ScholarlyWork
        {
            if (_enableCaching)
            {
                _cacheManager.Add(GetAuthorKey<T>(id), value);
            }
            return value;
        }
        public KeyValuePair<Core.Person, List<T>> AddPerson<T>(Guid id, KeyValuePair<Core.Person, List<T>> value,
            ICacheItemRefreshAction cacheRefreshAction)
            where T : Core.ScholarlyWork
        {
            if (_enableCaching)
            {
                _cacheManager.Add(GetAuthorKey<T>(id), value,
                    CacheItemPriority.High, cacheRefreshAction,
                    new AbsoluteTime(DateTime.Now.AddMinutes(_cacheExpirationTime)));
            }
            return value;
        }
        public KeyValuePair<Core.Tag, List<T>> AddTag<T>(Guid tagId, KeyValuePair<Core.Tag, List<T>> value,
            ICacheItemRefreshAction cacheRefreshAction)
            where T:Core.Resource
        {
            if (_enableCaching)
            {
                _cacheManager.Add(GetTagKey<T>(tagId), value,
                    CacheItemPriority.High, cacheRefreshAction,
                    new AbsoluteTime(DateTime.Now.AddMinutes(_cacheExpirationTime)));
            }
            return value;
        }

        public KeyValuePair<Core.Contact, List<T>> AddContributor<T>(Guid contributorId, 
            KeyValuePair<Core.Contact, List<T>> value,
            ICacheItemRefreshAction cacheRefreshAction) where T: Core.ScholarlyWork
        {
            if (_enableCaching)
            {
                _cacheManager.Add(GetContributorKey<T>(contributorId), value,
                    CacheItemPriority.High, cacheRefreshAction,
                    new AbsoluteTime(DateTime.Now.AddMinutes(_cacheExpirationTime)));
            }
            return value;
            //this.Add(GetContributorKey, contributorId, value, cacheRefreshAction);
        }
        /*delegate string GetKey<T>(Guid id);
        private void Add<T>(GetKey getKeyDelegate, 
            Guid id, 
            object value,
            ICacheItemRefreshAction cacheRefreshAction) where T:Core.Resource
        {
            if (_enableCaching)
            {
                _cacheManager.Add(getKeyDelegate(id), value,
                    CacheItemPriority.High, cacheRefreshAction,
                    new AbsoluteTime(DateTime.Now.AddMinutes(_cacheExpirationTime)));
            }
        }*/
        public Boolean hasAuthor<T>(Guid authorId) where T:Core.ScholarlyWork
        {
            return _enableCaching && _cacheManager.Contains(GetAuthorKey<T>(authorId));
        }
        public Boolean hasTag<T>(Guid tagId) where T : Core.Resource
        {
            return _enableCaching && _cacheManager.Contains(GetTagKey<T>(tagId));
        }
        public Boolean hasTag<T>(string tagLabel) where T : Core.Resource
        {
            return _enableCaching && _cacheManager.Contains(GetTagKey<T>(tagLabel));
        }
        public Boolean hadContributor<T>(Guid contributorId) where T:Core.ScholarlyWork
        {
            return _enableCaching && _cacheManager.Contains(GetTagKey<T>(contributorId));
        }

        public KeyValuePair<Core.Contact, List<T>> GetAuthor<T>(Guid authorId) where T:Core.ScholarlyWork
        {
            return (KeyValuePair<Core.Contact, List<T>>)_cacheManager.GetData(GetAuthorKey<T>(authorId));
        }
    }
}
