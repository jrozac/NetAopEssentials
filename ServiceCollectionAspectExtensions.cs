using Microsoft.Extensions.DependencyInjection;

namespace NetCoreAopEssentials
{

    /// <summary>
    /// Service collection extensions for caching
    /// </summary>
    public static class ServiceCollectionAspectExtensions
    {

        /// <summary>
        /// Configure aspect proxy
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static AspectConfigurationBuilder<TService, TImplementation> ConfigureAspectProxy<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            var col = new AspectConfigurationBuilder<TService, TImplementation>(services);
            return col;
        }

        /// <summary>
        /// Add scoped with aspect
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <typeparam name="TAspect"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddScopedWithAspect<TService, TImplementation, TAspect>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
            where TAspect : class, IAspect, new()
        {
            return new AspectConfigurationBuilder<TService, TImplementation>(services).
                RegisterAspect<TAspect>().AddScoped();
        }

        /// <summary>
        /// Add singleton with aspect
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <typeparam name="TAspect"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonWithAspect<TService, TImplementation, TAspect>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
            where TAspect : class, IAspect, new()
        {
            return new AspectConfigurationBuilder<TService, TImplementation>(services).
                RegisterAspect<TAspect>().AddSingleton();
        }

        /// <summary>
        /// Adds transient with aspect
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <typeparam name="TAspect"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransientWithAspect<TService, TImplementation, TAspect>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
            where TAspect : class, IAspect, new()
        {
            return new AspectConfigurationBuilder<TService, TImplementation>(services).
                RegisterAspect<TAspect>().AddTransient();
        }

    }
}
