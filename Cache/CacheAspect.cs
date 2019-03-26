using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NetCoreAopEssentials.Cache.Models;
using NetCoreAopEssentials.Cache.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetCoreAopEssentials.Cache
{

    /// <summary>
    /// Cache aspect
    /// </summary>
    public class CacheAspect<TImplementation> : IAspect<TImplementation>
        where TImplementation : class
    {

        /// <summary>
        /// Cache definitions 
        /// </summary>
        private Dictionary<string, MethodCacheConfiguration> _cacheDefinitions;

        /// <summary>
        /// Constructor which injects definitions
        /// </summary>
        /// <param name="cacheDefinitions"></param>
        internal CacheAspect(List<MethodCacheConfiguration> cacheDefinitions)
        {
            ImportDefinitions(cacheDefinitions);
        }

        /// <summary>
        /// Constructor default
        /// </summary>
        public CacheAspect()
        {
            var defaults = new CacheSetupDefaults();
            var cacheDefinitions = CacheSetupUtil.GetAttributesProfiles<TImplementation>().Select(profile =>
                CacheSetupUtil.CreateCacheConfiguration(profile, defaults)).ToList();
            ImportDefinitions(cacheDefinitions);
        }

        /// <summary>
        /// Get key 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetFullKey(string key)
        {
            var prefix = _cacheDefinitions.Count() > 0 ? _cacheDefinitions.First().Value.KeyPrefix : null;
            return prefix != null ? prefix + key : null;
        }

        /// <summary>
        /// Import definitions
        /// </summary>
        /// <param name="cacheDefinitions"></param>
        private void ImportDefinitions(List<MethodCacheConfiguration> cacheDefinitions)
        {
            // set definitions
            _cacheDefinitions = cacheDefinitions.ToDictionary(
                cfg => string.Format("{0}.{1}", cfg.MethodInfo.ReflectedType.FullName, cfg.MethodInfo.ToString()),
                cfg => cfg);
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
        public void Configure()
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
            bool quit = GeneralUtil.TryRun<CacheAspect<TImplementation>, bool>(provider, () => cfg.CacheResultFunc != null && !cfg.CacheResultFunc.Invoke(retval), 
                false, "Failed to exectue cache result func. Error message: {message}.");
            if(quit)
            {
                return;
            }

            // get timeout
            long timeoutOffset = 0;
            if(cfg.TimeoutMsOffsetFunc != null)
            {
                timeoutOffset = GeneralUtil.TryRun<CacheAspect<TImplementation>, long>(provider, () => cfg.TimeoutMsOffsetFunc(retval), 0, "Failed to execute timeout offset func with error {messgae}.");
            }
            var timeout = cfg.TimeoutMs + timeoutOffset;
            if(timeout <= 0)
            {
                GeneralUtil.LogError<CacheAspect<TImplementation>>(provider, "Timeout is not valid for method {method}.", cfg.MethodInfo.Name);
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
                        GeneralUtil.LogError<CacheAspect<TImplementation>>(provider, "Trying to cache null value for method {method}.", cfg.MethodInfo);
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
                        GeneralUtil.LogError<CacheAspect<TImplementation>>(provider, "Trying to cache null value for method {method}.", cfg.MethodInfo);
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
            bool quit = GeneralUtil.TryRun<CacheAspect<TImplementation>, bool>(provider, () => cfg.CacheResultFunc != null && !cfg.CacheResultFunc.Invoke(retval),
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
                GeneralUtil.LogError<CacheAspect<TImplementation>>(provider, "Cached type is not valid for method {method}.", methodInfo.Name);
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
            string keyItem = GeneralUtil.TryRun<CacheAspect<TImplementation>, string>(provider, () => cfg.KeyFunc(args, retval), 
                null, "Failed to get cache key with error: {message}.");
            if(string.IsNullOrWhiteSpace(keyItem))
            {
                return null;
            }

            // set key 
            string key = cfg.KeyPrefix + keyItem;
                
            // return key                
            return key;
        }

        /// <summary>
        /// Get cache setup
        /// </summary>
        /// <returns></returns>
        internal List<MethodCacheSetup> GetCacheSetup() {
            return _cacheDefinitions.Select(p => new MethodCacheSetup
            {
                Action = p.Value.Action,
                CacheResultFunc  = p.Value.CacheResultFunc != null,
                KeyPrefix = p.Value.KeyPrefix,
                KeyTpl = p.Value.KeyTpl,
                Method = p.Key,
                Provider = p.Value.Provider,
                TimeoutMs = p.Value.TimeoutMs,
                TimeoutMsOffsetFunc = p.Value.TimeoutMsOffsetFunc != null
            }).ToList();
        }

    }
}
