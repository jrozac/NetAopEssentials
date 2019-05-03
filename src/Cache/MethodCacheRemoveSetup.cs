using System;
using System.Reflection;

namespace NetAopEssentials.Cache
{

    /// <summary>
    /// Method cache remove setup
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    /// <typeparam name="TRet"></typeparam>
    public class MethodCacheRemoveSetup<TImplementation, TRet> : MethodCacheSetup<TImplementation>
        where TImplementation : class
    {

        /// <summary>
        /// Cache setup reference 
        /// </summary>
        private CacheSetup<TImplementation> _cacheSetup;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="keyTpl"></param>
        /// <param name="cacheSetup"></param>
        internal MethodCacheRemoveSetup(MethodInfo methodInfo, string keyTpl, CacheSetup<TImplementation> cacheSetup)
        {
            MethodInfo = methodInfo;
            KeyTpl = keyTpl;
            Action = EnumCacheAction.Set;
            _cacheSetup = cacheSetup;
        }

        /// <summary>
        /// Set custom cache provider
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public MethodCacheRemoveSetup<TImplementation, TRet> SetProvider(EnumCacheProvider provider)
        {
            Provider = provider;
            return this;
        }

        /// <summary>
        /// Set cache condition function. Function to determinate whether the cache should be deleted or not.
        /// </summary>
        /// <param name="cacheResultFunc"></param>
        /// <returns></returns>
        public MethodCacheRemoveSetup<TImplementation, TRet> SetCacheConditionFunction(Func<TRet, bool> cacheResultFunc)
        {
            CacheResultFunc = (r) => cacheResultFunc((TRet)r);
            return this;
        }

        /// <summary>
        /// Confirm configuration
        /// </summary>
        /// <returns></returns>
        public CacheSetup<TImplementation> Configure()
        {
            var setup = _cacheSetup;
            _cacheSetup = null;
            setup.MethodsCacheSetups.Add(this);
            return setup;
        }

    }
}