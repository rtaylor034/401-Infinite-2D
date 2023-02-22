using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using static Ability.Unsourced;

public static class AbilityRegistry
{

    public static ReadOnlyCollection<ConstructionTemplate<Ability>> Registry { get; private set; }
    

    public static void Initialize(GameSettings settings)
    {
        //captured values
        int STD_DURATION = settings.StandardEffectDuration;

        List<ConstructionTemplate<Ability>> masterList = new()
        {
            //>0 LANCE
            () => new Ability.Sourced
            (

                "Lance",
                Ability.ETypeIdentity.Attack,
                new ConstructionTemplate<UnitEffect>[]
                {
                    () => new UnitEffect.Slow(STD_DURATION),
                    () => new UnitEffect.Damage(STD_DURATION)
                },
                new HashSet<Vector3Int>
                {
                    Hit(0, 1, 0),
                    Hit(0, 2, 0),
                    Hit(0, 3, 0)
                },
                new Ability.PlayAction(async a =>
                {
                    a.AddImplicitResultant(await GameAction.Move.Prompt(a.Performer,
                        new GameAction.Move.PathedInfo(a.ParticipatingUnits[0])
                        {
                            Distance = 1
                        }));
                }),
                new Ability.Sourced.TargetingCondition[]
                {
                    Ability.Sourced.STANDARD_ATTACK_TARGET,
                    Ability.Sourced.STANDARD_COLLISION
                }
            ),

            //>1 BREAK WILL
            () => new Ability.Unsourced
            (
                "Break Will", Ability.ETypeIdentity.Utility,
                new Ability.Unsourced.SingleTargetCondition((p, u) => p.Team != u.Team),
                new Ability.PlayAction(async a =>
                {
                    a.AddImplicitResultant(await GameAction.Move.Prompt(a.Performer,
                        new GameAction.Move.PathedInfo(a.ParticipatingUnits[0])
                        {
                            Distance = 3
                        }));
                })


            ),

            //>2 INSPIRE
            //uses new format
            () => new Ability.Sourced
            (
                "Inspire", Ability.ETypeIdentity.Defense,

                new ConstructionTemplate<UnitEffect>[]
                {
                    () => new UnitEffect.Shield(STD_DURATION)
                },
                new HashSet<Vector3Int>
                {
                    Hit(1, 0, 0),
                    Hit(2, 0, 0),
                    Hit(0, 1, 0),
                    Hit(0, 2, 0),
                    Hit(0, 0, 1),
                    Hit(0, 0, 2),
                    Hit(1, 1, 0),
                    Hit(0, 1, 1)
                },
                new Ability.PlayAction(async a =>
                {
                    a.AddImplicitResultant(await GameAction.Move.Prompt(a.Performer,
                        new GameAction.Move.PathedInfo(a.ParticipatingUnits[1])
                        {
                            Distance = 5
                        }));
                }),
                new Ability.Sourced.TargetingCondition[]
                {
                    Ability.Sourced.STANDARD_DEFENSE_TARGET,
                    Ability.Sourced.STANDARD_COLLISION
                }

            )
        };


        //FINALIZE REGISTRY
        Registry = new ReadOnlyCollection<ConstructionTemplate<Ability>>(masterList);
    }

    /// <summary>
    /// [Local Shorthand]<br></br>
    /// Equivalent to <c><see cref="BoardCoords.Simple(int, int, int)"/></c>
    /// </summary>
    /// <param name="left"></param>
    /// <param name="up"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    private static Vector3Int Hit(int left, int up, int right) => BoardCoords.Simple(left, up, right);
    private static List<TargetCondition> UnsourcedSingleTarget(Func<Player, Unit, bool> singleUnitCondition) =>
            new() { (p, _, u) => singleUnitCondition(p, u) };
}
