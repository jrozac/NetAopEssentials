using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetCoreAopEssentials.Cache
{
    /// <summary>
    /// Serialization util
    /// </summary>
    internal static class CacheSerializationUtil
    {

        /// <summary>
        /// Formatter
        /// </summary>
        public static BinaryFormatter Formatter = new BinaryFormatter();

        /// <summary>
        /// Deserialize 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static object Deserialize(IServiceProvider provider, byte[] buffer)
        {
            // null
            if (buffer == null)
            {
                return null;
            }

            // deserilaize
            return GeneralUtil.TryRun(provider, typeof(CacheSerializationUtil).Name, () =>
            {

                using (var memStream = new MemoryStream())
                {
                    var binForm = new BinaryFormatter();
                    memStream.Write(buffer, 0, buffer.Length);
                    memStream.Seek(0, SeekOrigin.Begin);
                    var obj = binForm.Deserialize(memStream);
                    return obj;
                }
            }, null, "Failed to deserialize item with error: {message }.");
        }

        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static byte[] Serialize(IServiceProvider provider, object val)
        {
            // null 
            if(val == null)
            {
                return null;
            }

            // serialize
            return GeneralUtil.TryRun(provider, typeof(CacheSerializationUtil).Name, () =>
            {
                using (var ms = new MemoryStream())
                {
                    Formatter.Serialize(ms, val);
                    return ms.ToArray();
                }
            }, null, "Failed to serialize item with error: {message}.");
        }

        /// <summary>
        /// Checks whether type can be serialized
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsTypeSupported(Type type)
        {
            var serializable = IsSerializableRecursive(type);
            return serializable;
        }

        /// <summary>
        /// Recursive check for serialization support
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsSerializableRecursive(Type type)
        {
            // primitives and strings are serializable
            if(type.IsPrimitive || type == typeof(string))
            {
                return true;
            }

            // not serializable 
            if(!type.IsSerializable)
            {
                return false;
            }

            // for the rest try to create an instance and serialize it 
            try
            {
                var obj = Activator.CreateInstance(type);
                return Serialize(null, obj) != null;

            } catch(Exception)
            {
                return false;
            }

        }

    }
}
