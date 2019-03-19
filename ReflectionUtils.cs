using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NetCoreAopEssentials
{

    /// <summary>
    /// Reflection utils
    /// </summary>
    public static class ReflectionUtils
    {

        /// <summary>
        /// Gets fields fro declared template 
        /// </summary>
        /// <param name="keyTpl"></param>
        /// <returns></returns>
        public static List<string> GetFieldsFromTemplate(string keyTpl)
        {
            Regex regex = new Regex(@"(?<=\{)[^}]*(?=\})", RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(keyTpl);
            return matches.Cast<Match>().Select(m => m.Value).Distinct().ToList();
        }

    }
}
