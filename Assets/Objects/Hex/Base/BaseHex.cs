using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHex : Hex
{

    [SerializeField]
    private Player.ETeam _team;

    public Player.ETeam Team => _team;

    //TBI
    public bool IsGuarded
    {
        get
        {
            HashSet<BaseHex> result = new();
            foreach (Hex hex in _board.HexDict.Values)
                if (hex is BaseHex b) result.Add(b);

            foreach (BaseHex bhex in result)
                if (bhex.Occupant is not null && bhex.Occupant.Team == _team) return true;
            return false;
        }
    }

}
