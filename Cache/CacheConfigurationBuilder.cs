using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NetCoreAopEssentials.Cache
{

    /// <summary>
    /// Cache configuration builder
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    public class CacheConfigurationBuilder<TService, TImplementation>
        where TService : class
        where TImplementation : class, TService
    {
           
        /// <summary>
        /// Cache definitions 
        /// </summary>
        private Dictionary<string, MethodCacheConfiguration> _cacheDefinitions 
            = new Dictionary<string, MethodCacheConfiguration>();

        /// <summary>
        /// Configuration
        /// </summary>
        private readonly AspectConfigurationBuilder<TService, TImplementation> _aspectConfiguration;

        /// <summary>
        /// Default cache provider
        /// </summary>
        private readonly EnumCacheProvider _defaultProvider;

        /// <summary>
        /// Default timeout
        /// </summary>
        private readonly long _defaultTimeoutMs;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="aspectConfiguration"></param>
        /// <param name="defaultTimeoutMs"></param>
        /// <param name="defaultProvider"></param>
        internal CacheConfigurationBuilder(AspectConfigurationBuilder<TService, TImplementation> aspectConfiguration, long defaultTimeoutMs, EnumCacheProvider defaultProvider)
        {
            _aspectConfiguration = aspectConfiguration;
            _defaultProvider = defaultProvider;
            _defaultTimeoutMs = defaultTimeoutMs;
        }

        /// <summary>
        /// Register aspect
        /// </summary>
        /// <param name="keyPrefix"></param>
        /// <returns></returns>
        public AspectConfigurationBuilder<TService, TImplementation> BuildCacheAspect(string keyPrefix = null)
        {
            keyPrefix = keyPrefix ?? $"{System.Diagnostics.Process.GetCurrentProcess().ProcessName}.{typeof(TImplementation)}.";
            _aspectConfiguration.RegisterAspect(() => new CacheAspect(_cacheDefinitions, keyPrefix));
            _cacheDefinitions = new Dictionary<string, MethodCacheConfiguration>();
            return _aspectConfiguration;
        }

        /// <summary>
        /// Defines method to delete cache if method is executed.
        /// </summary>
        /// <typeparam name="TRet"></typeparam>
        /// <param name="methodExpr">Method definition</param>
        /// <param name="keyTpl">
        ///     Cache key template. Return value or retrun value properties or method properties can be used inside curly brackets.
        ///     E.g.:
        ///         Method: bool SaveUser(int id)
        ///         Key: User-{id}
        /// </param>
        /// <param name="groupId"></param>
        /// <param name="provider"></param>
        /// <param name="cacheResultFunc">Function to determinate whether the cache should be deleted or not.</param>
        /// <returns></returns>
        public CacheConfigurationBuilder<TService,TImplementation> RegisterCacheRemoveMethod<TRet>(Expression<Func<TImplementation, TRet>> methodExpr, 
            string keyTpl, string groupId = null, EnumCacheProvider? provider = null,  Func<TRet, bool> cacheResultFunc = null)
        {

            // set configuration
            var methodInfo = GetMethodInfo(methodExpr);
            var cfg = CreateCacheConfiguration(methodInfo, EnumCacheAction.Remove, keyTpl, 0, groupId, provider, cacheResultFunc);

            // add to configuration 
            SetConfiguration(methodInfo, cfg);

            // return 
            return this;
        }

        /// <summary>
        /// Defines method caching
        /// </summary>
        /// <typeparam name="TRet"></typeparam>
        /// <param name="methodExpr">Method definition</param>
        /// <param name="keyTpl">
        ///     Cache key template. Method properties can be used inside curly brackets.
        ///     E.g.:
        ///         Method: bool SaveUser(int id)
        ///         Key: User-{id}
        /// </param>
        /// <param name="timeout">Timeout</param>
        /// <param name="groupId"></param>
        /// <param name="provider"></param>
        /// <param name="cacheResultFunc">Function to determinate whether the cache should be deleted or not.</param>
        /// <param name="timeoutFunc">Function to determinate additional timeout time. Incoming parameters are function results.</param>
        /// <returns></returns>
        public CacheConfigurationBuilder<TService, TImplementation> RegisterCacheableMethod<TRet>(Expression<Func<TImplementation, TRet>> methodExpr, string keyTpl,
            long timeout = 0, string groupId = null, EnumCacheProvider? provider = null, Func<TRet, bool> cacheResultFunc = null, Func<TRet, long> timeoutFunc = null)
        {

            // set cache definition
            var methodInfo = GetMethodInfo(methodExpr);
            var cfg = CreateCacheConfiguration(methodInfo, EnumCacheAction.Set, keyTpl, timeout, groupId, provider, cacheResultFunc, timeoutFunc);

            // add to configuration 
            SetConfiguration(methodInfo, cfg);

            // return 
            return this;
        }

        /// <summary>
        /// Imports attributes configuration 
        /// </summary>
        public void ImportAttributesConfiguration()
        {
            // get methods to cache
            var mehtodsToSet = typeof(TImplementation).GetMethods().Where(m =>
                m.GetCustomAttributes().FirstOrDefault(a => a.GetType() == typeof(CacheableAttribute)) != null).ToList();

            // get methods to cache
            var mehtodsToRemove = typeof(TImplementation).GetMethods().Where(m =>
                m.GetCustomAttributes().FirstOrDefault(a => a.GetType() == typeof(CacheRemoveAttribute)) != null).ToList();

            // configure methods cache 
            mehtodsToSet.ForEach(m => {
                var def = (CacheableAttribute) m.GetCustomAttributes().First(a => a.GetType() == typeof(CacheableAttribute));
                var cfg = CreateCacheConfiguration(m, EnumCacheAction.Set, def.KeyTemplate, def.TimeoutMs, def.GroupId, def.Provider);
                SetConfiguration(m, cfg);
            });


            // configure methods cache 
            mehtodsToRemove.ForEach(m =>
            {
                var def = (CacheRemoveAttribute)m.GetCustomAttributes().First(a => a.GetType() == typeof(CacheRemoveAttribute));
                var cfg = CreateCacheConfiguration(m, EnumCacheAction.Remove, def.KeyTemplate, 0, def.GroupId, def.Provider);
                SetConfiguration(m, cfg);
            });

        }

        /// <summary>
        /// Gets method info
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="methodExpr"></param>
        /// <returns></returns>
        private MethodInfo GetMethodInfo<TProperty>(Expression<Func<TImplementation, TProperty>> methodExpr)
        {
            MethodCallExpression method = methodExpr.Body as MethodCallExpression;
            if (method == null)
            {
                throw new ArgumentException($"Expression {methodExpr.ToString()} is not valid");
            }

            // get method info and return
            // var methodInfo = typeof(TService).GetMethods().First(m => m.ToString() == method.Method.ToString());
            var methodInfo = method.Method;
            return methodInfo;
        }

        /// <summary>
        /// Sets configuration
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="cfg"></param>
        private void SetConfiguration(MethodInfo methodInfo, MethodCacheConfiguration cfg)
        {
            var id = string.Format("{0}.{1}", methodInfo.DeclaringType.FullName, methodInfo.ToString());
            _cacheDefinitions.Remove(id);
            _cacheDefinitions.Add(id, cfg);
        }
       
        /// <summary>
        /// Creates cache configuration
        /// </summary>
        /// <typeparam name="TRet"></typeparam>
        /// <param name="methodInfo"></param>
        /// <param name="action"></param>
        /// <param name="keyTpl"></param>
        /// <param name="timeout"></param>
        /// <param name="groupId"></param>
        /// <param name="provider"></param>
        /// <param name="cacheResultFunc"></param>
        /// <param name="timeoutFunc"></param>
        /// <returns></returns>
        private MethodCacheConfiguration CreateCacheConfiguration<TRet>(MethodInfo methodInfo, EnumCacheAction action, string keyTpl, long timeout, 
            string groupId, EnumCacheProvider? provider, Func<TRet, bool> cacheResultFunc = null, Func<TRet, long> timeoutFunc = null)
        {
            var cfg = new MethodCacheConfiguration
            {
                MethodInfo = methodInfo,
                GroupId = groupId,
                KeyTpl = keyTpl,
                Action = action,
                CacheResultFunc = (retVal) => retVal != null && (cacheResultFunc == null || cacheResultFunc((TRet)retVal)),
                TimeoutMs = timeout > 0 ? timeout : _defaultTimeoutMs,
                TimeoutMsOffsetFunc = (retVal) => timeoutFunc != null && retVal != null ? timeoutFunc((TRet)retVal) : 0,
                KeyFunc = CacheKeyUtil.CreateKeyFunc(methodInfo, action, keyTpl),
                Provider = provider ?? _defaultProvider
            };
            if (action == EnumCacheAction.Set)
            {
                CheckIfTypeIsCacheable(cfg.Provider, methodInfo);
            }
            return cfg;
        }

        /// <summary>
        /// Create cache configuration
        /// </summary>
        /// <typeparam name="TRet"></typeparam>
        /// <param name="methodInfo"></param>
        /// <param name="action"></param>
        /// <param name="keyTpl"></param>
        /// <param name="timeout"></param>
        /// <param name="groupId"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        private MethodCacheConfiguration CreateCacheConfiguration(MethodInfo methodInfo, EnumCacheAction action, 
            string keyTpl, long timeout, string groupId, EnumCacheProvider? provider)
        {
            var cfg = new MethodCacheConfiguration
            {
                MethodInfo = methodInfo,
                GroupId = groupId,
                KeyTpl = keyTpl,
                Action = action,
                TimeoutMs = timeout > 0 ? timeout : _defaultTimeoutMs,
                KeyFunc = CacheKeyUtil.CreateKeyFunc(methodInfo, action, keyTpl),
                Provider = provider ?? _defaultProvider
            };
            if(action == EnumCacheAction.Set)
            {
                CheckIfTypeIsCacheable(cfg.Provider, methodInfo);
            }
            return cfg;
        }

        /// <summary>
        /// Checks whether type is cacheable
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="type"></param>
        private void CheckIfTypeIsCacheable(EnumCacheProvider provider, MethodInfo info)
        {
            // get return type 
            var type = info.ReturnType;

            // check type is valid
            if(type == typeof(void))
            {
                throw new InvalidOperationException($"Method {info.Name} does not return and cannot be cached.");
            }

            // check if can be stored to cache 
            if(provider == EnumCacheProvider.Distributed && !CacheSerializationUtil.IsTypeSupported(type))
            {
                throw new InvalidOperationException($"Type {type} of method {info.Name} cannot be cached with provider {provider}. Classes must have SerializableAttribute.");
            }
        }
    }
}
