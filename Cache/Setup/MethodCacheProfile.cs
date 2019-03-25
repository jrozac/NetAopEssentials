using NetCoreAopEssentials.Cache.Models;
using System;
using System.Reflection;

namespace NetCoreAopEssentials.Cache.Setup
{

    /// <summary>
    /// Method cache configuration
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    /// <typeparam name="TRet"></typeparam>
    internal class MethodCacheProfile<TImplementation>
        where TImplementation : class
    {
        /// <summary>
        /// Cache method 
        /// </summary>
        public MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// Key template 
        /// </summary>
        public string KeyTpl { get; set; }

        /// <summary>
        /// Timeout ms
        /// </summary>
        public long Timeout { get; set; }

        /// <summary>
        /// Group id
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// Cache provider
        /// </summary>
        public EnumCacheProvider? Provider { get; set; }

        /// <summary>
        /// Cache action
        /// </summary>
        public EnumCacheAction Action { get; set; }
        
        /// <summary>
        /// Function to determinate whether value should be cached or not.
        /// </summary>
        public Func<object, bool> CacheResultFunc { get; set; }
        
        /// <summary>
        /// Function which returns custom timeout for cache value.
        /// </summary>
        public Func<object, long> TimeoutFunc { get; set; }

    }
}
