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
        /// Cache set setup for method
        /// </summary>
        /// <typeparam name="TRet"></typeparam>
        /// <param name="methodExpr"></param>
        /// <param name="keyTpl">
        ///     Cache key template. Method properties can be used inside curly brackets.
        ///     E.g.:
        ///         Method: bool SaveUser(int id)
        ///         Key: User-{id}
        /// </param>
        /// <returns></returns>
        public MethodCacheSetSetup<TImplementation,TRet> SetFor<TRet>(Expression<Func<TImplementation,TRet>> methodExpr, string keyTpl)
        {
            var info = GeneralUtil.GetMethodInfo(methodExpr);
            var methodSetup = new MethodCacheSetSetup<TImplementation, TRet>(info, keyTpl, this);
            return methodSetup;
        }

        /// <summary>
        /// Remove cache setup
        /// </summary>
        /// <typeparam name="TRet"></typeparam>
        /// <param name="methodExpr"></param>
        /// <param name="keyTpl">
        ///     Cache key template. Method properties can be used inside curly brackets.
        ///     E.g.:
        ///         Method: bool SaveUser(int id)
        ///         Key: User-{id}
        /// </param>
        /// <returns></returns>
        public MethodCacheRemoveSetup<TImplementation, TRet> RemoveFor<TRet>(Expression<Func<TImplementation, TRet>> methodExpr, string keyTpl)
        {
            var info = GeneralUtil.GetMethodInfo(methodExpr);
            var methodSetup = new MethodCacheRemoveSetup<TImplementation, TRet>(info, keyTpl, this);
            return methodSetup;
        }
    }
}