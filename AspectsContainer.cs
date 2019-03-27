using System;
using System.Collections.Generic;
using System.Linq;

namespace NetAopEssentials
{

    /// <summary>
    /// Aspects container
    /// </summary>
    internal class AspectsContainer
    {

        /// <summary>
        /// Aspects 
        /// </summary>
        private Dictionary<Type, List<IAspect>> _aspects = new Dictionary<Type, List<IAspect>>();

        /// <summary>
        /// Configure aspect for type 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <typeparam name="TAspect"></typeparam>
        /// <param name="customCreate"></param>
        internal void Configure<TService, TImplementation, TAspect>(Func<TAspect> customCreate = null)
            where TService : class
            where TImplementation : class, TService
            where TAspect : class, IAspect<TImplementation>
        {

            // check if already exists 
            var currentAspect = GetRegisteredAspect<TImplementation, TAspect>();
            if (currentAspect != null)
            {
                throw new ArgumentException($"Another {typeof(TAspect)} is already defined.");
            }

            // create aspect list
            if (!_aspects.ContainsKey(typeof(TImplementation)))
            {
                _aspects.Add(typeof(TImplementation), new List<IAspect>());
            }

            // add aspect 
            if (!_aspects[typeof(TImplementation)].Any(a => a.GetType() == typeof(TAspect)))
            {
                var aspect = customCreate?.Invoke() ?? Activator.CreateInstance<TAspect>();
                aspect.Configure();
                _aspects[typeof(TImplementation)].Add(aspect);
            }
        }

        /// <summary>
        /// Get registered aspect
        /// </summary>
        /// <typeparam name="TImplementation"></typeparam>
        /// <typeparam name="TAspect"></typeparam>
        /// <returns></returns>
        internal TAspect GetRegisteredAspect<TImplementation, TAspect>()
            where TAspect : IAspect
            where TImplementation : class
        {
            // get aspects for type
            var aspects = _aspects.ContainsKey(typeof(TImplementation)) ? _aspects[typeof(TImplementation)] : null;
            if (aspects == null)
            {
                return default(TAspect);
            }
            return (TAspect)aspects.FirstOrDefault(a => a.GetType() == typeof(TAspect));
        }

        /// <summary>
        /// Get aspects for type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal List<IAspect> GetAspects(Type type)
        {
            return _aspects.ContainsKey(type) ? _aspects[type] : new List<IAspect>();
        }

    }
}
