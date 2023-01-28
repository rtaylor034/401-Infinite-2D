using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Mono.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class GExtensions
{

    #region General Enumerable
    /// <summary>
    /// Checks if this has exactly one element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumberable"></param>
    /// <param name="element"></param>
    /// <remarks>
    /// > If TRUE, <paramref name="element"/> is set to the element. (will be garbage value if FALSE).
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
    public static IEnumerable<T> Wrapped<T>(this T item)
    {
        yield return item;
    }

    public static IEnumerable<T> Without<T>(this IEnumerable<T> enumerable, IEnumerable<T> exclusions)
    {
        foreach (var item in enumerable)
        {
            if (exclusions.Contains(item)) continue;
            yield return item;
        }
    }
    public static IEnumerable<T> Without<T>(this IEnumerable<T> enumerable, T exclusion) =>
        Without(enumerable, exclusion.Wrapped());
    #endregion

    #region Delegates
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

    #region InvokeAll()
    //Add more when needed
    //NEEDS DOCS
    public static TResult[] InvokeAll<TResult>(this IList<Func<TResult>> list)
    {
        var o = new TResult[list.Count];
        for (int i = 0; i < list.Count; i++) o[i] = list[i]();
        return o;
    }
    public static TResult[] InvokeAll<T1, TResult>(this IList<Func<T1, TResult>> list, T1 arg1)
    {
        var o = new TResult[list.Count];
        for (int i = 0; i < list.Count; i++) o[i] = list[i](arg1);
        return o;
    }
    public static TResult[] InvokeAll<T1, T2, TResult>(this IList<Func<T1, T2, TResult>> list, T1 arg1, T2 arg2)
    {
        var o = new TResult[list.Count];
        for (int i = 0; i < list.Count; i++) o[i] = list[i](arg1, arg2);
        return o;
    }
    public static TResult[] InvokeAll<T1, T2, T3, TResult>(this IList<Func<T1, T2, T3, TResult>> list, T1 arg1, T2 arg2, T3 arg3)
    {
        var o = new TResult[list.Count];
        for (int i = 0; i < list.Count; i++) o[i] = list[i](arg1, arg2, arg3);
        return o;
    }
    #endregion

    #region InvokeAwaitAll()
    //Add more when needed
    public static async Task<TResult[]> InvokeAwaitAll<TResult>(this IList<Func<Task<TResult>>> list)
    {
        var o = new TResult[list.Count];
        for (int i = 0; i < list.Count; i++) o[i] = await list[i]();
        return o;
    }
    public static async Task<TResult[]> InvokeAwaitAll<T1, TResult>(this IList<Func<T1, Task<TResult>>> list, T1 arg1)
    {
        var o = new TResult[list.Count];
        for (int i = 0; i < list.Count; i++) o[i] = await list[i](arg1);
        return o;
    }
    public static async Task<TResult[]> InvokeAwaitAll<T1, T2, TResult>(this IList<Func<T1, T2, Task<TResult>>> list, T1 arg1, T2 arg2)
    {
        var o = new TResult[list.Count];
        for (int i = 0; i < list.Count; i++) o[i] = await list[i](arg1, arg2);
        return o;
    }
    public static async Task<TResult[]> InvokeAwaitAll<T1, T2, T3, TResult>(this IList<Func<T1, T2, T3, Task<TResult>>> list, T1 arg1, T2 arg2, T3 arg3)
    {
        var o = new TResult[list.Count];
        for (int i = 0; i < list.Count; i++) o[i] = await list[i](arg1, arg2, arg3);
        return o;
    }
    #endregion

    #endregion

    #region Boolean Logic

    public static bool GateAND(this IEnumerable<bool> bools, bool invert = false)
    {
        foreach (bool value in bools)
            if (value == invert) return false;
        return true;
    }
    public static bool GateOR(this IEnumerable<bool> bools, bool invert = false)
    {
        foreach (bool value in bools)
            if (value == !invert) return true;
        return false;
    }
    #endregion

    public static T CompareAndSelect<T>(this IEnumerable<T> values, Func<T, T, bool> statement)
    {
        var o = values.GetEnumerator().Current;
        foreach(T t in values)
        {
            if (!statement(o, t)) o = t;
        }
        return o;
    }

    #region Queue
    public static bool CycleTo<T>(this Queue<T> queue, T element)
    {
        for (int i = 0; i < queue.Count; i++)
        {
            if (queue.Peek().Equals(element)) return true;
            queue.Cycle();
        }
        return false;
    }
    public static void Cycle<T>(this Queue<T> queue, uint n = 1)
    {
        for (uint i = 0; i < n; i++) queue.Enqueue(queue.Dequeue());
    }
    #endregion

    #region Trig
    public static Vector2 PolarToCartesian(this Vector2 polar, bool isDegrees = false)
    {
        polar.x = isDegrees ? polar.x * Mathf.Deg2Rad : polar.x;
        return new(Mathf.Cos(polar.x) * polar.y, Mathf.Sin(polar.x) * polar.y);
    }
    public static Vector2 PolarToCartesian(this (float angle, float radius) polar, bool isDegrees = false)
    {
        polar.angle = isDegrees ? polar.angle * Mathf.Deg2Rad : polar.angle;
        return new(Mathf.Cos(polar.angle) * polar.radius, Mathf.Sin(polar.angle) * polar.radius);
    }

    #endregion
}
