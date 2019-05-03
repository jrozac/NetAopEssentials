using System;
using System.Reflection;

namespace NetAopEssentials.Cache
{

    /// <summary>
    /// Method cache setup
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    /// <typeparam name="TRet"></typeparam>
    public class MethodCacheSetSetup<TImplementation, TRet> : MethodCacheSetup<TImplementation>
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
        internal MethodCacheSetSetup(MethodInfo methodInfo, string keyTpl, CacheSetup<TImplementation> cacheSetup)
        {
            MethodInfo = methodInfo;
            KeyTpl = keyTpl;
            Action = EnumCacheAction.Set;
            _cacheSetup = cacheSetup;
        }

        /// <summary>
        /// Sets cache timeout 
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public MethodCacheSetSetup<TImplementation, TRet> SetTimeout(long timeout)
        {
            Timeout = timeout;
            return this;
        }

        /// <summary>
        /// Set cache provider
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public MethodCacheSetSetup<TImplementation, TRet> SetProvider(EnumCacheProvider provider)
        {
            Provider = provider;
            return this;
        }

        /// <summary>
        /// Set cache timeout offset function
        /// </summary>
        /// <param name="timeoutFunc"></param>
        /// <returns></returns>
        public MethodCacheSetSetup<TImplementation, TRet> SetTimeoutOffsetFunction(Func<TRet, long> timeoutFunc)
        {
            TimeoutFunc = (r) => timeoutFunc((TRet)r);
            return this;
        }

        /// <summary>
        /// Set cache condition function. Function to determinate additional timeout time. Incoming parameters are function results.
        /// </summary>
        /// <param name="cacheResultFunc"></param>
        /// <returns></returns>
        public MethodCacheSetSetup<TImplementation, TRet> SetCacheConditionFunction(Func<TRet, bool> cacheResultFunc)
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
