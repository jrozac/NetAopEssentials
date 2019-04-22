using System;

namespace NetAopEssentials.Cache
{

    /// <summary>
    /// Cache set attribute 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CacheSetAttribute : Attribute
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyTemplate"></param>
        public CacheSetAttribute(string keyTemplate)
        {
            KeyTemplate = keyTemplate;
            TimeoutMs = null;
            Provider = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyTemplate"></param>
        /// <param name="timeoutMs"></param>
        /// <param name="provider"></param>
        public CacheSetAttribute(string keyTemplate, EnumCacheProvider provider)
        {
            KeyTemplate = keyTemplate;
            TimeoutMs = null;
            Provider =  provider;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyTemplate"></param>
        /// <param name="timeoutMs"></param>
        public CacheSetAttribute(string keyTemplate, long timeoutMs)
        {
            KeyTemplate = keyTemplate;
            TimeoutMs = timeoutMs;
            Provider = null;
        }

        /// <summary>
        /// 
        /// Constructor
        /// </summary>
        /// <param name="keyTemplate"></param>
        /// <param name="timeoutMs"></param>
        /// <param name="provider"></param>
        /// <param name="useCustomProvider"></param>
        public CacheSetAttribute(string keyTemplate, long timeoutMs, EnumCacheProvider provider)
        {
            KeyTemplate = keyTemplate;
            TimeoutMs = timeoutMs;
            Provider = provider;
        }

        /// <summary>
        /// Custom cache provider
        /// </summary>
        public EnumCacheProvider? Provider { get; private set; }

        /// <summary>
        /// Key template 
        /// </summary>
        public string KeyTemplate { get; private set; }

        /// <summary>
        /// Timeout 
        /// </summary>
        public long? TimeoutMs { get; private set; }
    }
}
