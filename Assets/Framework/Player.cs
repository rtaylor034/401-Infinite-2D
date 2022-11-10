using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{

    public ETeam Team { get; private set; }
    public int Energy { get; set; } = 0;
    
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

    public static Player DummyPlayer => new Player(ETeam.NONE);

    //Player class stores a player's Abilities, Passive, Control spheres, etc.
    //Under normal circumstances there will only be 2 player instances (Blue player and Red player).
    //Players are *not* Units. Units are the peices that move across the board that have HP, status effects, etc. , while Player objects hold information about the entire side, such as the cards, passive, etc. a player has.
    public override string ToString()
    {
        return $"*{Team}*";
    }
}
