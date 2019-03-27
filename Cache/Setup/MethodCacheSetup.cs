using NetAopEssentials.Cache.Models;

namespace NetAopEssentials.Cache.Setup
{

    /// <summary>
    /// Method cache setup
    /// </summary>
    public class MethodCacheSetup
    {

        /// <summary>
        /// Internal constructor disables external creation
        /// </summary>
        internal MethodCacheSetup() { }

        /// <summary>
        /// Method info 
        /// </summary>
        public string Method { get; internal set; }

        /// <summary>
        /// Cache key template
        /// </summary>
        public string KeyTpl { get; internal set; }

        /// <summary>
        /// Cache action
        /// </summary>
        public EnumCacheAction Action { get; internal set; }

        /// <summary>
        /// Cache provider 
        /// </summary>
        public EnumCacheProvider Provider { get; internal set; }

        /// <summary>
        /// Function whihc returns whether the result should be cached or not.
        /// </summary>
        public bool CacheResultFunc { get; internal set; }

        /// <summary>
        /// Cache timeout in milliseconds 
        /// </summary>
        public long TimeoutMs { get; internal set; }

        /// <summary>
        /// Key prefix
        /// </summary>
        public string KeyPrefix { get; internal set; }

        /// <summary>
        /// Added to timeout 
        /// </summary>
        public bool TimeoutMsOffsetFunc { get; internal set; }

    }
}
