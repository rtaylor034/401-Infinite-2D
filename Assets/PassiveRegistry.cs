using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public static class PassiveRegistry
{
    public static ReadOnlyCollection<ConstructorTemplate<Passive>> Registry { get; private set; }


    public static void Initialize(GameSettings settings)
    {
        List<ConstructorTemplate<Passive>> masterList = new()
        {
            new(typeof(Passive.Test), "Agile")
        };

        Registry = masterList.AsReadOnly();
    }
}
