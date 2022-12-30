using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public partial class UnitEffect
{
    //Shield is weird, its behavior code is implemented in UnitEffect.Damage, it also cannot have a duration more than 1
    public class Shield : UnitEffect
    {
        //Parameter does nothing, only exists for consistency with other effects
        public Shield(int _) : base(0) { }
        protected override void InternalSetup(bool val)
        {
            if (val)
            {
                //handled in UnitEffect.Damage
            }
            else
            {
                //handled in UnitEffect.Damage
            }
        }

    }
}
