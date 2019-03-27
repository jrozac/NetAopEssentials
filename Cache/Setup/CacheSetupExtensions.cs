using System.Linq;

namespace NetAopEssentials.Cache.Setup
{

    /// <summary>
    /// Cache setup extensions
    /// </summary>
    public static class CacheSetupExtensions
    {

        /// <summary>
        /// Build aspect from setup 
        /// </summary>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="setup"></param>
        /// <returns></returns>
        public static CacheAspect<TImplementation> BuildAspect<TImplementation>(this CacheSetup<TImplementation> setup)
            where TImplementation : class
        {

            // set configurations 
            var configurations = setup.MethodsCacheProfiles.Select(profile =>
                CacheSetupUtil.CreateCacheConfiguration(profile, setup.Defaults)).ToList();

            // import attributes configurations
            if (setup.Defaults.ReadAttributes)
            {
                var attrProfiles = CacheSetupUtil.GetAttributesProfiles<TImplementation>();
                var attrCfgs = attrProfiles.Select(profile => CacheSetupUtil.CreateCacheConfiguration(profile, setup.Defaults)).ToList();
                configurations.AddRange(attrCfgs);
            }

            // create aspect and return
            var aspect = new CacheAspect<TImplementation>(configurations);
            return aspect;

        }

    }
}
