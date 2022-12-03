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
        /// <summary>
        /// The inflicted <see cref="UnitEffect"/>.
        /// </summary>
        public UnitEffect Effect { get; private set; }
        
        /// <summary>
        /// The <see cref="Unit"/> that was inflicted with Effect.
        /// </summary>
        public Unit AffectedUnit { get; private set; }

        /// <summary>
        /// Occurs when any <see cref="InflictEffect"/> is created.
        /// </summary>
        /// <remarks><inheritdoc cref="__DOC__ExternalResultantEvent"/></remarks>
        public static event GameActionEventHandler<InflictEffect> ExternalResultantEvent;
        protected override void InternalPerform()
        {
            Effect.SetActive(true, AffectedUnit, Performer);
        }

        protected override void InternalUndo()
        {
            Effect.SetActive(false, AffectedUnit, Performer);
        }

        /// <summary>
        /// Inflicts <paramref name="affectedUnit"/> with <paramref name="inflictedEffect"/>, by <paramref name="performer"/>.
        /// </summary>
        /// <param name="performer"></param>
        /// <param name="inflictedEffect"></param>
        /// <param name="affectedUnit"></param>
        public InflictEffect(Player performer, UnitEffect inflictedEffect, Unit affectedUnit) : base(performer)
        {
            Effect = inflictedEffect;
            AffectedUnit = affectedUnit;
            ExternalResultantEvent?.Invoke(this);
        }

        public override string ToString()
        {
            return $"<EFFECT> {Effect} -> {AffectedUnit}" + base.ToString();
        }
    }

    
}