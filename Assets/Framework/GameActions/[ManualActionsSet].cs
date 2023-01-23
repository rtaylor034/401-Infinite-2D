using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameAction
{

    /// <summary>
    /// [ : ] <see cref="GameAction"/>
    /// </summary>
    public class ManualActionsSet : GameAction
    {
        public Player AffectedPlayer { get; private set; }
        public List<ManualAction> FromList { get; private set; }
        public List<ManualAction> ToList { get; private set; }

        protected override void InternalPerform() => AffectedPlayer.UpdateManualActions(ToList);
        protected override void InternalUndo() => AffectedPlayer.UpdateManualActions(FromList);

        public ManualActionsSet(Player affectedPlayer, IList<ManualAction> actionList, Player performer) : base(performer)
        {
            AffectedPlayer = affectedPlayer;
            FromList = affectedPlayer.ManualActions;
            ToList = new(actionList);
        }
        public override string ToString()
        {
            return $"<MA SET> {{{string.Join(", ", FromList)}}} -> {{{string.Join(", ", ToList)}}}" + base.ToString();
        }
    }
}