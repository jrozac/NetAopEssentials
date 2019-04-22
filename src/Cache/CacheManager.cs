using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace NetAopEssentials.Cache
{

    /// <summary>
    /// Cache manager
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    public class CacheManager<TImplementation>
        where TImplementation : class
    {

        /// <summary>
        /// Aspect
        /// </summary>
        private readonly CacheAspect<TImplementation> _cacheAspect;

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
            _cacheAspect = provider.GetRequiredService<AspectsContainer>().GetRegisteredAspect<TImplementation, CacheAspect<TImplementation>>();
            if(_cacheAspect == null)
            {
                throw new ArgumentException($"Aspect is not configured for {typeof(TImplementation)}.");
            }

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
            // get full key 
            var fkey = _cacheAspect.GetFullKey(key);
            if(fkey == null)
            {
                return null;
            }

            // get cache 
            switch(provider)
            {
                case EnumCacheProvider.Memory:
                    var obj = _memoryCache?.Get(fkey);
                    return obj;
                case EnumCacheProvider.Distributed:
                    var payload = _distributedCache?.Get(fkey);
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

            // get full key 
            var fkey = _cacheAspect.GetFullKey(key);
            if (fkey == null)
            {
                return;
            }

            // remove cache
            switch (provider)
            {
                case EnumCacheProvider.Memory:
                    _memoryCache?.Remove(fkey);
                    break;
                case EnumCacheProvider.Distributed:
                    _distributedCache?.Remove(fkey);
                    break;
            }
        }

        /// <summary>
        /// Get cache setups
        /// </summary>
        /// <returns></returns>
        public List<MethodCacheSetupInfo> GetCacheSetupInfos()
        {
            return _cacheAspect.GetCacheSetupInfos();
        }

    }
}