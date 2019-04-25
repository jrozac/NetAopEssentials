using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetAopEssentials.Cache
{

    /// <summary>
    /// Cache aspect
    /// </summary>
    public class CacheAspect<TImplementation> : IAspect<TImplementation>
        where TImplementation : class
    {

        /// <summary>
        /// Cache plans 
        /// </summary>
        private Dictionary<string, MethodCachePlan> _cachePlans;

        /// <summary>
        /// Cache setup
        /// </summary>
        private CacheSetup<TImplementation> _setup;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CacheAspect()
        {
            _setup = new CacheSetup<TImplementation>();
            _setup.ImportAttributesSetup();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="setupAction"></param>
        public CacheAspect(Action<CacheSetup<TImplementation>> setupAction)
        {

            // create setup 
            _setup = new CacheSetup<TImplementation>();
            setupAction?.Invoke(_setup);

            // use attributes if setup action not defined
            if (setupAction == null)
            {
                _setup.ImportAttributesSetup();
            }
        }
       
        /// <summary>
        /// After execution
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInfo"></param>
        /// <param name="instance"></param>
        /// <param name="args"></param>
        /// <param name="retval"></param>
        /// <param name="mainMethodDisabled"></param>
        /// <param name="mainMethodException"></param>
        /// <returns></returns>
        public object AfterExecution(IServiceProvider provider, MethodInfo methodInfo, object instance, object[] args, 
            object retval, bool mainMethodDisabled, Exception mainMethodException)
        {
            // main method not executed properly
            if(mainMethodDisabled || mainMethodException != null)
            {
                return retval;
            }

            // check for plans
            var cfg = GetCachePlanForMethod(instance, methodInfo);
            if (cfg == null)
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
        /// <param name="retval"></param>
        /// <param name="mainMethodDisabled"></param>
        /// <param name="disableMainMethod"></param>
        /// <param name="disableAfterExecution"></param>
        /// <returns></returns>
        public object BeforeExecution(IServiceProvider provider, MethodInfo methodInfo, object instance, object[] args, object retval, 
            bool mainMethodDisabled, out bool disableMainMethod, out bool disableAfterExecution)
        {

            // do not run if main method is disabled
            if(mainMethodDisabled)
            {
                disableMainMethod = true;
                disableAfterExecution = true;
                return retval;
            }

            // check for definition
            var cfg = GetCachePlanForMethod(instance, methodInfo);
            if(cfg == null)
            {
                disableMainMethod = false;
                disableAfterExecution = true;
                return retval;
            }

            // action remove (not set), run only after
            if(cfg.Action != EnumCacheAction.Set)
            {
                disableMainMethod = false;
                disableAfterExecution = false;
                return retval;
            }

            // get cached value
            var cachedRetval = GetCachedValue(provider, methodInfo, cfg, args);

            // do not run method if value is cached
            disableMainMethod = cachedRetval != null;
            disableAfterExecution = disableMainMethod;

            // return
            return cachedRetval ?? retval;
        }

        /// <summary>
        /// Configure cache 
        /// </summary>
        public void Configure()
        {

            // do nothing if setup does not exist
            if(_setup == null)
            {
                return;
            }

            // create plans 
            var plans = CacheSetupUtil.SetupToPlan(_setup);

            // import plans
            ImportPlans(plans);

            // remove setup
            _setup = null;
        }

        /// <summary>
        /// Get key 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetFullKey(string key)
        {
            var prefix = _cachePlans.Count() > 0 ? _cachePlans.First().Value.KeyPrefix : null;
            return prefix != null ? prefix + key : null;
        }

        /// <summary>
        /// Import plans
        /// </summary>
        /// <param name="cachePlans"></param>
        private void ImportPlans(List<MethodCachePlan> cachePlans)
        {
            // set plans
            _cachePlans = cachePlans.ToDictionary(
                cfg => string.Format("{0}.{1}", cfg.MethodInfo.ReflectedType.FullName, cfg.MethodInfo.ToString()),
                cfg => cfg);
        }

        /// <summary>
        /// Get method cache plan
        /// </summary>
        /// <param name="service"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private MethodCachePlan GetCachePlanForMethod(object service, MethodInfo methodInfo)
        {
            var id = string.Format("{0}.{1}", service.GetType().FullName, methodInfo.ToString());
            return _cachePlans.ContainsKey(id) ? _cachePlans[id] : null;
        }

        /// <summary>
        /// Set cached value
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="cfg"></param>
        /// <param name="args"></param>
        /// <param name="retval"></param>
        private void SetCachedValue(IServiceProvider provider, MethodCachePlan cfg, object[] args, object retval)
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
        private void RemoveCachedValue(IServiceProvider provider, MethodCachePlan cfg, object[] args, object retval)
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
        private object GetCachedValue(IServiceProvider provider, MethodInfo methodInfo, MethodCachePlan cfg, object[] args)
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
        private string GetKey(IServiceProvider provider, MethodCachePlan cfg, object[] args, object retval)
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
        internal List<MethodCacheSetupInfo> GetCacheSetupInfos() {
            return _cachePlans.Select(p => new MethodCacheSetupInfo
            {
                Action = p.Value.Action,
                KeyPrefix = p.Value.KeyPrefix,
                KeyTpl = p.Value.KeyTpl,
                Provider = p.Value.Provider,
                TimeoutMs = p.Value.TimeoutMs,
                CacheResultFunc = p.Value.CacheResultFunc != null,
                KeyFunc = p.Value.KeyFunc != null,
                Method = p.Key,
                TimeoutMsOffsetFunc = p.Value.TimeoutMsOffsetFunc != null
            }).ToList();
        }

    }
}
