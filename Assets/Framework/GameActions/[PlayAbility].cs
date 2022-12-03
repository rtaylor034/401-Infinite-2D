using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Search;
using UnityEngine;

public partial class GameAction
{

    public class PlayAbility : GameAction
    {
        public Ability PlayedAbility { get; private set; }
        public Unit[] ParticipatingUnits { get; private set; }

        /// <summary>
        /// Occurs when any <see cref="PlayAbility"/> is created.
        /// </summary>
        /// <remarks><inheritdoc cref="__DOC__ExternalResultantEvent"/></remarks>
        public static event GameActionEventHandler<PlayAbility> ExternalResultantEvent;

        public static event Action<PromptArgs> OnPromptEvent;
        protected override void InternalPerform()
        {
            if (PlayedAbility is Ability.Sourced sourced)
            {
                sourced.FollowUpMethod?.Invoke(this);
                //DEVNOTE: may create excessive UnitEffect objects, not really sure what to do about that.
                foreach (var effectC in sourced.TargetEffects)
                {
                    //realistically should only have 1 target (ParticipatingUnits[1]), but this is multitarget support for no reason.
                    for (int i = 1; i < ParticipatingUnits.Length; i++)
                        AddResultant(new InflictEffect(Performer, effectC.CreateInstance(), ParticipatingUnits[i]));
                }

            }
            else if (PlayedAbility is Ability.Unsourced unsourced)
            {
                unsourced.ActionMethod?.Invoke(this);
            }
        }

        protected override void InternalUndo()
        {
            //All performs are resultant gameactions, therefor no internal undo is necessary.
        }

        public PlayAbility(Player performer, Ability ability, IList<Unit> participants) : base(performer)
        {
            PlayedAbility = ability;
            ParticipatingUnits = new Unit[participants.Count];
            for (int i = 0; i < participants.Count; i++) ParticipatingUnits[i] = participants[i];
            ExternalResultantEvent?.Invoke(this);
        }

        public static void Prompt(PromptArgs args, Action<PlayAbility> confirmCallback, Selector.SelectionConfirmMethod cancelCallback = null)
        {
            var ability = args.Ability;
            var board = args.Board;
            var player = args.Performer;
            __Prompt(true);
            
            //for consistency with Move.Prompt, incase there is ever a forced ability action.
            void __Prompt(bool callPromptEvent)
            {
                if (callPromptEvent) OnPromptEvent?.Invoke(args);

                if (ability is Ability.Sourced sourced) __HandleSourced(sourced);
                else if (ability is Ability.Unsourced unsourced) __HandleUnsourced(unsourced);
                else throw new ArgumentException("Ability not recognized");

            }
            void __HandleSourced(Ability.Sourced a)
            {
                var validSources = board.Units.Where(u =>
                {
                    foreach (var condition in a.SourceConditions)
                        if (!condition(player, u)) return false;
                    return true;
                });
                
                GameManager.SELECTOR.Prompt(validSources, __SourceConfirm);

                void __SourceConfirm(Selector.SelectorArgs sourceSel)
                {
                    if (sourceSel.Selection is not Unit source) { cancelCallback?.Invoke(sourceSel); return; }

                    var validTargets = board.Units.Where(u =>
                    {
                        if (!a.HitArea.Offset(source.Position).Rotate(source.Position, player.PerspectiveRotation)
                        .Contains(u.Position)) return false;
                        foreach (var condition in a.TargetingConditions)
                            if (!condition(player, source, u)) return false;
                        return true;
                    });

                    GameManager.SELECTOR.Prompt(validTargets, __TargetConfirm);

                    void __TargetConfirm(Selector.SelectorArgs targetSel)
                    {
                        if (targetSel.Selection is not Unit target) { cancelCallback?.Invoke(targetSel); return; }

                        var participants = new Unit[] { source, target };
                        confirmCallback?.Invoke(new PlayAbility(player, a, participants));
                    }

                }


            }
            void __HandleUnsourced(Ability.Unsourced a)
            {
                throw new NotImplementedException();
            }
        }
        

        public class PromptArgs
        {
            public Player Performer { get; set; }
            public Ability Ability { get; set; }
            public Board Board { get; set; }

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
