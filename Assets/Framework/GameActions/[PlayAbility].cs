using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.Search;
using UnityEngine;

public partial class GameAction
{

    public class PlayAbility : GameAction
    {
        /// <summary>
        /// The <see cref="Ability"/> that was played.
        /// </summary>
        public Ability PlayedAbility { get; private set; }

        /// <summary>
        /// The Units that participated in this ability.
        /// </summary>
        /// <remarks>
        /// <i>
        /// SOURCED: Always 2 elements, [0] is the Source, [1] is the Target. <br></br>
        /// UNSOURCED: Size is ability-dependent, contains the Targets of the ability in order of selection.
        /// </i>
        /// </remarks>
        public Unit[] ParticipatingUnits { get; private set; }

        /// <summary>
        /// Occurs when any <see cref="PlayAbility"/> is prompted using <see cref="Prompt(PromptArgs, Action{PlayAbility}, Selector.SelectionConfirmMethod)"/>. <br></br>
        /// </summary>
        /// <remarks>
        /// <i>Modifications to the <see cref="PromptArgs"/> will be applied to the Prompt() call.</i>
        /// </remarks>
        public static event Action<PromptArgs> OnPromptEvent;
        protected override void InternalPerform()
        {
            //Everything is just a resultant gameaction, so no need for internal performs
        }

        protected override void InternalUndo()
        {
            //Undo is automatic (see InternalPerform())
        }

        protected override async Task InternalEvaluate()
        {
            await CreateAbilityResultants();
        }
        /// <summary>
        /// Plays <paramref name="ability"/>, with <paramref name="participants"/> as the participating Units, by <paramref name="performer"/>. <br></br> <br></br>
        /// > Unless you are creating a <see cref="PlayAbility"/> that has already happened, use <br></br>
        /// <b><see cref="Prompt(PromptArgs, Action{PlayAbility}, Selector.SelectionConfirmMethod)"/></b>.
        /// </summary>
        /// <remarks>
        /// <i><see cref="PlayAbility"/> object is created within Prompt()</i>
        /// </remarks>
        /// <param name="performer"></param>
        /// <param name="ability"></param>
        /// <param name="participants"></param>
        public PlayAbility(Player performer, Ability ability, IList<Unit> participants) : base(performer)
        {
            PlayedAbility = ability;
            ParticipatingUnits = new Unit[participants.Count];
            for (int i = 0; i < participants.Count; i++) ParticipatingUnits[i] = participants[i];
        }

