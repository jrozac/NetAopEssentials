using System;

namespace NetAopEssentialsTest.Aspects
{
    /// <summary>
    /// Aspect two
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    public class AspectTwo<TService, TImplementation> : TestAspectVoid<TService, TImplementation>
        where TService : class where TImplementation : class, TService
    {
        public AspectTwo(Action before, Action after) : base(before, after)
        {
        }
    };
}
