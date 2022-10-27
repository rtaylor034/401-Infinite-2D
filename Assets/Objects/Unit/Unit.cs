using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{

    public int HP { get; private set; }
    public Player.ETeam Team { get; private set; }
    public int MaxHP { get; private set; }
    public int ID { get; private set; }

    public Vector3Int Position { get; private set; }

    private static int _idCount = 0;

    public Unit Init(int maxhp, Player.ETeam team, Vector3Int pos)
    {
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

}
