using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using UnityEngine;

public class ConstructorTemplate<Base>
{

    private readonly System.Type _type;
    private readonly object[] _params;
    private readonly System.Reflection.ConstructorInfo _constructor;

    public ConstructorTemplate(System.Type derivedType, params object[] parameters)
    {
        _type = derivedType;
        _params = parameters;

        System.Type[] types = new System.Type[_params.Length];
        for (int i = 0; i < types.Length; i++) types[i] = _params[i].GetType();

        _constructor = _type.GetConstructor(types);

        if (_constructor == null)
            throw new System.ArgumentException($"{derivedType.Name} does not have a constructor with that takes parameters: ({string.Join(",", types.ToList())})");

        if (!derivedType.BaseType.IsEquivalentTo(typeof(Base)))
            throw new System.Exception($"{derivedType.Name} does not inherit from {typeof(Base).Name}");

    }

    public Base CreateInstance()
    {
        return (Base)_constructor.Invoke(_params);
    }

}
