using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{

    public ETeam Team { get; private set; }
    public int Energy { get; private set; } = 0;
    public int PerspectiveRotation => PerspectiveRotationOf(Team);

    public Player(ETeam team)
    {
        Team = team;
    }
    //main team enum
    public enum ETeam : byte
    {
        NONE,
        Blue,
        Red
    }

    /// <summary>
    /// Gets the amount of axis rotations it takes to match a team's perspective. <br></br>
    /// > Meant to be used with <see cref="BoardCoords.Rotate(Vector3Int, Vector3Int, int)"/>.
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public static int PerspectiveRotationOf(ETeam team)
    {
        return team switch
        {
            ETeam.Blue => 0,
            ETeam.Red => 3,
            _ => 0
        };
    }

    /// <summary>
    /// Sets this Players's Energy to <paramref name="val"/>. (Should only be called from <see cref="GameAction"/>[ : ])
    /// </summary>
    /// <param name="val"></param>
    public void UpdateEnergy(int val)
    {
        Energy = val;
    }

    public static Player DummyPlayer => new Player(ETeam.NONE);

    //Player class stores a player's Abilities, Passive, Control spheres, etc.
    //Under normal circumstances there will only be 2 player instances (Blue player and Red player).
    //Players are *not* Units. Units are the peices that move across the board that have HP, status effects, etc. , while Player objects hold information about the entire side, such as the cards, passive, etc. a player has.
    public override string ToString()
    {
        return $"P*{Team}";
    }
}
