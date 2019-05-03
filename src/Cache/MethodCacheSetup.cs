using System;
using System.Reflection;

namespace NetAopEssentials.Cache
{

    /// <summary>
    /// Method cache setup
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    /// <typeparam name="TRet"></typeparam>
    public class MethodCacheSetup<TImplementation>
        where TImplementation : class
    {

        /// <summary>
        /// Prevent external creation with internal constructor
        /// </summary>
        internal MethodCacheSetup()
        {
        }

        /// <summary>
        /// Cache method 
        /// </summary>
        internal MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// Key template 
        /// </summary>
        internal string KeyTpl { get; set; }

        /// <summary>
        /// Timeout ms
        /// </summary>
        internal long? Timeout { get; set; }

        /// <summary>
        /// Cache provider
        /// </summary>
        internal EnumCacheProvider? Provider { get; set; }

        /// <summary>
        /// Cache action
        /// </summary>
        internal EnumCacheAction Action { get; set; }

        /// <summary>
        /// Function to determinate whether value should be cached or not.
        /// </summary>
        internal Func<object, bool> CacheResultFunc { get; set; }
        
        /// <summary>
        /// Function which returns custom timeout for cache value.
        /// </summary>
        internal Func<object, long> TimeoutFunc { get; set; }

    }
}
