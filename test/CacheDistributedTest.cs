using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetAopEssentials.Cache;
using NetAopEssentialsTest.Services;
using System;

namespace NetAopEssentialsTest
{

    /// <summary>
    /// Distributed cache test 
    /// </summary>
    [TestClass]
    public class CacheDistributedTest
    {

        /// <summary>
        /// Test that non serializable object throws an exception on setup
        /// </summary>
        [TestMethod]
        public void TestNonSerializableObjectThrowsOnSetup()
        {
            try
            {
                IServiceCollection collection = new ServiceCollection();
                collection.AddDistributedMemoryCache();
                collection.AddScopedCached<IUserService, UserServiceWithBadAttributes>(s => {
                    s.SetFor(m => m.GetUser(0), "user-{id}").
                        CacheDefaultProvider(EnumCacheProvider.Distributed);
                });
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message.Contains("cannot be cached with provider Distributed"));
            }
        }


        /// <summary>
        /// Test that method return is cached 
        /// </summary>
        [TestMethod]
        public void TestMethodReturnIsCached() {

            // setup 
            IServiceCollection collection = new ServiceCollection();
            collection.AddDistributedMemoryCache();
            collection.AddScopedCached<IUserService, UserService>(s => {
                s.SetFor(m => m.GetSerializableUser(0), "userser-{id}").
                    CacheDefaultProvider(EnumCacheProvider.Distributed);
            });
            var provider = collection.BuildServiceProvider();
            var svc = provider.GetService<IUserService>();

            // check data cache
            UserService.MethodRunCountReset();
            Assert.AreEqual(0, UserService.MethodRunCount);
            var user1 = svc.GetSerializableUser(1);
            Assert.AreEqual(1, UserService.MethodRunCount);
            var user1c = svc.GetSerializableUser(1);
            Assert.AreEqual(1, UserService.MethodRunCount);
            Assert.AreEqual(user1.RandomToken, user1c.RandomToken);

        }

    }
}
