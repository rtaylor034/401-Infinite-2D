using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ABSTRACT (Must instantiate a derived class)
public abstract class Hex : MonoBehaviour
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
        Debug.Log($"I am a hex at coordinate {Position}.");
    }



}
