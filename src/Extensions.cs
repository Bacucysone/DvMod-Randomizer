using System;
using System.Collections.Generic;
using System.Linq;

namespace DvMod.Randomizer;

public static class Extensions { 
    /// <summary>
    /// Returns the list element that minimizes a given function
    /// </summary>
    /// <param name="list">Set of elements in which to look from</param>
    /// <param name="fDist">The function to minimize</param>
    /// <typeparam name="T">The type of elements of the list</typeparam>
    /// <returns>The first element x of <paramref name="list"/> that verifies <paramref name="fDist"/>(x) = min([<paramref name="fDist"/>(y) for y in <paramref name="list"/>])</returns>
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