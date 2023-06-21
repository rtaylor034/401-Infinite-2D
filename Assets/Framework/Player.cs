using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player
{

    public Team Team { get; private set; }
    public List<ManualAction> ManualActions => new(_manualActions);
    private List<ManualAction> _manualActions = new();
    public int Energy { get; private set; } = 0;
    public int ControlSpheres { get; private set; } = 0;

    public Player(Team team)
    {
        Team = team;
    }


    /// <summary>
    /// Sets this Players's Energy to <paramref name="val"/>. (Should only be called from <see cref="GameAction"/>[ : ])
    /// </summary>
    /// <param name="val"></param>
    public void UpdateEnergy(int val)
    {
        Energy = val;
    }
    /// <summary>
    /// Sets this Players's Control Spheres to <paramref name="val"/>. (Should only be called from <see cref="GameAction"/>[ : ])
    /// </summary>
    /// <param name="val"></param>
    public void UpdateControlSpheres(int val)
    {
        ControlSpheres = val;
    }
    public void UpdateManualActions(List<ManualAction> val)
    {
        _manualActions = val;
    }

    public HashSet<Unit> GetAllyUnits(Board board) => GetAllyUnits(board.Wrapped());
    public HashSet<Unit> GetAllyUnits(IEnumerable<Board> boards) => GetUnits(boards, true);
    public HashSet<Unit> GetEnemyUnits(Board board) => GetEnemyUnits(board.Wrapped());
    public HashSet<Unit> GetEnemyUnits(IEnumerable<Board> boards) => GetUnits(boards, false);

    private HashSet<Unit> GetUnits(IEnumerable<Board> boards, bool allies)
    {
        HashSet<Unit> o = new();
        foreach (Board board in boards)
            o.UnionWith(board.Units.Where(u => (u.Team == Team) == allies));
        return o;
    }
    /// <summary>
    /// A dummy <see cref="Player"/> object with no behavior.
    /// </summary>
    public static Player DummyPlayer => new Player(new("NONE", Color.white, 0));

    //Player class stores a player's Abilities, Passive, Control spheres, etc.
    //Under normal circumstances there will only be 2 player instances (Blue player and Red player).
    //Players are *not* Units. Units are the peices that move across the board that have HP, status effects, etc. , while Player objects hold information about the entire side, such as the cards, passive, etc. a player has.
    public override string ToString()
    {
        return $"P*{Team}({Energy})";
    }

    
}
