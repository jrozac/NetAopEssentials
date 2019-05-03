using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetAopEssentials;
using NetAopEssentialsTest.Aspects;
using NetAopEssentialsTest.Services;
using System;

namespace NetAopEssentialsTest
{

    /// <summary>
    /// Multi aspect test register
    /// </summary>
    [TestClass]
    public class BasicAspectTest
    {

        /// <summary>
        /// Test that two different aspects can be registered
        /// </summary>
        [TestMethod]
        public void TestTwoAspectsCanBeRegistered()
        {

            int before = 0;
            int after = 0;

            // register multiple aspects
            IServiceCollection collection = new ServiceCollection();
            collection.AddMemoryCache();
            collection.ConfigureAspectProxy<IUserService, UserServiceWithAttributes>().
                RegisterAspect(() => new AspectOne<IUserService, UserServiceWithAttributes>(() => { before = before + 1; }, () => { after = after + 1; })).
                RegisterAspect(() => new AspectTwo<IUserService, UserServiceWithAttributes>(() => { before = before + 2; }, () => { after = after + 2; })).
                AddScoped();
            var provider = collection.BuildServiceProvider();

            // try multiple scopes
            using(var scope = provider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetService<IUserService>();
                svc.GetUser(1);
            }
            using (var scope = provider.CreateScope())
            {
                var svc = scope.ServiceProvider.GetService<IUserService>();
                svc.GetUser(1);
            }

            // check test values were changed (apect 1 implements for 1, apsect2 for 2)
            Assert.AreEqual(2*3, before);
            Assert.AreEqual(2*3, after);

        }

        /// <summary>
        /// Tests that single aspect cannot be registered multiple times
        /// </summary>
        [TestMethod]
        public void TestSingleAspectCannotBeRegisteredTwoTimes()
        {

            // register single aspect multiple times thorws exception
            Assert.ThrowsException<ArgumentException>(() =>
            {
                IServiceCollection collection = new ServiceCollection();
                collection.AddMemoryCache();
                collection.ConfigureAspectProxy<IUserService, UserServiceWithAttributes>().
                    RegisterAspect(() => new AspectOne<IUserService, UserServiceWithAttributes>(() => { }, () => { })).
                    RegisterAspect(() => new AspectOne<IUserService, UserServiceWithAttributes>(() => { }, () => { }));
            });
        }

        /// <summary>
        /// Tests that singleton is created only once
        /// </summary>
        [TestMethod]
        public void TestSingletonIsCreatedOnlyOnce()
        {

            // register with singleton service
            IServiceCollection collection = new ServiceCollection();
            collection.AddMemoryCache();
            collection.ConfigureAspectProxy<ITestSingletonVoidService, TestSingletonVoidService>().
                RegisterAspect(() => new AspectOne<ITestSingletonVoidService, TestSingletonVoidService>(() => { }, () => { })).
                AddSingleton();
            var provider = collection.BuildServiceProvider();

            // get service and set value to random
            var val = Guid.NewGuid().ToString();
            provider.GetService<ITestSingletonVoidService>().Value = val;

            // get service inside new scope and check value is the same
            using(var scope = provider.CreateScope())
            {
                var newVal = scope.ServiceProvider.GetService<ITestSingletonVoidService>().Value;
                Assert.AreEqual(val, newVal);
            }
        }

    }
}