        /// <summary>
        /// Creates a <see cref="PlayAbility"/> action based on <paramref name="args"/>. <br></br>
        /// > Calls <paramref name="confirmCallback"/> with the created <see cref="PlayAbility"/> once all selections are made. <br></br>
        /// > If any selection is cancelled or invalid, <paramref name="cancelCallback"/> will be called with the invalid <see cref="Selector.SelectorArgs"/> instead.
        /// </summary>
        /// <remarks>
        /// <i>(See <see cref="PromptArgs"/>)</i>
        /// </remarks>
        /// <param name="args"></param>
        /// <param name="confirmCallback"></param>
        /// <param name="cancelCallback"></param>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<PlayAbility> Prompt(PromptArgs args, Action<Selector.SelectionArgs> cancelCallback = null)
        {
            var ability = args.Ability;
            var board = args.Board;
            var player = args.Performer;
            return await __Prompt(true);
            
            //for consistency with Move.Prompt, incase there is ever a forced ability action.
            async Task<PlayAbility> __Prompt(bool callPromptEvent)
            {
                if (callPromptEvent) OnPromptEvent?.Invoke(args);

                if (ability is Ability.Sourced sourced) return await __HandleSourced(sourced);
                else if (ability is Ability.Unsourced unsourced) return await __HandleUnsourced(unsourced);
                else throw new ArgumentException("Ability not recognized");

            }
            async Task<PlayAbility> __HandleSourced(Ability.Sourced a)
            {
                var validSources = board.Units.Where(u =>
                {
                    foreach (var condition in a.SourceConditions)
                        if (!condition(player, u)) return false;
                    return true;
                });
                
                var sourceSel = await GameManager.SELECTOR.Prompt(validSources);
            
                if (sourceSel.Selection is not Unit source) { cancelCallback?.Invoke(sourceSel); return null; }

                var validTargets = board.Units.Where(u =>
                {
                    if (!a.HitArea.Offset(source.Position).Rotate(source.Position, player.PerspectiveRotation)
                    .Contains(u.Position)) return false;
                    foreach (var condition in a.TargetingConditions)
                        if (!condition(player, source, u)) return false;
                    return true;
                });

                var targetSel = await GameManager.SELECTOR.Prompt(validTargets);

                if (targetSel.Selection is not Unit target)
                { cancelCallback?.Invoke(targetSel); return null; }

                var participants = new Unit[] { source, target };
                return new PlayAbility(player, a, participants);
            }

            async Task<PlayAbility> __HandleUnsourced(Ability.Unsourced a)
            {
                var conds = a.TargetConditions;
                var pUnits = new Unit[a.TargetConditions.Count];
                return await __PromptTarget(0);

                async Task<PlayAbility> __PromptTarget(int i)
                {
                    var validUnits = board.Units.Where(u => conds[i](player, (i > 0) ? pUnits[i-1] : null, u));
                    var sel = await GameManager.SELECTOR.Prompt(validUnits);

                    if (sel.Selection is not Unit u)
                    { cancelCallback?.Invoke(sel); return null; }

                    pUnits[i] = u;

                    if (++i <= pUnits.Length)
                        return new PlayAbility(player, a, pUnits);

                    return await __PromptTarget(i);
                
                }
            }
        }
        
        private async Task CreateAbilityResultants()
        {
            if (PlayedAbility is Ability.Sourced sourced)
            {
                foreach (Ability.PlayAction playAction in sourced.FollowUpMethod.GetInvocationList())
                    await playAction(this);
                //DEVNOTE: may create excessive UnitEffect objects, not really sure what to do about that.
                foreach (var effectC in sourced.TargetEffects)
                {
                    //realistically should only have 1 target (ParticipatingUnits[1]), but this is multitarget support for no reason.
                    for (int i = 1; i < ParticipatingUnits.Length; i++)
                        await AddResultant(new InflictEffect(Performer, effectC.CreateInstance(), ParticipatingUnits[i]));
                }

            }
            else if (PlayedAbility is Ability.Unsourced unsourced)
            {
                foreach (Ability.PlayAction playAction in unsourced.ActionMethod.GetInvocationList())
                    await playAction(this);
            }
            else throw new ArgumentException("Ability not recognized");
        }

        public class PromptArgs
        {
            /// <summary>
            /// The Player that is performing this ability.
            /// </summary>
            public Player Performer { get; set; }
            /// <summary>
            /// The Ability that is being played.
            /// </summary>
            public Ability Ability { get; set; }
            /// <summary>
            /// The Board that this ability is taking place on. <br></br>
            /// <i>(Usually can be referenced by <see cref="Unit.Board"/> or <see cref="GameManager.board"/>) <br></br>
            /// (Only exists because Ryan wants to overengineer everything and have future multi-board support)</i>
            /// </summary>
            public Board Board { get; set; }

            /// <summary>
            /// Prompts <paramref name="performer"/> to play an instance of <paramref name="abilityConstruction"/> on Board <paramref name="board"/>.
            /// </summary>
            /// <remarks>
            /// <i>(See <see cref="AbilityRegistry.Registry"/>)</i>
            /// </remarks>
            /// <param name="performer"></param>
            /// <param name="abilityConstruction"></param>
            /// <param name="board"></param>
            public PromptArgs(Player performer, ConstructorTemplate<Ability> abilityConstruction, Board board)
            {
                Performer = performer;
                Board = board;
                Ability = abilityConstruction.CreateInstance();
            }
        }

        public override string ToString()
        {
            return $"<ABILITY> {PlayedAbility.Name}: {string.Join(" -> ", ParticipatingUnits as IEnumerable<Unit>)}" + base.ToString();
        }
    }


}
