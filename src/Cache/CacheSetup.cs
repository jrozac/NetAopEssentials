using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NetAopEssentials.Cache
{

    /// <summary>
    /// Cache setup
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    public class CacheSetup<TImplementation>
        where TImplementation : class
    {

        /// <summary>
        /// Methods cache setups
        /// </summary>
        internal List<MethodCacheSetup<TImplementation>> MethodsCacheSetups { get; private set; }

        /// <summary>
        /// Defaults
        /// </summary>
        internal CacheSetupDefaults Defaults { get; private set; }

        /// <summary>
        /// Constructor 
        /// </summary>
        internal CacheSetup()
        {
            MethodsCacheSetups = new List<MethodCacheSetup<TImplementation>>();
            Defaults = new CacheSetupDefaults();
        }

        /// <summary>
        /// Import
        /// </summary>
        /// <returns></returns>
        public CacheSetup<TImplementation> ImportAttributesSetup()
        {
            Defaults.ReadAttributes = true;
            return this;
        }

        /// <summary>
        /// Use key custom prefixy
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public CacheSetup<TImplementation> UseKeyCustomPrefix(string prefix)
        {
            Defaults.KeyCustomPrefix = prefix;
            return this;
        }

        /// <summary>
        /// Cache default provider
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public CacheSetup<TImplementation> CacheDefaultProvider(EnumCacheProvider provider)
        {
            Defaults.DefaultProvider = provider;
            return this;
        }

        /// <summary>
        /// Default timeout in ms
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public CacheSetup<TImplementation> CacheDefaultTimeout(long timeout)
        {
            Defaults.DefaultTimeout = timeout;
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
        /// <param name="provider"></param>
        /// <param name="cacheResultFunc">Function to determinate whether the cache should be deleted or not.</param>
        /// <param name="timeoutFunc">Function to determinate additional timeout time. Incoming parameters are function results.</param>
        /// <returns></returns>
        public CacheSetup<TImplementation> SetFor<TRet>(Expression<Func<TImplementation, TRet>> methodExpr,
            string keyTpl, long? timeout = null, EnumCacheProvider? provider = null,
            Func<TRet, bool> cacheResultFunc = null, Func<TRet, long> timeoutFunc = null)
        {

            // set setup
            var setup = new MethodCacheSetup<TImplementation>
            {
                MethodInfo = GeneralUtil.GetMethodInfo(methodExpr),
                KeyTpl = keyTpl,
                Timeout = timeout,
                Provider = provider,
                Action = EnumCacheAction.Set
            };
            if (cacheResultFunc != null)
            {
                setup.CacheResultFunc = (r) => cacheResultFunc((TRet)r);
            }
            if (timeoutFunc != null)
            {
                setup.TimeoutFunc = (r) => timeoutFunc((TRet)r);
            }

            // save setup and return
            MethodsCacheSetups.Add(setup);
            return this;
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
        /// <param name="provider"></param>
        /// <param name="cacheResultFunc">Function to determinate whether the cache should be deleted or not.</param>
        /// <returns></returns>
        public CacheSetup<TImplementation> RemoveFor<TRet>(Expression<Func<TImplementation, TRet>> methodExpr,
            string keyTpl, EnumCacheProvider? provider = null, Func<TRet, bool> cacheResultFunc = null)
        {

            // set setup
            var setup = new MethodCacheSetup<TImplementation>
            {
                MethodInfo = GeneralUtil.GetMethodInfo(methodExpr),
                KeyTpl = keyTpl,
                Provider = provider,
                Action = EnumCacheAction.Remove
            };
            if (cacheResultFunc != null)
            {
                setup.CacheResultFunc = (r) => cacheResultFunc((TRet)r);
            }

            // save setup and return
            MethodsCacheSetups.Add(setup);
            return this;
        }

    }
}