using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{

    public int HP { get; private set; }
    public int MaxHP { get; private set; }
    public int ID { get; private set; }

    private static int _idCount = 0;

    public Unit Init(int maxhp)
    {
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
