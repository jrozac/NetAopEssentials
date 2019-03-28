using Microsoft.Extensions.DependencyInjection;
using System;

namespace NetAopEssentials.Cache
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
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddScopedCacheable<TService,TImplementation>(this IServiceCollection collection, 
            Action<CacheSetup<TImplementation>> setupAction = null)
            where TService : class
            where TImplementation : class, TService
        {
            return RegisterCacheAspect<TService, TImplementation>(collection, setupAction).AddScoped();
        }

        /// <summary>
        /// Add scoped cacheable 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="collection"></param>
        /// <param name="timeout"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IServiceCollection AddScopedCacheable<TService, TImplementation>(this IServiceCollection collection,
            int timeout, EnumCacheProvider provider = EnumCacheProvider.Memory)
            where TService : class
            where TImplementation : class, TService
        {
            return RegisterCacheAspect<TService, TImplementation>(collection, null, provider, timeout).AddScoped();
        }

        /// <summary>
        /// Add transient cached 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="collection"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransientCacheable<TService, TImplementation>(this IServiceCollection collection,
            Action<CacheSetup<TImplementation>> setupAction = null)
            where TService : class
            where TImplementation : class, TService
        {
            return RegisterCacheAspect<TService, TImplementation>(collection, setupAction).AddTransient();
        }

        /// <summary>
        /// Add transient cacheable 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="collection"></param>
        /// <param name="timeout"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransientCacheable<TService, TImplementation>(this IServiceCollection collection,
            int timeout, EnumCacheProvider provider = EnumCacheProvider.Memory)
            where TService : class
            where TImplementation : class, TService
        {
            return RegisterCacheAspect<TService, TImplementation>(collection, null, provider, timeout).AddTransient();
        }

        /// <summary>
        /// Add singleton cached 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="collection"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonCacheable<TService, TImplementation>(this IServiceCollection collection,
            Action<CacheSetup<TImplementation>> setupAction = null)
            where TService : class
            where TImplementation : class, TService
        {
            return RegisterCacheAspect<TService, TImplementation>(collection, setupAction).AddSingleton();
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
        public static IServiceCollection AddSingletonCacheable<TService, TImplementation>(this IServiceCollection collection,
            int timeout, EnumCacheProvider provider = EnumCacheProvider.Memory)
            where TService : class
            where TImplementation : class, TService
        {
            return RegisterCacheAspect<TService, TImplementation>(collection, null, provider, timeout).AddSingleton();
        }

        /// <summary>
        /// Create and register aspect 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="service"></param>
        /// <param name="setupAction"></param>
        /// <param name="provider"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private static AspectConfigurationBuilder<TService,TImplementation> RegisterCacheAspect<TService, TImplementation>(
            IServiceCollection service, Action<CacheSetup<TImplementation>> setupAction = null, EnumCacheProvider? provider = null, long? timeout = null)
            where TService : class
            where TImplementation : class, TService
        {

            // create setup 
            var setup = new CacheSetup<TImplementation>();
            setupAction?.Invoke(setup);

            // use attributes if setup action not defined
            if(setupAction == null)
            {
                setup.ImportAttributesConfiguration();
                if(provider.HasValue)
                {
                    setup.CacheDefaultProvider(provider.Value);
                }
                if(timeout.HasValue)
                {
                    setup.CacheDefaultTimeout(timeout.Value);
                }
            }

            // create aspect 
            var aspeect = setup.BuildAspect();

            // register aspect 
            var builder = service.ConfigureAspectProxy<TService, TImplementation>().RegisterAspect(() => aspeect);
            return builder;
        }
    }
}
