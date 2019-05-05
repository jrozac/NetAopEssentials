using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetAopEssentials;
using NetAopEssentials.Cache;
using NetAopEssentialsTest.Aspects;
using NetAopEssentialsTest.Models;
using NetAopEssentialsTest.Services;

namespace NetAopEssentialsTest
{
    /// <summary>
    /// Tests that documentation (readme.md) configuration is valid
    /// </summary>
    [TestClass]
    public class DocsConfigurationTest
    {

        /// <summary>
        /// Test custom aspect register
        /// </summary>
        [TestMethod]
        public void TestCustomAspectRegister()
        {

            IServiceCollection services = new ServiceCollection();
            services.ConfigureAspectProxy<IUserService, UserService>().
                RegisterAspect<AspectOne<IUserService, UserService>>().AddScoped();
            services.BuildServiceProvider();
        }

        /// <summary>
        /// Test cache aspect register
        /// </summary>
        [TestMethod]
        public void TestCacheAspectProxyRegister()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddMemoryCache();
            services.ConfigureAspectProxy<IUserService, UserService>().
                RegisterAspect<CacheAspect<IUserService, UserService>>().AddScoped();
            services.BuildServiceProvider();
        }

        /// <summary>
        /// Test cache aspect custom configuration register
        /// </summary>
        [TestMethod]
        public void TestCacheAspectProxyCustomConfigurationRegister()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddScopedCached<IUserService, UserService>((set) => set.
                SetFor(m => m.GetUser(0), "user-{id}").Configure().
                RemoveFor(m => m.UpdateUser(new User()), "user-{user.Id}").Configure().
                RemoveFor(m => m.DeleteByName("user1"), "user-{_ret.Id}").Configure().
                CacheDefaultProvider(EnumCacheProvider.Memory).
                CacheDefaultTimeout(CacheTimeout.Minute).ImportAttributesSetup());
            services.BuildServiceProvider();
        }

    }
}
