using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetCoreAopEssentials.Cache
{

    /// <summary>
    /// Cache key util
    /// </summary>
    internal static class CacheKeyUtil
    {

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
            var fields = ReflectionUtils.GetFieldsFromTemplate(keyTpl);
            
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
            fields.ToList().ForEach( f => tpl = tpl.ReplaceFirst("{" + f + "}", "{" + i++ + "}"));

            // return key mapper
            return new Func<object[], object, string>((args, retval) => {
                var formatArgs = retrieveFunc.Select(f => f?.Invoke(retval, args)).ToArray();
                if(formatArgs.Any(v => v ==null))
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
        private static Func<object, object[],object> GetFieldValueFunc(string keyField, MethodInfo methodInfo)
        {

            var fieldParts = keyField.Split('.');
            string key = fieldParts[0];
            fieldParts = fieldParts.Skip(1).ToArray();
            int index = GetKeyFieldIndex(keyField, methodInfo);

            // get key type
            Type type = null;
            if(key == "_ret")
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
                    var funcType = typeof(Func<,>).MakeGenericType(type,prop.PropertyType);
                    var getDelegate = Delegate.CreateDelegate(funcType, prop.GetMethod);
                    type = prop.PropertyType;
                    return getDelegate;
                }).ToList();
            }

            if(delegates != null)
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
            } else
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
            else if(methodInfo.ReturnType == typeof(void)) {
                throw new ArgumentException("Method returns void.");
            }

            // return
            return position;
        }

    }
}
