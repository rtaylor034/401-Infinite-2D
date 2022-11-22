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
        public UnitEffect Effect { get; private set; }
        public static event GameActionEventHandler<InflictEffect> OnPerform;
        protected override void InternalPerform()
        {
            Effect.SetActive(true);
            OnPerform?.Invoke(this);
        }

        protected override void InternalUndo()
        {
            Effect.SetActive(false);
        }

        public InflictEffect(Player performer, UnitEffect inflictedUnit) : base(performer)
        {
            Effect = inflictedUnit;
        }
    }


}