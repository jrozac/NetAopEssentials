using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace NetAopEssentials
{

    /// <summary>
    /// Aspect proxy 
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

            // execution environment vars
            object retval = null;
            bool mainMethodDisabled = false;
            var runAfterMethod = aspects.Select(a => true).ToList();

            // before execution
            for (int i=0; i < aspects.Count; i++)
            {
                // run before execution 
                bool disableMainMethod = false;
                bool disableAfterExecution = false;
                retval = aspects[i].BeforeExecution(_serviceProvider, targetMethod, _service, args, retval, mainMethodDisabled, out disableMainMethod, out disableAfterExecution);

                // update environment vars
                if(disableAfterExecution)
                {
                    runAfterMethod[i] = false;
                }
                mainMethodDisabled = mainMethodDisabled || disableMainMethod;
            }

            // execute function
            Exception mainMethodException = null;
            if(!mainMethodDisabled)
            {
                try
                {
                    retval = targetMethod.Invoke(_service, args);
                } catch(Exception e)
                {
                    mainMethodException = e;
                }
            }

            // afer function execution (apsects in reversed order)
            for(int i=aspects.Count-1; i >= 0; i--)
            {
                if(runAfterMethod[i])
                {
                    retval = aspects[i].AfterExecution(_serviceProvider, targetMethod, _service, args, retval, mainMethodDisabled, mainMethodException);
                }
            }

            // throw main method exception if occured
            if(mainMethodException != null)
            {
                throw mainMethodException;
            }

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
