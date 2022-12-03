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
        * INITIAL TARGET CONDITION - Ability.Unsourced.SingleTargetCondition
        * (May be excluded) SECONDARY TARGET CONDITIONS  - Ability.Unsourced.TargetCondition[]
        * ON-PLAY ACTION - Ability.PlayAction (void<GameAction.PlayAbility>)
        */

        List<ConstructorTemplate<Ability>> masterList = new()
        {
            //>0 TEST ATTACK
            new
            (
                typeof(Ability.Sourced),

                "Lance", Ability.ETypeIdentity.Attack,

                new ConstructorTemplate<UnitEffect>[]
                {
                    new(typeof(UnitEffect.Slow), 1)
                },
                new HashSet<Vector3Int>
                {
                    H(0, 1, 0),
                    H(0, 2, 0),
                    H(0, 3, 0)
                },

                new Ability.PlayAction(a =>
                {
                    GameAction.Move.Prompt(new GameAction.Move.PromptArgs.Pathed
                        (a.Performer, a.ParticipatingUnits[0], 1),
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

                "Break Will", Ability.ETypeIdentity.Utility,

                new Ability.Unsourced.SingleTargetCondition((p, u) => p.Team != u.Team),

                new Ability.PlayAction(a =>
                {
                    GameAction.Move.Prompt(new GameAction.Move.PromptArgs.Pathed
                        (a.Performer, a.ParticipatingUnits[0], 3),
                        move => a.AddLateResultant(move));
                })

                
            ),
        };


        //FINALIZE REGISTRY
        Registry = new ReadOnlyCollection<ConstructorTemplate<Ability>>(masterList);
    }

    private static Vector3Int H(int left, int up, int right) => BoardCoords.Simple(left, up, right);
}
