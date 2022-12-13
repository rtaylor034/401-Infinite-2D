using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public static bool IsSingleElement<T>(this IEnumerable<T> enumerable, out T element)
    {
        bool i = true;
        element = default;
        foreach (var item in enumerable)
        {
            if (!i) return false;
            element = item;
            i = false;
        }
        return !i;
    }

    //taken from https://stackoverflow.com/questions/1577822/passing-a-single-item-as-ienumerablet
    /// <summary>
    /// Wraps this object instance into an <see cref="IEnumerable"/>&lt;<typeparamref name="T"/>&gt;
    /// consisting of a single item.
    /// </summary>
    /// <typeparam name="T"> Type of the object. </typeparam>
    /// <param name="item"> The instance that will be wrapped. </param>
    /// <returns> An <see cref="IEnumerable"/>&lt;<typeparamref name="T"/>&gt; consisting of a single item. </returns>
    public static IEnumerable<T> YieldAsEnumerable<T>(this T item)
    {
        yield return item;
    }
    /// <summary>
    /// [Shorthand] <br></br>
    /// <c>.GetInvocationList().Cast&lt;<typeparamref name="T"/>&gt;()</c>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="del"></param>
    /// <returns></returns>
    public static IEnumerable<T> CastedInvocationList<T>(this T del) where T : MulticastDelegate
    {
        return del.GetInvocationList().Cast<T>();
    }
}
