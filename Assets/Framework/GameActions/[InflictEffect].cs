using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameAction
{

    /// <summary>
    /// [ : ] <see cref="GameAction"/>
    /// </summary>
    public class InflictEffect : GameAction
    {
        protected override void InternalPerform()
        {
            throw new System.NotImplementedException();
        }

        protected override void InternalUndo()
        {
            throw new System.NotImplementedException();
        }

        public InflictEffect(Player performer, Unit inflictedUnit, UnitEffect effect) : base(performer)
        {

        }
    }


}