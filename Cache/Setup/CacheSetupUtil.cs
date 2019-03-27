using NetAopEssentials.Cache.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetAopEssentials.Cache.Setup
{

    /// <summary>
    /// Cache setup util
    /// </summary>
    internal static class CacheSetupUtil
    {

        /// <summary>
        /// Create cache configuration
        /// </summary>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="profile"></param>
        /// <param name="defaults"></param>
        /// <returns></returns>
        internal static MethodCacheConfiguration CreateCacheConfiguration<TImplementation>(MethodCacheProfile<TImplementation> profile, 
            CacheSetupDefaults defaults)
            where TImplementation : class
        {

            // check if cacheable first. It throws an exception if type is not cacheable 
            if (profile.Action == EnumCacheAction.Set)
            {
                CheckIfCacheable(profile.MethodInfo, profile.Provider ?? defaults.DefaultProvider);
            }

            // set configuration and return 
            var cfg = new MethodCacheConfiguration
            {
                MethodInfo = profile.MethodInfo,
                KeyTpl = profile.KeyTpl,
                Action = profile.Action,
                CacheResultFunc = profile.CacheResultFunc,
                TimeoutMs = profile.Timeout > 0 ? profile.Timeout : defaults.DefaultTimeout,
                TimeoutMsOffsetFunc = profile.TimeoutFunc,
                KeyFunc = CreateKeyFunc(profile.MethodInfo, profile.Action, profile.KeyTpl),
                Provider = profile.Provider ?? defaults.DefaultProvider,
                KeyPrefix = defaults.KeyCustomPrefix ?? $"{System.Diagnostics.Process.GetCurrentProcess().ProcessName}.{typeof(TImplementation)}.",
            };
            return cfg;
        }

        /// <summary>
        /// Reads attributes configuration 
        /// </summary>
        internal static List<MethodCacheProfile<TImplementation>> GetAttributesProfiles<TImplementation>()
            where TImplementation : class
        {
            // get methods to set cache
            var mehtodsToSet = typeof(TImplementation).GetMethods().Where(m =>
                m.GetCustomAttributes().FirstOrDefault(a => a.GetType() == typeof(CacheableAttribute)) != null);

            // get methods to remove cache
            var mehtodsToRemove = typeof(TImplementation).GetMethods().Where(m =>
                m.GetCustomAttributes().FirstOrDefault(a => a.GetType() == typeof(CacheRemoveAttribute)) != null);

            // configure set cache profiles
            var setProfiles = mehtodsToSet.Select(m =>
            {
                var def = (CacheableAttribute)m.GetCustomAttributes().First(a => a.GetType() == typeof(CacheableAttribute));
                var profile = new MethodCacheProfile<TImplementation>
                {
                    Action = EnumCacheAction.Set,
                    KeyTpl = def.KeyTemplate,
                    MethodInfo = m,
                    Provider = def.Provider,
                    Timeout = def.TimeoutMs
                };
                return profile;
            }).ToList();

            // configure remove cache profiles
            var removeProfiles = mehtodsToRemove.Select(m =>
            {
                var def = (CacheRemoveAttribute)m.GetCustomAttributes().First(a => a.GetType() == typeof(CacheRemoveAttribute));
                var profile = new MethodCacheProfile<TImplementation>
                {
                    Action = EnumCacheAction.Remove,
                    KeyTpl = def.KeyTemplate,
                    MethodInfo = m,
                    Provider = def.Provider
                };
                return profile;
            }).ToList();

            // merge and return 
            removeProfiles.AddRange(setProfiles);
            return removeProfiles;

        }

        /// <summary>
        /// Creates key get function 
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="action"></param>
        /// <param name="keyTpl"></param>
        /// <returns></returns>
        public static Func<object[], object, string> CreateKeyFunc(MethodInfo methodInfo, EnumCacheAction action, string keyTpl)
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
