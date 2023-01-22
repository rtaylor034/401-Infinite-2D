using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using UnityEngine;

//[DEPRICATED] (See ConstructionTemplate in "Delegates" file)
/*
public class ConstructorTemplate<T>
{
    public System.Type ConstructionType => _type;
    public System.Type ReturnType => typeof(T);
    private readonly System.Type _type;
    private readonly object[] _params;
    private readonly System.Reflection.ConstructorInfo _constructor;

    /// <summary>
    /// A template for a constructor of type <paramref name="constructionType"/> with specified <paramref name="parameters"/>.
    /// </summary>
    /// <remarks>
    /// A new instance can be created with this constructor using <see cref="CreateInstance"/>. <br></br>
    /// The new instance will be cast to <typeparamref name="T"/>.
    /// </remarks>
    /// <param name="constructionType"></param>
    /// <param name="parameters"></param>
    /// <exception cref="System.ArgumentException"></exception>
    /// <exception cref="System.Exception"></exception>
    public ConstructorTemplate(System.Type constructionType, params object[] parameters)
    {
        _type = constructionType;
        _params = parameters;

        System.Type[] types = new System.Type[_params.Length];
        for (int i = 0; i < types.Length; i++) types[i] = _params[i].GetType();

        _constructor = _type.GetConstructor(types);

        //exceptions 
        if (_constructor == null)
            throw new System.ArgumentException($"{constructionType.Name} does not have a constructor with that takes parameters: ({string.Join(" | ", types.ToList())})");
        if (!typeof(T).IsAssignableFrom(_type))
            throw new System.Exception($"{constructionType.Name} cannot be converted to {typeof(T).Name}");

    }

    /// <summary>
    /// Constructs an object based on this constructor template.
    /// </summary>
    /// <remarks>
    /// <c>new (<typeparamref name="T"/>)&lt;ConstructionType&gt;(&lt;params&gt;);</c>
    /// </remarks>
    public T CreateInstance()
    {
        return (T)_constructor.Invoke(_params);
    }

}
*/
