using NetCoreAopEssentials.Cache.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NetCoreAopEssentials.Cache.Setup
{
    public class CacheSetup<TImplementation>
        where TImplementation : class
    {

        /// <summary>
        /// Methods cache configurations
        /// </summary>
        internal List<MethodCacheProfile<TImplementation>> MethodsCacheProfiles { get; private set; }

        /// <summary>
        /// Defaults
        /// </summary>
        internal CacheSetupDefaults Defaults { get; private set; }

        /// <summary>
        /// Constructor 
        /// </summary>
        internal CacheSetup()
        {
            MethodsCacheProfiles = new List<MethodCacheProfile<TImplementation>>();
            Defaults = new CacheSetupDefaults();
        }

        /// <summary>
        /// Import
        /// </summary>
        /// <returns></returns>
        public CacheSetup<TImplementation> ImportAttributesConfiguration()
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
            if (timeout <= 0)
            {
                throw new ArgumentException("Timeout has to be grater than zero.");
            }
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
        /// <param name="groupId"></param>
        /// <param name="provider"></param>
        /// <param name="cacheResultFunc">Function to determinate whether the cache should be deleted or not.</param>
        /// <param name="timeoutFunc">Function to determinate additional timeout time. Incoming parameters are function results.</param>
        /// <returns></returns>
        public CacheSetup<TImplementation> SetFor<TRet>(Expression<Func<TImplementation, TRet>> methodExpr,
            string keyTpl, long timeout = 0, string groupId = null, EnumCacheProvider? provider = null,
            Func<TRet, bool> cacheResultFunc = null, Func<TRet, long> timeoutFunc = null)
        {

            // set configuration
            var cfg = new MethodCacheProfile<TImplementation>
            {
                MethodInfo = GeneralUtil.GetMethodInfo(methodExpr),
                KeyTpl = keyTpl,
                Timeout = timeout,
                GroupId = groupId,
                Provider = provider,
                Action = EnumCacheAction.Set
            };
            if (cacheResultFunc != null)
            {
                cfg.CacheResultFunc = (r) => cacheResultFunc((TRet)r);
            }
            if (timeoutFunc != null)
            {
                cfg.TimeoutFunc = (r) => timeoutFunc((TRet)r);
            }

            // save profile and return
            MethodsCacheProfiles.Add(cfg);
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
        /// <param name="groupId"></param>
        /// <param name="provider"></param>
        /// <param name="cacheResultFunc">Function to determinate whether the cache should be deleted or not.</param>
        /// <returns></returns>
        public CacheSetup<TImplementation> RemoveFor<TRet>(Expression<Func<TImplementation, TRet>> methodExpr,
            string keyTpl, string groupId = null, EnumCacheProvider? provider = null, Func<TRet, bool> cacheResultFunc = null)
        {

            // set configuration
            var cfg = new MethodCacheProfile<TImplementation>
            {
                MethodInfo = GeneralUtil.GetMethodInfo(methodExpr),
                KeyTpl = keyTpl,
                GroupId = groupId,
                Provider = provider,
                Action = EnumCacheAction.Remove
            };
            if (cacheResultFunc != null)
            {
                cfg.CacheResultFunc = (r) => cacheResultFunc((TRet)r);
            }

            // save profile and return
            MethodsCacheProfiles.Add(cfg);
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
        /// <param name="groupId"></param>
        /// <param name="provider"></param>
        /// <param name="cacheResultFunc">Function to determinate whether the cache should be deleted or not.</param>
        /// <returns></returns>
        public CacheSetup<TImplementation> RemoveFor(Expression<Action<TImplementation>> methodExpr,
            string keyTpl, string groupId = null, EnumCacheProvider? provider = null)
        {

            // set configuration
            var cfg = new MethodCacheProfile<TImplementation>
            {
                MethodInfo = GeneralUtil.GetMethodInfo(methodExpr),
                KeyTpl = keyTpl,
                GroupId = groupId,
                Provider = provider,
                Action = EnumCacheAction.Remove
            };

            // save profile and return
            MethodsCacheProfiles.Add(cfg);
            return this;
        }

    }
}