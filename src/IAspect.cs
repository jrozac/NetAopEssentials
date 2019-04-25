using System;
using System.Reflection;

namespace NetAopEssentials
{

    /// <summary>
    /// Aspect interface 
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    public interface IAspect<TImplementation> : IAspect
        where TImplementation : class
    {
        /// <summary>
        /// Configure aspect for class 
        /// </summary>
        void Configure();
    }

    /// <summary>
    /// Aspect interface 
    /// </summary>
    public interface IAspect
    {

        /// <summary>
        /// Before execution
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInfo"></param>
        /// <param name="instance"></param>
        /// <param name="args"></param>
        /// <param name="retval"></param>
        /// <param name="disableMainMethod"></param>
        /// <param name="disableAfterExecution"></param>
        /// <returns></returns>
        object BeforeExecution(IServiceProvider provider, MethodInfo methodInfo, object instance, object[] args, object retval, bool mainMethodDisabled, out bool disableMainMethod, out bool disableAfterExecution);

        /// <summary>
        /// After execution
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInfo"></param>
        /// <param name="instance"></param>
        /// <param name="args"></param>
        /// <param name="retval"></param>
        /// <param name="mainMethodDisabled"></param>
        /// <param name="mainMethodException"></param>
        /// <returns></returns>
        object AfterExecution(IServiceProvider provider, MethodInfo methodInfo, object instance, object[] args, object retval, bool mainMethodDisabled, Exception mainMethodException);

    }
}
