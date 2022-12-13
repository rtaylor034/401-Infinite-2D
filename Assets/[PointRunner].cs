using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public partial class Passive
{
    
    /// <summary>
    /// [ : ] <see cref="Passive"/>
    /// </summary>
    public class PointRunner : Passive
    {
        public PointRunner(string name) : base(name) { }

        protected override void InternalSetup(bool val)
        {
            if (val)
            {
                GameAction.OnEvaluationEvent += Effect;
            } else
            {
                GameAction.OnEvaluationEvent += Effect;
            }
        }

        private async Task Effect(GameAction action)
        {
            if (action is not GameAction.Move move || action.Performer != EmpoweredPlayer) return;

            var u = move.MovedUnit;
            if (u.Team != action.Performer.Team) return;

            foreach(Hex hex in u.Board.HexesAt(move.ToPos.GetAdjacent()))
            {
                if (hex.Occupant == null) continue;
                if (hex.Occupant.Team == u.Team)
                {
                    await GameAction.Declare(await GameAction.Move.Prompt(
                        new GameAction.Move.PromptArgs.Pathed(EmpoweredPlayer, u, 2)));
                }
            }

        }
    }

}
