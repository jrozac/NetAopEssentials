using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace NetCoreAopEssentials
{

    /// <summary>
    /// Cache proxy 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AspectProxy<TService> : DispatchProxy
        where TService : class
    {

        /// <summary>
        /// Aspects 
        /// </summary>
        private static Dictionary<Type, List<IAspect>> Aspects = new Dictionary<Type, List<IAspect>>();

        /// <summary>
        /// Service 
        /// </summary>
        private TService _service;

        /// <summary>
        /// Service provider  
        /// </summary>
        private IServiceProvider _serviceProvider;

        /// <summary>
        /// Invoke method 
        /// </summary>
        /// <param name="targetMethod"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            // get aspects to be exectued
            List<IAspect> aspects = Aspects.ContainsKey(_service.GetType()) ? 
                Aspects[_service.GetType()] : new List<IAspect>();

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
        /// <param name="sp"></param>
        /// <returns></returns>
        public static TService Create<TImplementation>(IServiceProvider sp)
            where TImplementation : class,TService
        {

            // create instance 
            TService impl = ActivatorUtilities.CreateInstance<TImplementation>(sp);

            // create proxy 
            object proxy = Create<TService, AspectProxy<TService>>();
            ((AspectProxy<TService>)proxy)._service = impl;
            ((AspectProxy<TService>)proxy)._serviceProvider = sp;

            // return
            return (TService) proxy;
        }

        /// <summary>
        /// Configure aspect for type 
        /// </summary>
        /// <typeparam name="TImplementation"></typeparam>
        /// <typeparam name="TAspect"></typeparam>
        /// <param name="customCreate"></param>
        internal static void Configure<TImplementation,TAspect>(Func<TAspect> customCreate = null)
            where TImplementation : class, TService
            where TAspect : class, IAspect
        {
            // create aspect list
            if(!Aspects.ContainsKey(typeof(TImplementation)))
            {
                Aspects.Add(typeof(TImplementation), new List<IAspect>());
            }

            // add aspect 
            if(!Aspects[typeof(TImplementation)].Any(a => a.GetType() == typeof(TAspect)))
            {
                var aspect = customCreate?.Invoke() ?? Activator.CreateInstance<TAspect>();
                aspect.ConfigureFor<TImplementation>();
                Aspects[typeof(TImplementation)].Add(aspect);
            }
        }

    }
}
