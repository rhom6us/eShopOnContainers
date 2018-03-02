using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Identity.API.Extensions {
    public static class RewsoStringExtensions {
        public static string Join(this IEnumerable<string> source, string seperator = "") {
            return string.Join(seperator, source);
        }

        [NotNull]
        public static string ReplaceR(this string source, string pattern, string replacement = "", RegexOptions options = default(RegexOptions)) {
            return Regex.Replace(source, pattern, replacement, options);
        }

        [NotNull]
        public static string EnsureTrailing(this string source, string trail) {
            return source.ReplaceR($@"({Regex.Escape(trail)})*$", string.Empty) + trail;
        }
    }
}