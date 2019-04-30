using System;
using System.Collections.Generic;
using System.Linq;

namespace NetAopEssentials
{

    /// <summary>
    /// Aspects container
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    internal class AspectsContainer<TService,TImplementation>
        where TService : class
        where TImplementation : class, TService
    {

        /// <summary>
        /// Aspects 
        /// </summary>
        internal List<IAspect<TService, TImplementation>> Aspects { get; private set; } = 
            new List<IAspect<TService, TImplementation>>();

        /// <summary>
        /// Configure aspect for type 
        /// </summary>
        /// <typeparam name="TAspect"></typeparam>
        /// <param name="customCreate"></param>
        internal void Configure<TAspect>(Func<TAspect> customCreate = null)
            where TAspect : class, IAspect<TService,TImplementation>
        {

            // check if already exists 
            var currentAspect = Aspects.FirstOrDefault(a => a.GetType() == typeof(TAspect));
            if (currentAspect != null)
            {
                throw new ArgumentException($"Another {typeof(TAspect)} is already defined.");
            }

            // create aspect and add to list 
            var aspect = customCreate?.Invoke() ?? Activator.CreateInstance<TAspect>();
            aspect.Configure();
            Aspects.Add(aspect);
        }

    }
}
