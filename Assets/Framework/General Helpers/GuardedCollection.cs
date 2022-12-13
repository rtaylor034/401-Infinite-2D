using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardedCollection<T>
{
    private ICollection<T> _collection;
    
    public GuardedCollection(ICollection<T> collection)
    {
        _collection = collection;
    }

    public static GuardedCollection<T> operator +(GuardedCollection<T> col, T element)
    {
        col._collection.Add(element);
        return col;
    }

    public static GuardedCollection<T> operator -(GuardedCollection<T> col, T element)
    {
        col._collection.Remove(element);
        return col;
    }

}
