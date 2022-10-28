using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallHex : Hex
{

    public override bool BlocksPathing => true;
    public override bool BlocksTargeting => true;
    public override bool IsOccupiable => false;


}
