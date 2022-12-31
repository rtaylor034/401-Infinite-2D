using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// [ : ] <see cref="Hex"/>
/// </summary>
public class BaseHex : Hex
{

    [SerializeField]
    private Player.ETeam _team;

    public Player.ETeam Team => _team;

    public bool IsGuarded
    {
        get
        {
            foreach (Hex hex in _board.HexDict.Values)
            {
                if (hex is not BaseHex bhex) continue;
                if (bhex.Occupant is not null && bhex.Occupant.Team == _team) return true;
            }
            return false;
        }
    }

}
