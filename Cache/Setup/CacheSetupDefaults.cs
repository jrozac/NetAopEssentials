using NetAopEssentials.Cache.Models;

namespace NetAopEssentials.Cache.Setup
{
    /// <summary>
    /// Cache setup defaults
    /// </summary>
    internal class CacheSetupDefaults
    {

        /// <summary>
        /// Read attributes for configuration
        /// </summary>
        internal bool ReadAttributes { get; set; } = false;

        /// <summary>
        /// Cache key custom prefix. By default class fullname is used.
        /// </summary>
        internal string KeyCustomPrefix { get; set; } = null;

        /// <summary>
        /// Default provider
        /// </summary>
        internal EnumCacheProvider DefaultProvider { get; set; } = EnumCacheProvider.Memory;

        /// <summary>
        /// Timeout
        /// </summary>
        internal long DefaultTimeout { get; set; } = CacheTimeout.Minute;

    }
}
