using System;

namespace NetAopEssentials.Cache
{

    /// <summary>
    /// Cache remove attribute. Used to register method to remove cache.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CacheRemoveAttribute : Attribute
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyTemplate"></param>
        /// <param name="provider"></param>
        public CacheRemoveAttribute(string keyTemplate, EnumCacheProvider provider)
        {
            KeyTemplate = keyTemplate;
            Provider = provider;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyTemplate"></param>
        public CacheRemoveAttribute(string keyTemplate)
        {
            KeyTemplate = keyTemplate;
            Provider = null;
        }

        /// <summary>
        /// Custom cache provider
        /// </summary>
        public EnumCacheProvider? Provider { get; private set; }

        /// <summary>
        /// Key template 
        /// </summary>
        public string KeyTemplate { get; private set; }

    }
}
