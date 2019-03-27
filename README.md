# NetAopEssentials
Net AOP support. Support for generic AOP and caching of custom methods.

## Overview
Services can be registered through aspect proxy (or multiple aspect proxies). 
This allows custom code to run before and after methods execution. 
A custom proxy can be implemented by implementing the interface IAspect. 
Aspects are initialized one per application lifetime per service registration. 
E.g. if the same aspect is used for 3 services, 3 aspect instances will be initialized, 
regardless whether the services are registered as singletons, scoped or transient. 
Upon initialization the **ConfigureFor** method is called.

The following code registers a custom aspect for selected service.

~~~cs
IServiceCollection services = new ServiceCollection();
services.ConfigureAspectProxy<IDummyService, DummyService>().RegisterAspect<DummyAspect>().AddScoped();
services.BuildServiceProvider();
~~~

## Caching aspect
Caching is an aspect implementation to support custom methods caching. Memory cache and distributed cache are supported. 
Therefore, at least one is required in service collection. Methods caching can be defined by using **CacheableAttribute** 
and **CacheRemoveAttribute** attributes on selected methods. Another option to define methods caching is through service 
collection configuration. Both ways require caching aspect registration. 
The following code shows caching of IUserService where methods for caching are defined with attributes only.

~~~cs
IServiceCollection services = new ServiceCollection();
services.AddMemoryCache();
svc.ConfigureAspectProxy<IUserService, UserService>().RegisterAspect<CacheAspect<UserService>>().AddScoped();
services.BuildServiceProvider();
~~~

Note that the timeout is generally set to 60000 ms and MemoryCache is defined as default provider. However both values can be 
overridden by attributes definition.The following code shows custom caching registration methods through service collection 
configuration.

~~~cs
IServiceCollection services = new ServiceCollection();
services.AddScopedCacheable<IUserService, UserService>((set) => set.
	SetFor(m => m.GetUser(0), "user-{id}").
	RemoveFor(m => m.Save(new User()), "user-{user.Id}").
	RemoveFor(m => m.UpdateRandomUser(), "user-{_ret.Id}").
	CacheDefaultProvider(EnumCacheProvider.Memory).
	CacheDefaultTimeout(CacheTimeout.Minute).
	ImportAttributesConfiguration()
);
~~~

Not the key template syntax. For values in curly brackets real values from method parameters are used. 
To use the method return value, the prefix **_ret** might be used.
Note that parameters names are case sensitive.