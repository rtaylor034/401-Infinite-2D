using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GExtensions
{

    /// <summary>
    /// Checks if this has exactly one element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumberable"></param>
    /// <param name="element"></param>
    /// <remarks>
    /// If TRUE, <paramref name="element"/> is set to the element. (will be garbage value if FALSE).
    /// </remarks>
    public static bool IsSingleElement<T>(this IEnumerable<T> enumberable, out T element)
    {
        bool i = true;
        element = default;
        foreach (var item in enumberable)
        {
            if (!i) return false;
            element = item;
            i = false;
        }
        return !i;
    }


}
