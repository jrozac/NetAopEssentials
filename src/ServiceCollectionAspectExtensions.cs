using Microsoft.Extensions.DependencyInjection;

namespace NetAopEssentials
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

    }
}
