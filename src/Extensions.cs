using System;
using System.Collections.Generic;
using System.Linq;

namespace DvMod.Randomizer;

public static class Extensions {
    public static T FindMin<T>(this IEnumerable<T> list, Func<T, float> fDist) {
        T[] enumerable = list.ToArray();
        T elem = enumerable.FirstOrDefault();
        foreach (T x in enumerable) {
            if (fDist(x) >= fDist(elem)) continue;
            elem = x;
        }
        return elem;
    }
    public static T[] CopyLast<T>(this T[] list) {
        if (!list.Any()) return [];
        return [.. list, list.Last()];
    }
    
    public static int Offset(this long x, long offset) => (int)(x - offset);
}