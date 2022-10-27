using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//ABSTRACT (Must instantiate a derived class)
public abstract class Hex : MonoBehaviour, ISelectable
{
    //position in board coordinates.
    public Vector3Int Position { get; private set; }

    public Unit Occupant { get; set; } = null;

    public virtual bool IsBlocker => false;

    public Hex Init(Vector3Int pos)
    {
        Position = pos;
        return this;
    }

    public virtual void TestMethod()
    {
        Debug.Log($"I am a {GetType().Name} hex at coordinate {Position}.");
    }

    #region ISelectable
    //empty for now
    public void ToggleSelectable(bool toggle) { }
    public void ToggleHovered(bool toggle) { }

    #endregion

}
