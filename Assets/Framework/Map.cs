using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Map
{
    public readonly static List<Map> MapList = new();
    public string Name { get; private set; }
    public string[] HXN { get; private set; }
    public Vector3Int[][] Spawns { get; private set; }

    private Map(string name, string[] hxn, Vector2Int[][] spawns)
    {
        Name = name;
        HXN = hxn;

        Spawns = new Vector3Int[spawns.Length][];
        for (int i = 0; i < spawns.Length; i++)
        {
            Spawns[i] = new Vector3Int[spawns[i].Length];
            for (int h = 0; i < spawns[i].Length; h++)
            {
                Spawns[i][h] = (spawns[i][h].x * BoardCoords.up) - (spawns[i][h].y * BoardCoords.left);
            }
        }

        MapList.Add(this);
    }


    //Static constructor, initializes all maps.
    static Map()
    {
        //Default map [0]
        MapList.Add(new Map("Default Map",
            (
            "ww/" +
            "wwww/" +
            "wwwwww/" +
            "oooooooB0/" +
            "woooooooB0B0/" +
            "ooooooooooow/" +
            "ooooooowooooww/" +
            "oowoowoowwooowww/" +
            "oowooooooooowwwww/" +
            "oowooooooooooooww/" +
            "oowoocooooowoooow/" +
            "woooooowwoowwoooo/" +
            "oooooooowwooooooo/" +
            "woooowooooocooooo/" +
            "wwooooooooooowooo/" +
            "wwwwwoooooowoooww/" +
            " wwwooowwoowwooow/" +
            "   wwoooowooooooo/" +
            "     wooooooooooo/" +
            "       B1B1oooooooo/" +
            "         B1ooooooo/" +
            "           wwwwww/" +
            "             wwww/" +
            "               ww"
            ).Split('/'),

            new Vector2Int[][]
            { new Vector2Int[]
            {
                new(3, 8),
                new(4, 9),
                new(4, 10)
            },
            new Vector2Int[]
            {
                new(19, 8),
                new(19, 9),
                new(20, 10)
            }
            }
            ));



    }



}
