using System;

namespace NetCoreAopEssentials.Cache
{

    /// <summary>
    /// Service provider cache extensions 
    /// </summary>
    public static class ServiceProviderCacheExtensions
    {

        /// <summary>
        /// Remove cachce
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="key"></param>
        public static CacheManager<TImplementation> GetCacheManager<TImplementation>(this IServiceProvider provider)
            where TImplementation : class
        {
            return new CacheManager<TImplementation>(provider);
        }

    }
}
