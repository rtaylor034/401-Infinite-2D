using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallHex : Hex
{

    //Walls are blockers, so they override the default Hex behavior.
    public override bool IsBlocker => true;

    //overrides TestMethod because why not
    public override void TestMethod()
    {
        Debug.Log($"I am a WALL at coordinate {Position}");

    }

}
