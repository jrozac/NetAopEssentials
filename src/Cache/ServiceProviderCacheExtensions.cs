using System;

namespace NetAopEssentials.Cache
{

    /// <summary>
    /// Service provider cache extensions 
    /// </summary>
    public static class ServiceProviderCacheExtensions
    {

        /// <summary>
        /// Get cache manager
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static CacheManager<TService,TImplementation> GetCacheManager<TService,TImplementation>(this IServiceProvider provider)
            where TService : class
            where TImplementation : class, TService
        {
            return new CacheManager<TService,TImplementation>(provider);
        }

    }
}
