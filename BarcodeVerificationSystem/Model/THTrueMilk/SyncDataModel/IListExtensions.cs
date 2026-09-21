using System;
using System.Collections.Generic;

namespace BarcodeVerificationSystem.Model.THTrueMilk
{
    internal static class IListExtensions
    {
        public static int FindIndex<T>(this IList<T> list, Predicate<T> match)
        {
            for (int i = 0; i < list.Count; i++) if (match(list[i])) return i; return -1;
        }
        public static T Find<T>(this IList<T> list, Predicate<T> match)
        {
            for (int i = 0; i < list.Count; i++) if (match(list[i])) return list[i]; return default;
        }
        public static int RemoveAll<T>(this IList<T> list, Predicate<T> match)
        {
            int removed = 0;
            for (int i = list.Count - 1; i >= 0; i--) { if (match(list[i])) { list.RemoveAt(i); removed++; } }
            return removed;
        }
    }
}
