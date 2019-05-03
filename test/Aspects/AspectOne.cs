using System;

namespace NetAopEssentialsTest.Aspects
{
    /// <summary>
    /// Aspect one
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    public class AspectOne<TService, TImplementation> : TestAspectVoid<TService, TImplementation>
        where TService : class where TImplementation : class, TService
    {
        public AspectOne(Action before, Action after) : base(before, after)
        {
        }

        public AspectOne() : base(() => { }, () => { })
        {

        }
    };
}
