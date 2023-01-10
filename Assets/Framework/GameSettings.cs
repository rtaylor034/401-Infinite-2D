using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

public record GameSettings
{
    
    public ReadOnlyCollection<ConstructorTemplate<Player>> TurnOrder { get; private set; }
    public int StandardEffectDuration { get; private set; }
    public ReadOnlyCollection<Team> Teams { get; private set; }

    private GameSettings(List<Team> teams, List<int> turnOrder, int standardEffectDuration)
    {
        List<ConstructorTemplate<Player>> orderInit = new(turnOrder.Count);
        for (int i = 0; i < turnOrder.Count; i++) orderInit[i] = new(typeof(Player), teams[turnOrder[i]]);

        Teams = teams.AsReadOnly();
        TurnOrder = orderInit.AsReadOnly();
        StandardEffectDuration = standardEffectDuration;
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
        standardEffectDuration: 1
        );
}
