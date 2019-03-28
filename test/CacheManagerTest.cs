using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetAopEssentialsTest.Services;
using NetAopEssentials.Cache;
using System;
using System.Linq;
using NetAopEssentialsTest.Models;

namespace NetAopEssentialsTest
{

    /// <summary>
    /// Cache manager test
    /// </summary>
    [TestClass]
    public class CacheManagerTest
    {

        /// <summary>
        /// Basic functionality test
        /// </summary>
        [TestMethod]
        public void TestCacheManagerBasics()
        {

            // setup
            var provider = GetNewProvider();
            var manager = provider.GetCacheManager<UserService>();
            var svc = provider.GetRequiredService<IUserService>();

            // check that setup is valid for key
            Assert.AreEqual(1, manager.GetConfigurations().Count());
            Assert.AreEqual("user-{id}", manager.GetConfigurations().First().KeyTpl);

            // check cache is available 
            UserService.MethodRunCountReset();
            var user1 = svc.GetUser(1);
            var user1c = svc.GetUser(1);
            Assert.AreEqual(user1.RandomToken, user1c.RandomToken); // cached
            var user1m = manager.GetCache<User>("user-1");
            Assert.IsNotNull(user1m);
            Assert.AreEqual(user1.RandomToken, user1m.RandomToken);

            // try to remove cache
            manager.RemoveCache("user-1");
            user1m = manager.GetCache<User>("user-1");
            Assert.IsNull(user1m);
            user1c = svc.GetUser(1);
            Assert.AreNotEqual(user1.RandomToken, user1c.RandomToken);
        }

        /// <summary>
        /// Create new provider with default test setup
        /// </summary>
        /// <returns></returns>
        private IServiceProvider GetNewProvider()
        {
            // setup bad attrbiutes service manually to cache only get user method
            UserService.MethodRunCountReset();
            IServiceCollection collection = new ServiceCollection();
            collection.AddMemoryCache();
            collection.AddScopedCached<IUserService, UserService>(s => {
                s.SetFor(m => m.GetUser(0), "user-{id}");
            });
            var provider = collection.BuildServiceProvider();
            return provider;
        }
    }
}
