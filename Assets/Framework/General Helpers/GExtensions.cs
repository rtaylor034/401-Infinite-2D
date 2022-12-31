using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public static IEnumerable<T> Wrapped<T>(this T item)
    {
        yield return item;
    }
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
    
    public static T[] GetInvocationValues<T>(this Delegate @delegate, params object[] parameters) =>
        GetInvocationValues<T>(@delegate.GetInvocationList(), parameters);

    public static T[] GetInvocationValues<T>(this IList<Delegate> methods, params object[] parameters)
    {
        var o = new T[methods.Count()];
        for(int i = 0; i < o.Length; i++)
        {
            o[i] = (T)methods[i].DynamicInvoke(parameters);
        }
        return o;
    }
    #endregion

    #region Boolean Logic
    public static bool GateAND(this IEnumerable<bool> bools, bool invert = false)
    {
        foreach (bool value in bools)
            if (value == invert) return invert;
        return !invert;
    }

    public static bool GateOR(this IEnumerable<bool> bools, bool invert = false)
    {
        foreach (bool value in bools)
            if (value == !invert) return !invert;
        return invert;
    }
    #endregion

    public delegate bool CompareStatement<T>(T value, T allOthers);
    public static T CompareAndSelect<T>(this IEnumerable<T> values, CompareStatement<T> statement)
    {
        var o = values.GetEnumerator().Current;
        foreach(T t in values)
        {
            if (!statement(o, t)) o = t;
        }
        return o;
    }
}
