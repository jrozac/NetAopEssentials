using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace NetCoreAopEssentials.Cache
{

    /// <summary>
    /// Cache manager
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    public class CacheManager<TService, TImplementation>
        where TService : class
        where TImplementation : class, TService
    {

        /// <summary>
        /// get key prefix
        /// </summary>
        private readonly string _keyPrefix;

        /// <summary>
        /// Memory cache 
        /// </summary>
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// Distributed cache 
        /// </summary>
        private readonly IDistributedCache _distributedCache;

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="provider"></param>
        internal CacheManager(IServiceProvider provider)
        {
            // get aspect
            var aspect = AspectProxy<TService>.GetRegisteredAspect<TImplementation, CacheAspect>();
            if(aspect == null)
            {
                throw new ArgumentException($"Aspect is not configured for {typeof(TImplementation)}.");
            }
            _keyPrefix = aspect.KeyPrefix;

            // get cache 
            _memoryCache = provider.GetService<IMemoryCache>();
            _distributedCache = provider.GetService<IDistributedCache>();
        }

        /// <summary>
        /// Get cache 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetCache(string key)
        {
            return GetCache(key, EnumCacheProvider.Memory) ?? GetCache(key, EnumCacheProvider.Distributed);
        }

        /// <summary>
        ///  get cache 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetCache<T>(string key)
        {
            var obj = GetCache(key);
            return obj != null && obj.GetType() == typeof(T) ? (T)obj : default(T);
        }

        /// <summary>
        /// Get cache for provider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public T GetCache<T>(string key, EnumCacheProvider provider)
        {
            var obj = GetCache(key, provider);
            return obj != null && obj.GetType() == typeof(T) ? (T)obj : default(T);
        }

        /// <summary>
        /// Gets cache for selected provider
        /// </summary>
        /// <param name="key"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public object GetCache(string key, EnumCacheProvider provider)
        {
            switch(provider)
            {
                case EnumCacheProvider.Memory:
                    var obj = _memoryCache?.Get(_keyPrefix + key);
                    return obj;
                case EnumCacheProvider.Distributed:
                    var payload = _distributedCache?.Get(_keyPrefix + key);
                    return CacheSerializationUtil.Deserialize(null, payload);
            }
            return null;
        }

        /// <summary>
        /// Remove cahce 
        /// </summary>
        /// <param name="key"></param>
        public void RemoveCache(string key)
        {
            RemoveCache(key, EnumCacheProvider.Memory);
            RemoveCache(key, EnumCacheProvider.Distributed);
        }

        /// <summary>
        /// Remove cache from selected provider 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="provider"></param>
        public void RemoveCache(string key, EnumCacheProvider provider)
        {
            switch(provider)
            {
                case EnumCacheProvider.Memory:
                    _memoryCache?.Remove(_keyPrefix + key);
                    break;
                case EnumCacheProvider.Distributed:
                    _distributedCache?.Remove(_keyPrefix + key);
                    break;
            }
        }

    }
}
