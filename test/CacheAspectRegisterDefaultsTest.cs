using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetAopEssentials;
using NetAopEssentials.Cache;
using NetAopEssentialsTest.Services;
using System;

namespace NetAopEssentialsTest
{

    /// <summary>
    /// Test default cache behaviour
    /// </summary>
    [TestClass]
    public class CacheAspectRegisterDefaultsTest
    {
        /// <summary>
        /// Test attributes setup caches
        /// </summary>
        [TestMethod]
        public void TestMethodReturnIsCached()
        {

            // create setup where default setup is used (attributes define cache)
            IServiceCollection collection = new ServiceCollection();
            collection.AddMemoryCache();
            collection.ConfigureAspectProxy<IUserService, UserServiceWithAttributes>().
                RegisterAspect<CacheAspect<IUserService,UserServiceWithAttributes>>().AddScoped();
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
        /// Test attributes setup does not cache null
        /// </summary>
        [TestMethod]
        public void TestMethodReturnIsNotCachedIfNull()
        {

            // create setup where default setup is used (attributes define cache)
            UserService.MethodRunCountReset();
            IServiceCollection collection = new ServiceCollection();
            collection.AddMemoryCache();
            collection.ConfigureAspectProxy<IUserService, UserServiceWithAttributes>().
                RegisterAspect<CacheAspect<IUserService,UserServiceWithAttributes>>().AddScoped();
            var provider = collection.BuildServiceProvider();
            var svc = provider.GetRequiredService<IUserService>();

            // check that value is not cached if return is null
            Assert.AreEqual(0, UserService.MethodRunCount);
            var user3 = svc.GetUser(3);
            Assert.IsNull(user3);
            Assert.AreEqual(1, UserService.MethodRunCount);
            var user3c = svc.GetUser(3);
            Assert.IsNull(user3c);
            Assert.AreEqual(2, UserService.MethodRunCount);

        }

        /// <summary>
        /// Test attributes setup
        /// </summary>
        [TestMethod]
        public void TestBadSetupThrowsExceptionOnRegister()
        {

            // create setup where default setup is used (attributes define cache)
            IServiceCollection collection = new ServiceCollection();
            collection.AddMemoryCache();
            Assert.ThrowsException<ArgumentException>(() => {
                collection.ConfigureAspectProxy<IUserService, UserServiceWithBadAttributes>().
                    RegisterAspect<CacheAspect<IUserService, UserServiceWithBadAttributes>>().AddScoped();
            });
        }

        /// <summary>
        /// Check that invalid setup timeout throw exception
        /// </summary>
        [TestMethod]
        public void TestBadTimeoutThrowsExceptionOnRegister()
        {

            UserService.MethodRunCountReset();
            IServiceCollection collection = new ServiceCollection();
            collection.AddMemoryCache();
            Assert.ThrowsException<ArgumentException>(() =>
            {
                collection.AddScopedCached<IUserService, UserService>(s => s.
                    SetFor(m => m.GetUser(0), "user-{id}", 0, null, (user) => user.Name == "User1")
                );
            });

        }

    }
}
