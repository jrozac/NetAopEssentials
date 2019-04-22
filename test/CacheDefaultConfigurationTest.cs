using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetAopEssentials.Cache;
using NetAopEssentialsTest.Services;
using System;

namespace NetAopEssentialsTest
{

    /// <summary>
    /// Cache Default setup tests 
    /// </summary>
    [TestClass]
    public class CacheDefaultSetupTest
    {

        /// <summary>
        /// Test that an exception is thrown if bad cach key setup is used
        /// </summary>
        [TestMethod]
        public void TestBadSetupThrows()
        {
            try
            {
                IServiceCollection collection = new ServiceCollection();
                collection.AddMemoryCache();
                collection.AddScopedCached<IUserService, UserServiceWithBadAttributes>();
                Assert.Fail();
            } catch(Exception)
            {
            }
        }

        /// <summary>
        /// Test method return is cached
        /// </summary>
        [TestMethod]
        public void TestMethodReturnIsCached()
        {

            // register 
            IServiceCollection collection = new ServiceCollection();
            collection.AddMemoryCache();
            collection.AddScopedCached<IUserService, UserServiceWithAttributes>();
            var provider = collection.BuildServiceProvider();
            var svc = provider.GetRequiredService<IUserService>();

            // check that value is cached
            UserService.MethodRunCountReset();
            Assert.AreEqual(0, UserService.MethodRunCount);
            var user1 = svc.GetUser(1);
            Assert.AreEqual(1, UserService.MethodRunCount);
            var user1c = svc.GetUser(1);
            Assert.AreEqual(1, UserService.MethodRunCount);
            Assert.AreEqual(user1.Id, user1c.Id);
            Assert.AreEqual(user1.Name, user1c.Name);
            Assert.AreEqual(user1.RandomToken, user1c.RandomToken);

            // check that cache is removed
            svc.UpdateUser(user1);
            user1c = svc.GetUser(1);
            Assert.AreEqual(3, UserService.MethodRunCount);
            Assert.AreNotEqual(user1.RandomToken, user1c.RandomToken);

            // cache again check
            user1c = svc.GetUser(1);
            Assert.AreEqual(3, UserService.MethodRunCount);

        }

        /// <summary>
        /// Test that service registration does work with provided timeout
        /// </summary>
        [TestMethod]
        public void TestRegisterWithTimeoutSet()
        {

            // build collection 
            IServiceCollection collection = new ServiceCollection();
            collection.AddMemoryCache();
            collection.AddScopedCached<IUserService, UserServiceWithAttributes>(CacheTimeout.Century);
            var provider = collection.BuildServiceProvider();

            // get service 
            UserService.MethodRunCountReset();
            var service = provider.GetService<IUserService>();

            // test cache
            var user1 = service.GetUser(1);
            var user2 = service.GetUser(1);
            Assert.AreEqual(1, UserService.MethodRunCount);
            Assert.AreEqual(user1.RandomToken, user2.RandomToken);

        }
    }
}
