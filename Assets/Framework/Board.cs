using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class Board : MonoBehaviour
{
    [SerializeField]
    private Unit _UnitObject;
    [SerializeField]
    public HXNKey HXNKey;

    private Dictionary<Vector3Int, Hex> _hexDict = new();

    /*
    public void CreateBoard()
    {
        
        //(This is placeholder code)
        
        //"this" refers to the empty GameObject floating in space at (0, 0, 0). It can be ommited in "this.transform", but its there for clarity.
        Unit testunit = Instantiate(_UnitObject, this.transform).Init(3);
        testunit.TestMethod();

        Hex testopenhex = Instantiate(_OpenHexObject, this.transform.position + new Vector3(0, -1, 0), this.transform.rotation, this.transform).Init(new Vector3Int(1, 2, -3));
        testopenhex.TestMethod();
        
        Hex testwallhex = Instantiate(_WallHexObject, this.transform.position + new Vector3(0, -2, 0), this.transform.rotation, this.transform).Init(new Vector3Int(-1, -2, 3));
        testwallhex.TestMethod();

    }
    */

    public void CreateBoard()
    {
        GenerateMap(Map.MapList[0], 0.54f);
    }

    /// <summary>
    /// Instantiates/renders the physical Hexes of the given <see cref="Map"/>
    /// </summary>
    /// <param name="map"></param>
    /// <param name="hexSpacing"></param>
    private async void GenerateMap(Map map, float hexSpacing = 1)
    {
        /*
         The first element(string) of map.HXN starts at 0,0,0(bottom left of map), and then each char in that string generates a Hex at that position.
         each following char generates its Hex 1 position right-downward of the previous, until the end of the string.
         Do this for each element(string) in map.HXN, moving the starting position up 1 hex each time until the entire map is generated.

         (each individual string is a "row", generating from the bottom-up)
         */
        for (int u = 0; u < map.HXN.Length; u++)
        {
            //quick reference var
            string hstr = map.HXN[u];

            //for logging
            Debug.Log($"generating row: [{hstr}]");

            for (int x = 0; x < hstr.Length; x++)
            {
                Vector3Int coords = (BoardCoords.up * u) - (BoardCoords.left * x);
                Hex hexprefab = HXNKey.GetHex(hstr[x]);

                if (hexprefab == null) continue;

                Hex hex = Instantiate(hexprefab, transform).Init(coords);

                //Uses helper class BoardCoords 
                Vector3 worldpos = new Vector3(BoardCoords.CartesianCoordsOf(coords).x, BoardCoords.CartesianCoordsOf(coords).y, 0);
                hex.transform.localPosition = worldpos * hexSpacing;
                _hexDict.Add(coords, hex);

                //for visualization (Can be removed)
                await Task.Delay(60);

            }


        }

    }

    /// <summary>
    /// Gets the Hex at the given coordinates.
    /// </summary>
    /// <remarks>
    /// Returns null if there is not a Hex at the given coordinates.
    /// </remarks>
    /// <param name="coords"></param>
    /// <returns></returns>
    public Hex HexAt(Vector3Int coords)
    {
        if (!_hexDict.TryGetValue(coords, out Hex hex)) return null;
        return hex;
    }

}
