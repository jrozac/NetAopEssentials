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
        public static IServiceCollection AddScopedCached<TService,TImplementation>(this IServiceCollection collection, 
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
        public static IServiceCollection AddScopedCached<TService, TImplementation>(this IServiceCollection collection,
            long timeout, EnumCacheProvider provider = EnumCacheProvider.Memory)
            where TService : class
            where TImplementation : class, TService
        {
            var action = CreateDefaultSetupAction<TService, TImplementation>(timeout, provider);
            return RegisterCacheAspect<TService, TImplementation>(collection, action).AddScoped();
        }

        /// <summary>
        /// Add transient cached 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="collection"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransientCached<TService, TImplementation>(this IServiceCollection collection,
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
        public static IServiceCollection AddTransientCached<TService, TImplementation>(this IServiceCollection collection,
            long timeout, EnumCacheProvider provider = EnumCacheProvider.Memory)
            where TService : class
            where TImplementation : class, TService
        {
            var action = CreateDefaultSetupAction<TService, TImplementation>(timeout, provider);
            return RegisterCacheAspect<TService, TImplementation>(collection, action).AddTransient();
        }

        /// <summary>
        /// Add singleton cached 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="collection"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonCached<TService, TImplementation>(this IServiceCollection collection,
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
        public static IServiceCollection AddSingletonCached<TService, TImplementation>(this IServiceCollection collection,
            long timeout, EnumCacheProvider provider = EnumCacheProvider.Memory)
            where TService : class
            where TImplementation : class, TService
        {
            var action = CreateDefaultSetupAction<TService, TImplementation>(timeout, provider);
            return RegisterCacheAspect<TService, TImplementation>(collection, action).AddSingleton();
        }

        /// <summary>
        /// Create default setup action
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="timeout"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        private static Action<CacheSetup<TImplementation>> CreateDefaultSetupAction<TService,TImplementation>(
            long timeout, EnumCacheProvider provider)
            where TService : class
            where TImplementation : class, TService
        {

            // create setup action
            var setupAction = new Action<CacheSetup<TImplementation>>((setup) => {
                setup.ImportAttributesSetup();
                setup.CacheDefaultProvider(provider);
                setup.CacheDefaultTimeout(timeout);
            });
            return setupAction;

        }

        /// <summary>
        /// Create and register aspect 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="service"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        private static AspectConfigurationBuilder<TService,TImplementation> RegisterCacheAspect<TService, TImplementation>(
            IServiceCollection service, Action<CacheSetup<TImplementation>> setupAction = null)
            where TService : class
            where TImplementation : class, TService
        {
            // create aspect
            var aspect = new CacheAspect<TImplementation>(setupAction);

            // register aspect 
            var builder = service.ConfigureAspectProxy<TService, TImplementation>().RegisterAspect(() => aspect);
            return builder;
        }
    }
}
