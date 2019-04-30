using NetAopEssentials;
using System;
using System.Reflection;

namespace NetAopEssentialsTest.Aspects
{

    /// <summary>
    /// Test void aspect
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    public class TestAspectVoid<TService, TImplementation> : IAspect<TService, TImplementation>
    where TService : class
    where TImplementation : class, TService
    {
        Action _beforeRun;
        Action _afterRun;

        public TestAspectVoid(Action before, Action after)
        {
            _beforeRun = before;
            _afterRun = after;
        }

        public object AfterExecution(IServiceProvider provider, MethodInfo methodInfo, object instance, object[] args, object retval, bool mainMethodDisabled, Exception mainMethodException)
        {
            _afterRun.Invoke();
            return retval;
        }

        public object BeforeExecution(IServiceProvider provider, MethodInfo methodInfo, object instance, object[] args, object retval, bool mainMethodDisabled, out bool disableMainMethod, out bool disableAfterExecution)
        {
            disableMainMethod = false;
            disableAfterExecution = false;
            _beforeRun.Invoke();
            return retval;
        }

        public void Configure()
        {
        }
    }
}
