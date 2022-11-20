using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//ABSTRACT (Must instantiate a derived class)
public abstract class Hex : Selectable
{
    //position in board coordinates.
    public Vector3Int Position { get; private set; }

    public virtual bool IsOccupiable => Occupant == null;
    public virtual bool BlocksPathing => false;
    public virtual bool BlocksTargeting => false;

    public Unit Occupant { get; set; } = null;
    public Board Board => _board;

    protected Board _board;

    public Hex Init(Board board, Vector3Int pos)
    {
        _board = board;
        Position = pos;
        return this;
    }

    public virtual void TestMethod()
    {
        Debug.Log($"I am a {GetType().Name} hex at coordinate {Position}.");
    }

    
}
