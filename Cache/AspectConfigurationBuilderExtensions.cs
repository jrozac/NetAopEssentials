using System;

namespace NetCoreAopEssentials.Cache
{

    /// <summary>
    /// Etensions 
    /// </summary>
    public static class AspectConfigurationBuilderExtensions
    {

        /// <summary>
        /// Configure cache proxy 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="config"></param>
        /// <param name="defaultTimeoutMs"></param>
        /// <param name="defaultProvider"></param>
        /// <param name="importAttributes"></param>
        /// <returns></returns>
        public static CacheConfigurationBuilder<TService, TImplementation> EnableMethodsCache<TService, TImplementation>(this AspectConfigurationBuilder<TService, TImplementation> config,
                long defaultTimeoutMs = 60000, EnumCacheProvider defaultProvider = EnumCacheProvider.Memory, bool importAttributesConfiguration = true)
            where TService : class
            where TImplementation : class, TService
        {

            // check timeout value
            if(defaultTimeoutMs <= 0)
            {
                throw new ArgumentException("Timeout ms value should be grater than 0.");
            }

            // create cache builder
            var builder = new CacheConfigurationBuilder<TService, TImplementation>(config, defaultTimeoutMs, defaultProvider);
            if (importAttributesConfiguration)
            {
                builder.ImportAttributesConfiguration();
            }
            return builder;
        }

        /// <summary>
        /// Register Atributes cache 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
        public static AspectConfigurationBuilder<TService, TImplementation> RegisterAtributesCache<TService, TImplementation>(this AspectConfigurationBuilder<TService, TImplementation> config, 
                long defaultTimeoutMs = 60000,  EnumCacheProvider defaultCacheProvider = EnumCacheProvider.Memory, string keyPrefix = null)
            where TService : class
            where TImplementation : class, TService
        {

            // check timeout value
            if (defaultTimeoutMs <= 0)
            {
                throw new ArgumentException("Timeout ms value should be grater than 0.");
            }

            // setup
            var builder = new CacheConfigurationBuilder<TService, TImplementation>(config, defaultTimeoutMs, defaultCacheProvider);
            builder.ImportAttributesConfiguration();
            return builder.BuildCacheAspect(keyPrefix);
        }

    }
}
