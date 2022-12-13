using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <b>abstract</b>
/// </summary>
public abstract partial class Passive
{
    public Player EmpoweredPlayer { get; private set; }
    public string Name { get; set; }

    protected Passive(string name)
    {
        Name = name;
    }

    public void SetActive(bool val, Player empoweredPlayer)
    {
        EmpoweredPlayer = empoweredPlayer;
        InternalSetup(val);
    }
    /// <summary>
    /// <b>[abstract]</b> <br></br>
    /// Called when <see cref="SetActive(bool, Player)"/> is called with its <paramref name="val"/>. <br></br>
    /// > Used to subscribe/unsubscribe the appropriate methods for each derivation[ : ] of <see cref="Passive"/>
    /// </summary>
    /// <param name="val"></param>
    protected abstract void InternalSetup(bool val);
}
