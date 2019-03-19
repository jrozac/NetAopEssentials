using System;
using System.Reflection;

namespace NetCoreAopEssentials
{

    /// <summary>
    /// Aspect interface 
    /// </summary>
    public interface IAspect
    {
        /// <summary>
        /// Configure aspect for class 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void ConfigureFor<T>() where T : class;

        /// <summary>
        /// Before execution
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInfo"></param>
        /// <param name="instance"></param>
        /// <param name="args"></param>
        /// <param name="returnNoRun"></param>
        /// <returns></returns>
        object BeforeExecution(IServiceProvider provider, MethodInfo methodInfo, object instance, object[] args, out bool returnNoRun);

        /// <summary>
        /// After function execution.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="methodInfo"></param>
        /// <param name="instance"></param>
        /// <param name="args"></param>
        /// <param name="retval">Method return value</param>
        /// <param name="returnNoRun"></param>
        /// <returns></returns>
        object AfterExecution(IServiceProvider provider, MethodInfo methodInfo, object instance, object[] args, object retval, bool returnNoRun);

    }
}
