using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using System;

public record GameSettings
{

    public ReadOnlyCollection<ConstructionTemplate<Player>> TurnOrder { get; private set; }
    public int StandardEffectDuration { get; private set; }
    public ReadOnlyCollection<Team> Teams { get; private set; }
    public ReadOnlyCollection<ConstructionTemplate<ManualAction>> DefaultManualActions { get; private set; }
    public int BoardCount { get; private set; } = 1;

    private GameSettings(List<Team> teams, List<int> turnOrder, List<ConstructionTemplate<ManualAction>> defaultManualActions, int standardEffectDuration)
    {
        List<ConstructionTemplate<Player>> orderInit = new();
        for (int i = 0; i < turnOrder.Count; i++) orderInit.Add(() => new Player(teams[turnOrder[i]]));

        Teams = teams.AsReadOnly();
        TurnOrder = orderInit.AsReadOnly();
        StandardEffectDuration = standardEffectDuration;
        DefaultManualActions = defaultManualActions.AsReadOnly();
    }

    public readonly static GameSettings STANDARD = new(
        teams: new()
        {
            new("Blue", Color.blue, 0),
            new("Red", Color.red, 3)
        },
        turnOrder: new()
        {
            0,
            1
        },
        defaultManualActions: new()
        {
            () =>
            new ManualAction(ManualAction.EStandardType.Move)
            {
                EntryPoints = player => player.GetAllyUnits(GameManager.GAME.ActiveBoards[0]),
                Action = async (player, selectedUnit) =>
                {
                    if (selectedUnit is not Unit u) throw new System.Exception();

                    return await (await GameAction.Move.Prompt(player, new GameAction.Move.PathedInfo(u)
                    {
                        Distance = 4,
                        MinDistance = 1
                    })
                    )?.AddResultant(new GameAction.EnergyChange(player, player, e => e - 1));
                },
                PlayerConditions = new() { ManualAction.ONE_ENERGY_REQUIRED }
            }
        },
        standardEffectDuration: 1
        );
}
