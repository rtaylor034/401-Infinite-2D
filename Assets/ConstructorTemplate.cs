using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using UnityEngine;

public class ConstructorTemplate<T>
{

    private readonly System.Type _type;
    private readonly object[] _params;
    private readonly System.Reflection.ConstructorInfo _constructor;

    /// <summary>
    /// </summary>
    /// <param name="derivedType"></param>
    /// <param name="parameters"></param>
    /// <exception cref="System.ArgumentException"></exception>
    /// <exception cref="System.Exception"></exception>
    public ConstructorTemplate(System.Type derivedType, params object[] parameters)
    {
        _type = derivedType;
        _params = parameters;

        System.Type[] types = new System.Type[_params.Length];
        for (int i = 0; i < types.Length; i++) types[i] = _params[i].GetType();

        _constructor = _type.GetConstructor(types);

        //exceptions 
        if (_constructor == null)
            throw new System.ArgumentException($"{derivedType.Name} does not have a constructor with that takes parameters: ({string.Join(",", types.ToList())})");

        if (!typeof(T).IsAssignableFrom(_type))
            throw new System.Exception($"{derivedType.Name} does not inherit from {typeof(T).Name}");

    }

    /// <summary>
    /// Constructs a <typeparamref name="T"/> object based on this template.
    /// </summary>
    public T CreateInstance()
    {
        return (T)_constructor.Invoke(_params);
    }

}
