using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using static Ability;

public static class AbilityRegistry
{

    public static ReadOnlyCollection<ConstructionTemplate<Ability>> Registry { get; private set; }
    

    public static void Initialize(GameSettings settings)
    {
        //captured values
        int STD_DURATION = settings.StandardEffectDuration;

        List<ConstructionTemplate<Ability>> masterList = new()
        {
            () => new Sourced("Lance", ETypeIdentity.Attack)
            {
                TargetEffects = new()
                {
                    () => new UnitEffect.Slow(STD_DURATION),
                    () => new UnitEffect.Damage(STD_DURATION)
                },
                HitArea = new()
                {
                    Hit(0, 1, 0),
                    Hit(0, 2, 0),
                    Hit(0, 3, 0)
                },
                TargetingConditions = new()
                {
                    Sourced.STANDARD_ATTACK_TARGET,
                    Sourced.STANDARD_COLLISION
                },
                FollowUpMethod = async a =>
                {
                    a.AddImplicitResultant(await GameAction.Move.Prompt(a.Performer,
                        new GameAction.Move.PathedInfo(a.ParticipatingUnits[0])
                        {
                            Distance = 1
                        }));
                },

            },

            () => new Unsourced("Break Will", ETypeIdentity.Utility)
            {
                TargetConditions = UnsourcedSingleTarget((p, u) => p.Team != u.Team),
                ActionMethod = async a =>
                {
                    a.AddImplicitResultant(await GameAction.Move.Prompt(a.Performer,
                        new GameAction.Move.PathedInfo(a.ParticipatingUnits[0])
                        {
                            Distance = 3
                        }));
                }
            },

            () => new Sourced("Inspire", ETypeIdentity.Defense)
            {
                TargetEffects = new()
                {
                    () => new UnitEffect.Shield(STD_DURATION)
                },
                HitArea = new()
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
                TargetingConditions = new()
                {
                    Sourced.STANDARD_DEFENSE_TARGET,
                    Sourced.STANDARD_COLLISION
                },
                FollowUpMethod = async a =>
                {
                    a.AddImplicitResultant(await GameAction.Move.Prompt(a.Performer,
                        new GameAction.Move.PathedInfo(a.ParticipatingUnits[1])
                        {
                            Distance = 5
                        }));
                }

            }
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
    private static List<Unsourced.TargetCondition> UnsourcedSingleTarget(Func<Player, Unit, bool> singleUnitCondition) =>
            new() { (p, _, u) => singleUnitCondition(p, u) };
}
