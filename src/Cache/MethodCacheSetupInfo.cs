namespace NetAopEssentials.Cache
{

    /// <summary>
    /// Method cache information 
    /// </summary>
    public class MethodCacheSetupInfo
    {

        /// <summary>
        /// Internal constructor disables external creation
        /// </summary>
        internal MethodCacheSetupInfo() { }

        /// <summary>
        /// Method id
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
        /// Cache result function is set
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

        /// <summary>
        /// Key definition function is set
        /// </summary>
        public bool KeyFunc { get; internal set; }

    }
}
