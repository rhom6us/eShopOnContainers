using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.eShopOnContainers.Services.Identity.API.Extensions {
    public static class RewsoStringExtensions {
        public static string Join(this IEnumerable<string> source, string seperator = "") {
            return string.Join(seperator, source);
        }

        public static string ReplaceR(this string source, string pattern, string replacement = "", RegexOptions options = default(RegexOptions)) {
            return Regex.Replace(source, pattern, replacement, options);
        }

        public static string EnsureTrailing(this string source, string trail) {
            return source.ReplaceR($@"({Regex.Escape(trail)})*$", string.Empty) + trail;
        }
    }
}