using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public static class AbilityRegistry
{

    public static ReadOnlyCollection<ConstructorTemplate<Ability>> Registry { get; private set; }
    
    static AbilityRegistry()
    {
        /*
        * Sourced:
        * NAME - string
        * TYPE IDENTITY - Ability.ETypeIdentity
        * TARGET EFFECTS - ConstructorTemplate<UnitEffect>[]
        * HIT AREA - HashSet<Vector3Int>
        * ON-PLAY FOLLOWUP METHOD - Ability.PlayAction (void<GameAction.PlayAbility>)
        * TARGETING CONDITIONS - Ability.Sourced.TargetingCondition[]
        * (defaulted) SOURCE CONDITIONS - Ability.Sourced.SourceCondition[] = new[] {STANDARD_VALID_SOURCE}
        * 
        * Unsourced:
        * NAME - string
        * TYPE IDENTITY - Ability.ETypeIdentity
        * ON-PLAY ACTION - Ability.PlayAction (void<GameAction.PlayAbility>)
        * INITIAL TARGET CONDITION - Ability.Unsourced.SingleTargetCondition
        * (May be excluded) SECONDARY TARGET CONDITIONS  - Ability.Unsourced.TargetCondition[]
        */

        List<ConstructorTemplate<Ability>> masterList = new()
        {
            //>0 TEST ATTACK
            new
            (
                typeof(Ability.Sourced),

                "Test Attack", Ability.ETypeIdentity.Attack,

                new ConstructorTemplate<UnitEffect>[]
                {
                    new(typeof(UnitEffect.Slow), 1)
                },
                new HashSet<Vector3Int>
                {
                    BoardCoords.up
                },

                new Ability.PlayAction(a =>
                {
                    GameAction.Move.Prompt(new GameAction.Move.PromptArgs.Pathed
                        (a.Performer, a.ParticipatingUnits[1], 8) 
                        {
                            Directionals =
                            (GameAction.Move.PromptArgs.Pathed.EDirectionalsF.Away,
                            a.ParticipatingUnits[0].Position),

                            MinDistance = 3,
                            Forced = true
                        },
                        move => a.AddLateResultant(move));
                }),

                new Ability.Sourced.TargetingCondition[]
                {
                    Ability.Sourced.STANDARD_ATTACK_TARGET,
                    Ability.Sourced.STANDARD_COLLISION
                }
            ),

            //>1 TEST UTILITY
            new
            (
                typeof(Ability.Unsourced),

                "Test Utility", Ability.ETypeIdentity.Utility,

                new Ability.PlayAction(a =>
                {
                    GameAction.Move.Prompt(new GameAction.Move.PromptArgs.Pathed
                        (a.Performer, a.ParticipatingUnits[0], 8),
                        move => a.AddLateResultant(move));
                }),

                new Ability.Unsourced.SingleTargetCondition((p, u) => p.Team == u.Team)
            ),
        };


        //FINALIZE REGISTRY
        Registry = new ReadOnlyCollection<ConstructorTemplate<Ability>>(masterList);
    }

}
