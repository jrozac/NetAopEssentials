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
        /// Aspects container
        /// </summary>
        private readonly AspectsContainer _aspectsContainer;

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="services"></param>
        internal AspectConfigurationBuilder(IServiceCollection services)
        {
            _services = services;
            _aspectsContainer = new AspectsContainer();
        }

        /// <summary>
        /// Register aspect
        /// </summary>
        /// <typeparam name="TAspect"></typeparam>
        /// <param name="customCreate"></param>
        /// <returns></returns>
        public AspectConfigurationBuilder<TService, TImplementation> RegisterAspect<TAspect>(Func<TAspect> customCreate = null)
            where TAspect : class, IAspect<TImplementation>
        {
            _aspectsContainer.Configure<TService, TImplementation, TAspect>(customCreate);
            return this;
        }

        /// <summary>
        /// Add scoped service 
        /// </summary>
        /// <returns></returns>
        public IServiceCollection AddScoped()
        {
            _services.AddSingleton(_aspectsContainer);
            _services.AddScoped<TImplementation, TImplementation>();
            _services.AddScoped((provider) => AspectProxy.Create<TService,TImplementation>(provider));
            return _services;
        }

        /// <summary>
        /// Add transient
        /// </summary>
        /// <returns></returns>
        public IServiceCollection AddTransient()
        {
            _services.AddSingleton<AspectsContainer>(_aspectsContainer);
            _services.AddTransient<TImplementation, TImplementation>();
            _services.AddTransient((provider) => AspectProxy.Create<TService,TImplementation>(provider));
            return _services;
        }

        /// <summary>
        /// Add singleton
        /// </summary>
        /// <returns></returns>
        public IServiceCollection AddSingleton()
        {
            _services.AddSingleton<AspectsContainer>(_aspectsContainer);
            _services.AddSingleton<TImplementation, TImplementation>();
            _services.AddSingleton((provider) => AspectProxy.Create<TService,TImplementation>(provider));
            return _services;
        }

    }
}
