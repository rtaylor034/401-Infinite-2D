using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public static class AbilityRegistry
{

    public static ReadOnlyCollection<ConstructorTemplate<Ability>> Registry { get; private set; }
    

    public static void Initialize(GameSettings settings)
    {

        int STD_DURATION = settings.StandardEffectDuration;
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
        * ON-PLAY ACTION - Ability.PlayAction <void(GameAction.PlayAbility)>
        */

        List<ConstructorTemplate<Ability>> masterList = new()
        {
            //>0 LANCE
            new
            (
                typeof(Ability.Sourced),

                "Lance", Ability.ETypeIdentity.Attack,

                new ConstructorTemplate<UnitEffect>[]
                {
                    new(typeof(UnitEffect.Slow), STD_DURATION),
                    new(typeof(UnitEffect.Damage), STD_DURATION)
                },
                new HashSet<Vector3Int>
                {
                    H(0, 1, 0),
                    H(0, 2, 0),
                    H(0, 3, 0)
                },

                new Ability.PlayAction(async a =>
                {
                    await a.AddResultant(await GameAction.Move.Prompt(new GameAction.Move.PromptArgs.Pathed
                        (a.Performer, a.ParticipatingUnits[0], 1)));
                }),

                new Ability.Sourced.TargetingCondition[]
                {
                    Ability.Sourced.STANDARD_ATTACK_TARGET,
                    Ability.Sourced.STANDARD_COLLISION
                }
            ),

            //>1 BREAK WILL
            new
            (
                typeof(Ability.Unsourced),

                "Break Will", Ability.ETypeIdentity.Utility,

                new Ability.Unsourced.SingleTargetCondition((p, u) => p.Team != u.Team),

                new Ability.PlayAction(async a =>
                {
                    await a.AddResultant(await GameAction.Move.Prompt(new GameAction.Move.PromptArgs.Pathed
                        (a.Performer, a.ParticipatingUnits[0], 3)));
                })


            ),

            //>2 INSPIRE
            //uses new format
            new(typeof(Ability.Sourced), new object[]
            {
                "Inspire", Ability.ETypeIdentity.Defense,

                new ConstructorTemplate<UnitEffect>[]
                {
                    new(typeof(UnitEffect.Shield), STD_DURATION)
                },

                new HashSet<Vector3Int>
                {
                    H(1, 0, 0),
                    H(2, 0, 0),
                    H(0, 1, 0),
                    H(0, 2, 0),
                    H(0, 0, 1),
                    H(0, 0, 2),
                    H(1, 1, 0),
                    H(0, 1, 1)
                },

                new Ability.PlayAction(async a =>
                {
                    await a.AddResultant(await GameAction.Move.Prompt(new GameAction.Move.PromptArgs.Pathed
                        (a.Performer, a.ParticipatingUnits[1], 5)));
                }),

                new Ability.Sourced.TargetingCondition[]
                {
                    Ability.Sourced.STANDARD_DEFENSE_TARGET,
                    Ability.Sourced.STANDARD_COLLISION
                }

            })
        };


        //FINALIZE REGISTRY
        Registry = new ReadOnlyCollection<ConstructorTemplate<Ability>>(masterList);
    }

    private static Vector3Int H(int left, int up, int right) => BoardCoords.Simple(left, up, right);
}
