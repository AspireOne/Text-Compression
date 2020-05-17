using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HuffmanCompression.Extensions
{
    public static class EnumerableExtensions
    {
        private static IEnumerable<string> GraphemeClusters(this string s)
        {
            var enumerator = StringInfo.GetTextElementEnumerator(s);
            while (enumerator.MoveNext())
                yield return (string)enumerator.Current;
        }

        public static string Reverse(this string s) => string.Join("", s.GraphemeClusters().Reverse().ToArray());
    }
}