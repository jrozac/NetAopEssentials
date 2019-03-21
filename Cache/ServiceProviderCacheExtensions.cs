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
        public static CacheManager<TService, TImplementation> GetCacheManager<TService,TImplementation>(this IServiceProvider provider)
            where TService : class
            where TImplementation : class, TService
        {
            return new CacheManager<TService, TImplementation>(provider);
        }

    }
}
