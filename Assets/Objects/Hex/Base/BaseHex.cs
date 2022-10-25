using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHex : Hex
{

    [SerializeField]
    private Player.ETeam _team;

    public Player.ETeam Team => _team;


}
