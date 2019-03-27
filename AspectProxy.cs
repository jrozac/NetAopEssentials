using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace NetAopEssentials
{

    /// <summary>
    /// Cache proxy 
    /// </summary>
    public class AspectProxy : DispatchProxy
    {

        /// <summary>
        /// Service 
        /// </summary>
        private object _service;

        /// <summary>
        /// Service provider  
        /// </summary>
        private IServiceProvider _serviceProvider;

        /// <summary>
        /// Aspects container
        /// </summary>
        private AspectsContainer _aspectsContainer;

        /// <summary>
        /// Invoke method 
        /// </summary>
        /// <param name="targetMethod"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            // get aspects to be exectued
            var aspects = _aspectsContainer.GetAspects(_service.GetType());

            // declare return value
            object retval = null;

            // return insted of run
            bool rt = false;

            // before execution
            for (int i=0; i < aspects.Count; i++)
            {
                retval = aspects[i].BeforeExecution(_serviceProvider, targetMethod, _service, args, out rt);
            }

            // execute function
            if(!rt)
            {
                retval = targetMethod.Invoke(_service, args);
            }

            // afer function execution
            aspects.ForEach(a => retval = a.AfterExecution(_serviceProvider, targetMethod, _service, args, retval, rt));

            // return
            return retval;

        }

        /// <summary>
        /// Create proxy instance 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="sp"></param>
        /// <returns></returns>
        public static TService Create<TService, TImplementation>(IServiceProvider sp)
            where TService : class
            where TImplementation : class,TService
        {

            // create instance 
            TService impl = ActivatorUtilities.CreateInstance<TImplementation>(sp);

            // create proxy 
            object proxy = Create<TService, AspectProxy>();
            ((AspectProxy)proxy)._service = impl;
            ((AspectProxy)proxy)._serviceProvider = sp;
            ((AspectProxy)proxy)._aspectsContainer = sp.GetRequiredService<AspectsContainer>();

            // return
            return (TService) proxy;
        }

    }
}
