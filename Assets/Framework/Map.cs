using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Map
{
    public readonly static List<Map> MapList = new();
    public string Name { get; private set; }
    public string[] HXN { get; private set; }

    private Map(string name, string[] hxn)
    {
        Name = name;
        HXN = hxn;
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
            "oooooooB/" +
            "woooooooBB/" +
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
            "       RRoooooooo/" +
            "         Rooooooo/" +
            "           wwwwww/" +
            "             wwww/" +
            "               ww"
            ).Split('/')
            ));



    }



}
