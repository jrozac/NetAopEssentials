using Microsoft.Extensions.DependencyInjection;
using System;

namespace NetCoreAopEssentials
{

    /// <summary>
    /// Aspect configuration collection. Used to define aspects.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    public class AspectConfigurationBuilder<TService, TImplementation>
        where TService : class
        where TImplementation : class, TService
    {

        /// <summary>
        /// Services collection
        /// </summary>
        private readonly IServiceCollection _services;

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="services"></param>
        internal AspectConfigurationBuilder(IServiceCollection services)
        {
            _services = services;
        }

        /// <summary>
        /// Register aspect
        /// </summary>
        /// <typeparam name="TAspect"></typeparam>
        /// <param name="customCreate"></param>
        /// <returns></returns>
        public AspectConfigurationBuilder<TService, TImplementation> RegisterAspect<TAspect>(Func<TAspect> customCreate = null)
            where TAspect : class, IAspect
        {
            AspectProxy<TService>.Configure<TImplementation, TAspect>(customCreate);
            return this;
        }

        /// <summary>
        /// Add scoped service 
        /// </summary>
        /// <returns></returns>
        public IServiceCollection AddScoped()
        {
            _services.AddScoped<TImplementation, TImplementation>();
            _services.AddScoped((provider) => AspectProxy<TService>.Create<TImplementation>(provider));
            return _services;
        }

        /// <summary>
        /// Add transient
        /// </summary>
        /// <returns></returns>
        public IServiceCollection AddTransient()
        {
            _services.AddTransient<TImplementation, TImplementation>();
            _services.AddTransient((provider) => AspectProxy<TService>.Create<TImplementation>(provider));
            return _services;
        }

        /// <summary>
        /// Add singleton
        /// </summary>
        /// <returns></returns>
        public IServiceCollection AddSingleton()
        {
            _services.AddSingleton<TImplementation, TImplementation>();
            _services.AddSingleton((provider) => AspectProxy<TService>.Create<TImplementation>(provider));
            return _services;
        }

    }
}
