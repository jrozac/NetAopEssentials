using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetCoreAopEssentialsTest.Services;
using NetCoreAopEssentials.Cache;
using NetCoreAopEssentials.Cache.Models;
using System.Threading.Tasks;

namespace NetCoreAopEssentialsTest
{

    /// <summary>
    /// Tests to verify dynamic conditions application
    /// </summary>
    [TestClass]
    public class CacheDynamicConditionsTest
    {

        /// <summary>
        /// Test that data are cached only if provided condition is valid
        /// </summary>
        [TestMethod]
        public void TestConditionalCache()
        {

            // setup conditional caching where only user with name user1 is cached
            UserService.MethodRunCountReset();
            IServiceCollection collection = new ServiceCollection();
            collection.AddMemoryCache();
            collection.AddScopedCacheable<IUserService, UserService>(s => s.
                SetFor(m => m.GetUser(0), "user-{id}", 0, null, (user) => user.Name == "User1")
            );
            var provider = collection.BuildServiceProvider();
            var svc = provider.GetService<IUserService>();

            // check that user 1 (configured to be cached) is cached
            var user1 = svc.GetUser(1);
            Assert.AreEqual("User1", user1.Name);
            var user1c = svc.GetUser(1);
            Assert.AreEqual(user1.RandomToken, user1c.RandomToken);

            // check that user 2 (configured not to be cached) is not cached
            var user2 = svc.GetUser(2);
            Assert.AreNotEqual("User1", user2.Name);
            var user2c = svc.GetUser(2);
            Assert.AreNotEqual(user2.RandomToken, user2c.RandomToken);
        }

        /// <summary>
        /// Test that conditional timout is considered
        /// </summary>
        [TestMethod]
        public void TestConditionalTimeout()
        {

            // setup with conditional timeout of 1 secod for user id 1, 2 secods for user id 2
            UserService.MethodRunCountReset();
            IServiceCollection collection = new ServiceCollection();
            collection.AddMemoryCache();
            collection.AddScopedCacheable<IUserService, UserService>(s => s.
                SetFor(m => m.GetUser(0), "user-{id}", 0, null, null, (user) => user.TimeouMs).
                CacheDefaultTimeout(1)
            );
            var provider = collection.BuildServiceProvider();
            var svc = provider.GetService<IUserService>();

            // get users 
            var user1 = svc.GetUser(1);
            var user2 = svc.GetUser(2);
            Assert.AreEqual(1000, user1.TimeouMs);
            Assert.AreEqual(2000, user2.TimeouMs);

            // check if cached 
            var user1c = svc.GetUser(1);
            var user2c = svc.GetUser(2);
            Assert.AreEqual(user1.RandomToken, user1c.RandomToken);
            Assert.AreEqual(user2.RandomToken, user2c.RandomToken);

            // wait one second - user 1 should not be cached, but user 2 should
            Task.Delay(1010).GetAwaiter().GetResult();
            user1c = svc.GetUser(1);
            user2c = svc.GetUser(2);
            Assert.AreNotEqual(user1.RandomToken, user1c.RandomToken);
            Assert.AreEqual(user2.RandomToken, user2c.RandomToken);

            // wait one second - nothing should be cached
            user1 = user1c;
            Task.Delay(1010).GetAwaiter().GetResult();
            user1c = svc.GetUser(1);
            user2c = svc.GetUser(2);
            Assert.AreNotEqual(user1.RandomToken, user1c.RandomToken);
            Assert.AreNotEqual(user2.RandomToken, user2c.RandomToken);
        }

    }
}
