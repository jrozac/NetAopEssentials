using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NetCoreAopEssentials.Cache
{

    /// <summary>
    /// Cache aspect
    /// </summary>
    public class CacheAspect : IAspect
    {

        /// <summary>
        /// Cache definitions 
        /// </summary>
        private Dictionary<string, MethodCacheConfiguration> _cacheDefinitions;

        /// <summary>
        /// Key prefix
        /// </summary>
        internal string KeyPrefix { get; private set; }

        /// <summary>
        /// Constructor which injects definitions
        /// </summary>
        /// <param name="cacheDefinitions"></param>
        internal CacheAspect(Dictionary<string, MethodCacheConfiguration> cacheDefinitions, string keyPrefix)
        {
            _cacheDefinitions = cacheDefinitions;
            KeyPrefix = keyPrefix;
        }

        /// <summary>
        /// After execution
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInfo"></param>
        /// <param name="instance"></param>
        /// <param name="args"></param>
        /// <param name="retval"></param>
        /// <param name="returnNoRun"></param>
        /// <returns></returns>
        public object AfterExecution(IServiceProvider provider, MethodInfo methodInfo, object instance, object[] args, object retval, bool returnNoRun)
        {

            // check for definition
            var cfg = GetConfiguration(instance, methodInfo);
            if (cfg == null || returnNoRun)
            {
                return retval;
            }

            // do action
            switch(cfg.Action)
            {
                case EnumCacheAction.Remove:
                    RemoveCachedValue(provider, cfg, args, retval);
                    break;
                case EnumCacheAction.Set:
                    SetCachedValue(provider, cfg, args, retval);
                    break;
            }

            // return 
            return retval;

        }

        /// <summary>
        /// Before function execution, cache is checked for value first.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInfo"></param>
        /// <param name="instance"></param>
        /// <param name="args"></param>
        /// <param name="returnNoRun"></param>
        /// <returns></returns>
        public object BeforeExecution(IServiceProvider provider, MethodInfo methodInfo, object instance, object[] args, out bool returnNoRun)
        {

            // check for definition
            var cfg = GetConfiguration(instance, methodInfo);
            if(cfg == null || cfg.Action != EnumCacheAction.Set)
            {
                returnNoRun = false;
                return null;
            }

            // get cached
            var retval = GetCachedValue(provider, methodInfo, cfg, args);

            // do not run method if value is cached
            returnNoRun = retval != null;

            // return
            return retval;
        }

        /// <summary>
        /// Configure cache 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ConfigureFor<T>() where T : class
        {

        }

        /// <summary>
        /// Get method cache configuration
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private MethodCacheConfiguration GetConfiguration(object service, MethodInfo methodInfo)
        {
            var id = string.Format("{0}.{1}", service.GetType().FullName, methodInfo.ToString());
            return _cacheDefinitions.ContainsKey(id) ? _cacheDefinitions[id] : null;
        }

        /// <summary>
        /// Set cached value 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="val"></param>
        private void SetCachedValue(IServiceProvider provider, MethodCacheConfiguration cfg, object[] args, object retval)
        {
            // get key
            var key = GetKey(provider, cfg, args, retval);
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            // do not set if condition not valid
            bool quit = GeneralUtil.TryRun<CacheAspect, bool>(provider, () => cfg.CacheResultFunc != null && !cfg.CacheResultFunc.Invoke(retval), 
                false, "Failed to exectue cache result func. Error message: {message}.");
            if(quit)
            {
                return;
            }

            // get timeout
            long timeoutOffset = 0;
            if(cfg.TimeoutMsOffsetFunc != null)
            {
                timeoutOffset = GeneralUtil.TryRun<CacheAspect, long>(provider, () => cfg.TimeoutMsOffsetFunc(retval), 0, "Failed to execute timeout offset func with error {messgae}.");
            }
            var timeout = cfg.TimeoutMs + timeoutOffset;
            if(timeout <= 0)
            {
                GeneralUtil.LogError<CacheAspect>(provider, "Timeout is not valid for method {method}.", cfg.MethodInfo.Name);
                return;
            }

            // set cache 
            switch (cfg.Provider)
            {
                case EnumCacheProvider.Memory:
                    IMemoryCache memCache = provider.GetRequiredService<IMemoryCache>();
                    if(retval != null)
                    {
                        memCache.Set(key, retval, TimeSpan.FromMilliseconds(timeout));
                    } else
                    {
                        GeneralUtil.LogError<CacheAspect>(provider, "Trying to cache null value for method {method}.", cfg.MethodInfo);
                    }
                    break;
                case EnumCacheProvider.Distributed:
                    IDistributedCache distCache = provider.GetRequiredService<IDistributedCache>();
                    var bytes = CacheSerializationUtil.Serialize(provider, retval);
                    if (bytes != null)
                    {
                        distCache.Set(key, bytes, new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(timeout)
                        });
                    } else
                    {
                        GeneralUtil.LogError<CacheAspect>(provider, "Trying to cache null value for method {method}.", cfg.MethodInfo);
                    }
                    break;
            }
        }

        /// <summary>
        /// Remove cached value 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="cfg"></param>
        /// <param name="args"></param>
        /// <param name="retval"></param>
        private void RemoveCachedValue(IServiceProvider provider, MethodCacheConfiguration cfg, object[] args, object retval)
        {

            // check key
            var key = GetKey(provider, cfg, args, retval);
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            // do not remove if condition not valid
            bool quit = GeneralUtil.TryRun<CacheAspect, bool>(provider, () => cfg.CacheResultFunc != null && !cfg.CacheResultFunc.Invoke(retval),
                false, "Failed to exectue cache result func. Error message: {message}.");
            if (quit)
            {
                return;
            }

            // remove 
            switch(cfg.Provider)
            {
                case EnumCacheProvider.Memory:
                    IMemoryCache memoryCache = provider.GetRequiredService<IMemoryCache>();
                    memoryCache.Remove(key);
                    break;
                case EnumCacheProvider.Distributed:
                    IDistributedCache distCache = provider.GetRequiredService<IDistributedCache>();
                    distCache.Remove(key);
                    break;
            }
        }

        /// <summary>
        /// Get cached value 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInfo"></param>
        /// <param name="cfg"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private object GetCachedValue(IServiceProvider provider, MethodInfo methodInfo, MethodCacheConfiguration cfg, object[] args)
        {

            // get key 
            var key = GetKey(provider, cfg, args, null);
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            // get cache 
            object cached = null;
            switch(cfg.Provider)
            {
                case EnumCacheProvider.Memory:
                    IMemoryCache memCache = provider.GetRequiredService<IMemoryCache>();
                    cached = memCache.Get(key);
                    break;
                case EnumCacheProvider.Distributed:
                    IDistributedCache distCache = provider.GetRequiredService<IDistributedCache>();
                    var bytes = distCache.Get(key);
                    cached = CacheSerializationUtil.Deserialize(provider, bytes);
                    break;
            }

            // quit if cached value not available 
            if(cached == null)
            {
                return null;
            }

            // check for type
            cached = cached.GetType() == methodInfo.ReturnType ? cached : null;
            if(cached == null)
            {
                GeneralUtil.LogError<CacheAspect>(provider, "Cached type is not valid for method {method}.", methodInfo.Name);
            }

            // return
            return cached;

        }

        /// <summary>
        /// Gets key 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="cfg"></param>
        /// <param name="args"></param>
        /// <param name="retval"></param>
        /// <returns></returns>
        private string GetKey(IServiceProvider provider, MethodCacheConfiguration cfg, object[] args, object retval)
        {
            // get key 
            string keyItem = GeneralUtil.TryRun<CacheAspect, string>(provider, () => cfg.KeyFunc(args, retval), 
                null, "Failed to get cache key with error: {message}.");
            if(string.IsNullOrWhiteSpace(keyItem))
            {
                return null;
            }
            var key = KeyPrefix + keyItem;
                
            // return key                
            return key;
        }

    }
}
