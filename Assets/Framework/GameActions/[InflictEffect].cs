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
        public Unit AffectedUnit { get; private set; }

        /// <summary>
        /// Occurs when any <see cref="InflictEffect"/> is created.
        /// </summary>
        /// <remarks><inheritdoc cref="__DOC__ExternalResultantEvent"/></remarks>
        public static event GameActionEventHandler<InflictEffect> ExternalResultantEvent;
        protected override void InternalPerform()
        {
            Effect.SetActive(true, AffectedUnit);
        }

        protected override void InternalUndo()
        {
            Effect.SetActive(false, AffectedUnit);
        }

        public InflictEffect(Player performer, UnitEffect inflictedEffect, Unit affectedUnit) : base(performer)
        {
            Effect = inflictedEffect;
            AffectedUnit = affectedUnit;
            ExternalResultantEvent?.Invoke(this);
        }

        public override string ToString()
        {
            return $"<INFLICT EFFECT> {Effect} -> {AffectedUnit}" + base.ToString();
        }
    }

    
}