using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace NetCoreAopEssentials
{

    /// <summary>
    /// General util
    /// </summary>
    internal static class GeneralUtil
    {

        /// <summary>
        /// Log error
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="logName"></param>
        /// <param name="mesage"></param>
        /// <param name="args"></param>
        public static void LogError(IServiceProvider provider, string logCategory, string mesage, params object[] args)
        {
            var factory = provider.GetService<ILoggerFactory>();
            var logger = factory?.CreateLogger(logCategory);
            logger?.LogError(mesage, args);
        }

        /// <summary>
        /// Log error
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void LogError<T>(IServiceProvider provider, string message, params object[] args) where T : class
        {
            var logger = provider.GetService<ILogger<T>>();
            logger?.LogError(message, args);
        }

        /// <summary>
        /// Log information
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void LogInformation<T>(IServiceProvider provider, string message, params object[] args) where T : class
        {
            var logger = provider.GetService<ILogger<T>>();
            logger?.LogInformation(message, args);
        }

        /// <summary>
        /// Log warning
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void LogWarning<T>(IServiceProvider provider, string message, params object[] args) where T : class
        {
            var logger = provider.GetService<ILogger<T>>();
            logger?.LogWarning(message, args);
        }

        /// <summary>
        /// Try run action
        /// </summary>
        /// <typeparam name="TRef"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="action"></param>
        /// <param name="defaultReturn"></param>
        /// <param name="logTemplate"></param>
        /// <returns></returns>
        public static T TryRun<TRef,T>(IServiceProvider provider, Func<T> action, T defaultReturn, string logTemplate) where TRef : class
        {
            try
            {
                return action();
            } catch(Exception e)
            {
                LogError<TRef>(provider, logTemplate, e.Message);
                return defaultReturn;
            }
        }

        /// <summary>
        /// Try run
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="logCategory"></param>
        /// <param name="action"></param>
        /// <param name="defaultReturn"></param>
        /// <param name="logTemplate"></param>
        /// <returns></returns>
        public static T TryRun<T>(IServiceProvider provider, string logCategory, Func<T> action, T defaultReturn, string logTemplate)
        {
            try
            {
                return action();
            }
            catch (Exception e)
            {
                LogError(provider, logCategory, logTemplate, e.Message);
                return defaultReturn;
            }
        }

        /// <summary>
        /// Replace first occurence
        /// </summary>
        /// <param name="text"></param>
        /// <param name="search"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

    }
}
