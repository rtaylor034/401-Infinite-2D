using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

public class GameSettings
{

    public ReadOnlyCollection<ConstructorTemplate<Player>> TurnOrder { get; private set; }
    public int StandardEffectDuration { get; private set; }

    private GameSettings(List<ConstructorTemplate<Player>> turnOrder, int standardEffectDuration)
    {
        TurnOrder = turnOrder.AsReadOnly();
        StandardEffectDuration = standardEffectDuration;
    }

    public readonly static GameSettings STANDARD = new(
        turnOrder: new()
        {
            new ConstructorTemplate<Player>(typeof(Player), Player.ETeam.Blue),
            new ConstructorTemplate<Player>(typeof(Player), Player.ETeam.Red),
        },
        standardEffectDuration: 1
        );
}
