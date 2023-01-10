using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Unit : Selectable, ITeamable
{

    public int HP { get; private set; }
    public Team Team { get; private set; }
    public int MaxHP { get; private set; }
    public int ID { get; private set; }

    public Vector3Int Position { get; private set; }
    /// <summary>
    /// The Board that this Unit resides on.
    /// </summary>
    /// <remarks>
    /// <i>Future-proof for multi-board gameplay(?)</i>
    /// </remarks>
    public Board Board => _board;
    /// <summary>
    /// All Units that are on the same Team as this <see cref="Unit"/> (including itself).
    /// </summary>
    /// <remarks>
    /// <c>=> _board.Units.Where(u => u.Team == Team)</c>
    /// </remarks>
    public IEnumerable<Unit> Allies => _board.Units.Where(u => u.Team == Team);
    /// <summary>
    /// All Units that are not on the same Team as this <see cref="Unit"/>.
    /// </summary>
    /// <remarks>
    /// <c>=> _board.Units.Where(u => u.Team != Team)</c>
    /// </remarks>
    public IEnumerable<Unit> Enemies => _board.Units.Where(u => u.Team != Team);

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

    /// <summary>
    /// <b>[MUST BE CALLED AFTER INSTANTIATION]</b> (<see cref="Object.Instantiate(Object)>"/>)
    /// </summary>
    /// <param name="board"></param>
    /// <param name="maxhp"></param>
    /// <param name="team"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Unit Init(Board board, int maxhp, Vector3Int pos)
    {
        _board = board;
        Position = pos;
        MaxHP = maxhp;
        HP = MaxHP;
        ID = ++_idCount;
        return this;
    }

    public void SetTeam(Team team)
    {
        Team = team;
        GetComponent<SpriteRenderer>().color = team.Colors.Unit;
    }
    public void TestMethod()
    {
        Debug.Log($"I am unit {ID} with {HP} HP.");
    }

    public override string ToString()
    {
        return $"[U{ID}{Team.ToString()[0]}:{HP}]";
    }
}
