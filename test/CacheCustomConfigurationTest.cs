using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetAopEssentials.Cache;
using NetAopEssentialsTest.Services;

namespace NetAopEssentialsTest
{

    /// <summary>
    /// Test custom configuration
    /// </summary>
    [TestClass]
    public class CacheCustomConfigurationTest
    {

        /// <summary>
        /// Test method return is cached for manual configuration.
        /// </summary>
        [TestMethod]
        public void TestMethodReturnIsCached()
        {
            // setup bad attrbiutes service manually to cache only get user method
            UserService.MethodRunCountReset();
            IServiceCollection collection = new ServiceCollection();
            collection.AddMemoryCache();
            collection.AddScopedCacheable<IUserService, UserServiceWithBadAttributes>(s => {
                s.SetFor(m => m.GetUser(0), "user-{id}");
            });
            var provider = collection.BuildServiceProvider();
            var svc = provider.GetRequiredService<IUserService>();

            // check caching
            UserService.MethodRunCountReset();
            Assert.AreEqual(0, UserService.MethodRunCount);
            var user1 = svc.GetUser(1);
            Assert.IsNotNull(user1);
            Assert.AreEqual(1, UserService.MethodRunCount);

            // update user does not clear cache 
            bool updateStatus = svc.UpdateUser(user1);
            Assert.IsTrue(updateStatus);
            var user1c = svc.GetUser(1);
            Assert.IsNotNull(user1c);
            Assert.AreEqual(2, UserService.MethodRunCount);
            Assert.AreEqual(user1.RandomToken, user1c.RandomToken);
        }

    }
}
