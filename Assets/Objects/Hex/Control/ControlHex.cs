using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// [ : ] <see cref="Hex"/>
/// </summary>
public class ControlHex : Hex
{

    public bool IsLocked { get
        {
            HashSet<Player.ETeam> teams = new();
            foreach (Hex hex in _board.HexDict.Values)
            {
                if (hex is not ControlHex chex) continue;
                if (chex.Occupant is not null && !teams.Add(chex.Occupant.Team)) return true;
            }
            return false;
        } }

}
