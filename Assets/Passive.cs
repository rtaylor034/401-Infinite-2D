using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <b>abstract</b>
/// </summary>
public abstract partial class Passive
{

    public string Name { get; set; }

    protected Passive(string name)
    {
        Name = name;
    }
}
