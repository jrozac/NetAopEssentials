using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetAopEssentials.Cache
{

    /// <summary>
    /// Cache setup util
    /// </summary>
    internal static class CacheSetupUtil
    {

        /// <summary>
        /// Create cache plan
        /// </summary>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="setup"></param>
        /// <param name="defaults"></param>
        /// <returns></returns>
        internal static MethodCachePlan CreateCachePlan<TImplementation>(MethodCacheSetup<TImplementation> setup, 
            CacheSetupDefaults defaults)
            where TImplementation : class
        {

            // set plan
            var plan = new MethodCachePlan
            {
                MethodInfo = setup.MethodInfo,
                KeyTpl = setup.KeyTpl,
                Action = setup.Action,
                CacheResultFunc = setup.CacheResultFunc,
                TimeoutMs = setup.Timeout.HasValue ? setup.Timeout.Value : defaults.DefaultTimeout,
                TimeoutMsOffsetFunc = setup.TimeoutFunc,
                KeyFunc = CreateKeyFunc(setup.MethodInfo, setup.Action, setup.KeyTpl),
                Provider = setup.Provider ?? defaults.DefaultProvider,
                KeyPrefix = defaults.KeyCustomPrefix ?? $"{System.Diagnostics.Process.GetCurrentProcess().ProcessName}.{typeof(TImplementation)}.",
            };
            
            // chech timeout
            if (plan.TimeoutMs <= 0)
            {
                throw new ArgumentException("Timeout has to be grater than zero.");
            }

            // check if cacheable, it throws an exception if type is not cacheable 
            if (setup.Action == EnumCacheAction.Set)
            {
                CheckIfCacheable(setup.MethodInfo, setup.Provider ?? defaults.DefaultProvider);
            }

            // return
            return plan;
        }

        /// <summary>
        /// Reads attributes setup 
        /// </summary>
        internal static List<MethodCacheSetup<TImplementation>> GetAttributesSetups<TImplementation>()
            where TImplementation : class
        {
            // get methods to set cache
            var mehtodsToSet = typeof(TImplementation).GetMethods().Where(m =>
                m.GetCustomAttributes().FirstOrDefault(a => a.GetType() == typeof(CacheSetAttribute)) != null);

            // get methods to remove cache
            var mehtodsToRemove = typeof(TImplementation).GetMethods().Where(m =>
                m.GetCustomAttributes().FirstOrDefault(a => a.GetType() == typeof(CacheRemoveAttribute)) != null);

            // configure set cache setups
            var setSetups = mehtodsToSet.Select(m =>
            {
                var def = (CacheSetAttribute)m.GetCustomAttributes().First(a => a.GetType() == typeof(CacheSetAttribute));
                var setup = new MethodCacheSetup<TImplementation>
                {
                    Action = EnumCacheAction.Set,
                    KeyTpl = def.KeyTemplate,
                    MethodInfo = m,
                    Provider = def.Provider,
                    Timeout = def.TimeoutMs
                };
                return setup;
            }).ToList();

            // configure remove cache setups
            var removeSetups = mehtodsToRemove.Select(m =>
            {
                var def = (CacheRemoveAttribute)m.GetCustomAttributes().First(a => a.GetType() == typeof(CacheRemoveAttribute));
                var setup = new MethodCacheSetup<TImplementation>
                {
                    Action = EnumCacheAction.Remove,
                    KeyTpl = def.KeyTemplate,
                    MethodInfo = m,
                    Provider = def.Provider
                };
                return setup;
            }).ToList();

            // merge and return 
            removeSetups.AddRange(setSetups);
            return removeSetups;

        }

        /// <summary>
        /// Creates key get function 
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="action"></param>
        /// <param name="keyTpl"></param>
        /// <returns></returns>
        internal static Func<object[], object, string> CreateKeyFunc(MethodInfo methodInfo, EnumCacheAction action, string keyTpl)
        {

            // get fileds 
            var fields = GeneralUtil.GetFieldsFromTemplate(keyTpl);

            // no fields 
            if (!fields.Any())
            {
                return new Func<object[], object, string>((args, retval) => keyTpl);
            }

            // prepare retrieve funcs
            var retrieveFunc = fields.Select(f => GetFieldValueFunc(f, methodInfo)).ToArray();

            // set replace template 
            string tpl = keyTpl;
            int i = 0;
            fields.ToList().ForEach(f => tpl = tpl.ReplaceFirst("{" + f + "}", "{" + i++ + "}"));

            // return key mapper
            return new Func<object[], object, string>((args, retval) => {
                var formatArgs = retrieveFunc.Select(f => f?.Invoke(retval, args)).ToArray();
                if (formatArgs.Any(v => v == null))
                {
                    return null;
                }
                var key = string.Format(tpl, formatArgs);
                return key;
            });
        }

        /// <summary>
        /// Creates plan from setup
        /// </summary>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="cacheSetup"></param>
        /// <returns></returns>
        internal static List<MethodCachePlan> SetupToPlan<TImplementation>(CacheSetup<TImplementation> cacheSetup)
            where TImplementation : class
        {

            // set plans 
            var plans = cacheSetup.MethodsCacheSetups.Select(setup =>
                CreateCachePlan(setup, cacheSetup.Defaults)).ToList();

            // import attributes plans
            if (cacheSetup.Defaults.ReadAttributes)
            {
                var attrSetups = GetAttributesSetups<TImplementation>();
                var attrCfgs = attrSetups.Select(setup => CreateCachePlan(setup, cacheSetup.Defaults)).ToList();
                plans.AddRange(attrCfgs);
            }

            // return
            return plans;
        }

        /// <summary>
        /// Get field value getter func
        /// </summary>
        /// <param name="keyField"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private static Func<object, object[], object> GetFieldValueFunc(string keyField, MethodInfo methodInfo)
        {
            // read data 
            var fieldParts = keyField.Split('.');
            string key = fieldParts[0];
            fieldParts = fieldParts.Skip(1).ToArray();
            int index = GetKeyFieldIndex(keyField, methodInfo);

            // get key type
            Type type = null;
            if (key == "_ret")
            {
                // return
                type = methodInfo.ReturnType;
            }
            else
            {
                // parameter value 
                var param = methodInfo.GetParameters()[index - 1];
                type = param.ParameterType;
            }

            // create deletates 
            List<Delegate> delegates = null;
            if (fieldParts.Length > 0)
            {
                delegates = Enumerable.Range(0, fieldParts.Length).Select(i =>
                {
                    var propName = fieldParts[i];
                    var prop = type.GetProperty(propName);
                    var funcType = typeof(Func<,>).MakeGenericType(type, prop.PropertyType);
                    var getDelegate = Delegate.CreateDelegate(funcType, prop.GetMethod);
                    type = prop.PropertyType;
                    return getDelegate;
                }).ToList();
            }

            // create function
            if (delegates != null)
            {
                return new Func<object, object[], object>((retval, args) => {
                    object val = index == 0 ? retval : args[index - 1];
                    delegates.ForEach(d => {
                        if (val != null)
                        {
                            val = d.DynamicInvoke(val);
                        }
                    });
                    return val;
                });
            }
            else
            {
                return new Func<object, object[], object>((retval, args) => {
                    return index == 0 ? retval : args[index - 1];
                });
            }

        }

        /// <summary>
        /// Gets template key field index. 
        /// Return value (_ret) has index value 0, while method parameters have sequence number incremented by 1
        /// </summary>
        /// <param name="keyField"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private static int GetKeyFieldIndex(string keyField, MethodInfo methodInfo)
        {

            // method parameters
            var parameters = methodInfo.GetParameters();

            // get parameter data from field name 
            string paramName = keyField.Split('.')[0];

            // get position
            int position = 0;
            if (paramName != "_ret")
            {
                int paramIndex = Array.FindIndex(parameters, p => p.Name == paramName);
                if (paramIndex == -1)
                {
                    throw new ArgumentException($"Field {paramName} is not valid.");
                }
                position = 1 + paramIndex;
            }
            else if (methodInfo.ReturnType == typeof(void))
            {
                throw new ArgumentException("Method returns void.");
            }

            // return
            return position;
        }

        /// <summary>
        /// Checks whether type is cacheable
        /// </summary>
        /// <param name="info"></param>
        /// <param name="provider"></param>
        private static void CheckIfCacheable(MethodInfo info, EnumCacheProvider provider)
        {
            // get return type 
            var type = info.ReturnType;

            // check type is valid
            if (type == typeof(void))
            {
                throw new InvalidOperationException($"Method {info.Name} does not return and cannot be cached.");
            }

            // check if can be stored to cache 
            if (provider == EnumCacheProvider.Distributed && !CacheSerializationUtil.IsTypeSupported(type))
            {
                throw new InvalidOperationException($"Type {type} of method {info.Name} cannot be cached with provider {provider}. Classes must have SerializableAttribute.");
            }
        }

    }
}
