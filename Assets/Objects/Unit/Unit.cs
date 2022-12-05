using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : Selectable
{
    //Change

    public int HP { get; private set; }
    public Player.ETeam Team { get; private set; }
    public int MaxHP { get; private set; }
    public int ID { get; private set; }

    public Vector3Int Position { get; private set; }
    public Board Board => _board;

    protected Board _board;

    private static int _idCount = 0;
    
    
    /// <summary>
    /// Updates this Unit's position on the board. (Should only be called from <see cref="GameAction"/>[ : ])
    /// </summary>
    /// <param name="pos"></param>
    public void UpdatePosition(Vector3Int pos)
    {
        _board.HexAt(Position).Occupant = null;
        Position = pos;
        _board.HexAt(pos).Occupant = this;
        transform.localPosition = _board.GetLocalTransformAt(Position, -1);
    }

    /// <summary>
    /// Sets this Unit's HP to <paramref name="val"/>. (Should only be called from <see cref="GameAction"/>[ : ])
    /// </summary>
    /// <param name="val"></param>
    public void UpdateHP(int val)
    {
        HP = val;
    }

    public Unit Init(Board board, int maxhp, Player.ETeam team, Vector3Int pos)
    {
        _board = board;
        Position = pos;
        Team = team;
        MaxHP = maxhp;
        HP = MaxHP;
        ID = ++_idCount;
        return this;
    }

    public void TestMethod()
    {
        Debug.Log($"I am unit {ID} with {HP} HP.");
    }

    public override string ToString()
    {
        return $"[U{ID}{Team.ToString()[0]}-{HP}]";
    }
}
