using Microsoft.Extensions.DependencyInjection;

namespace NetCoreAopEssentials.Cache
{

    /// <summary>
    /// Cache service collection extensions
    /// </summary>
    public static class ServiceCollectionCacheExtensions
    {

        /// <summary>
        /// Add scoped cached 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="collection"></param>
        /// <param name="timeout"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IServiceCollection AddScopedCacheable<TService,TImplementation>(this IServiceCollection collection, int timeout, EnumCacheProvider provider = EnumCacheProvider.Memory)
            where TService : class
            where TImplementation : class, TService
        {
            return RegisterCacheProxy<TService, TImplementation>(collection, timeout, provider).AddScoped();
        }

        /// <summary>
        /// Add singleton cacheable
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="collection"></param>
        /// <param name="timeout"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonCacheable<TService, TImplementation>(this IServiceCollection collection, int timeout, EnumCacheProvider provider = EnumCacheProvider.Memory)
            where TService : class
            where TImplementation : class, TService
        {
            return RegisterCacheProxy<TService, TImplementation>(collection, timeout, provider).AddSingleton();
        }

        /// <summary>
        /// Adds transient cached 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="collection"></param>
        /// <param name="timeout"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransientCacheable<TService, TImplementation>(this IServiceCollection collection, int timeout, EnumCacheProvider provider = EnumCacheProvider.Memory)
            where TService : class
            where TImplementation : class, TService
        {
            return RegisterCacheProxy<TService, TImplementation>(collection, timeout, provider).AddTransient();
        }

        /// <summary>
        /// Register cache proxy
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="collection"></param>
        /// <param name="timeout"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        private static AspectConfigurationBuilder<TService,TImplementation> RegisterCacheProxy<TService, TImplementation>(this IServiceCollection collection, int timeout, EnumCacheProvider provider = EnumCacheProvider.Memory)
            where TService : class
            where TImplementation : class, TService
        {
            return collection.ConfigureAspectProxy<TService, TImplementation>().
                EnableMethodsCache(timeout, provider, true).BuildCacheAspect();
        }
    }
}
