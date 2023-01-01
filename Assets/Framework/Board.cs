using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class Board : MonoBehaviour
{
    [SerializeField]
    private Unit _UnitObject;
    [SerializeField]
    public HXNKey HXNKey;
    [SerializeField]
    private float _hexSpacing = 0.54f;

    private readonly HashSet<Unit> _units = new();
    private readonly Dictionary<Vector3Int, Hex> _hexDict = new();

    /// <summary>
    /// Gets the set of all Units on the board.
    /// </summary>
    public HashSet<Unit> Units => new(_units);
    public Dictionary<Vector3Int, Hex> HexDict => new(_hexDict);
    public void CreateBoard()
    {
        GenerateMap(Map.MapList[0]);
        GenerateUnits();
    }

    /// <summary>
    /// Instantiates Units on every BaseHex on the board. (GenerateMap() must be called first).
    /// </summary>
    private void GenerateUnits()
    {
        foreach (Hex hex in _hexDict.Values)
        {

            if (hex is not BaseHex b) continue;

            Unit u = Instantiate(_UnitObject, transform).Init(this, 3, b.Team, b.Position);
            u.transform.localPosition = GetLocalTransformAt(b.Position, -1);
            b.Occupant = u;
            _units.Add(u);
        }
    }

    /// <summary>
    /// Instantiates/renders the physical Hexes of the given <see cref="Map"/>
    /// </summary>
    /// <param name="map"></param>
    /// <param name="hexSpacing"></param>
    private void GenerateMap(Map map)
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
            //Debug.Log($"generating row: [{hstr}]");

            for (int x = 0; x < hstr.Length; x++)
            {
                Vector3Int coords = (BoardCoords.up * u) - (BoardCoords.left * x);
                Hex hexprefab = HXNKey.GetHex(hstr[x]);

                if (hexprefab == null) continue;

                Hex hex = Instantiate(hexprefab, transform).Init(this, coords);

                //Uses helper class BoardCoords 
                hex.transform.localPosition = GetLocalTransformAt(coords);
                _hexDict.Add(coords, hex);

            }


        }

    }

    /// <summary>
    /// Gets the Hex at the given coordinates.
    /// </summary>
    /// <remarks>
    /// If no hex is found at the coordinates: <br></br>
    /// > <paramref name="strict"/> = true : throws an exception. (Default) <br></br>
    /// > <paramref name="strict"/> = false : returns null.
    /// </remarks>
    /// <param name="position"></param>
    /// <returns></returns>
    public Hex HexAt(Vector3Int position, bool strict = false)
    {
        if (!_hexDict.TryGetValue(position, out Hex hex))
        {
            if (strict) throw new System.Exception($"No Hex found at {position} on board {name} | (strict was set true)");
        }
        return hex;
    }

    public HashSet<Hex> HexesAt(IEnumerable<Vector3Int> positions, bool strict = false)
    {
        HashSet<Hex> o = new HashSet<Hex>();

        foreach (Vector3Int pos in positions)
        {
            if (!_hexDict.TryGetValue(pos, out Hex hex))
            {
                if (strict) throw new System.Exception($"No Hex found at {pos} on board {name} | (strict was set true)");
            }
            else
            o.Add(hex);
        }
        return o;
    }

    //ALL changing of a GameObject's in-world position should happen in Board or GameManager. (transform.position should not be used, use transform.localPosition).
    /// <summary>
    /// Gets the transform.localPosition of the specified board coords.
    /// </summary>
    /// <param name="coords"></param>
    /// <returns></returns>
    public Vector3 GetLocalTransformAt(Vector3Int coords, int zPos = 0)
    {
        Vector2 fpos = coords.CartesianCoordsOf() * _hexSpacing;
        return new Vector3(fpos.x, fpos.y, zPos);
    }

    public delegate bool ContinuePathCondition(Hex prev, Hex next);
    public delegate int PathWeightFunction(Hex prev, Hex next);
    public delegate bool FinalPathCondition(Hex hex);

    //Old pathfinding algorithm (uncool and cringe)
    /*
    public HashSet<Hex> PathFindOLD(Vector3Int startPos, (int, int) range, ContinuePathCondition pathCondition, FinalPathCondition finalCondition)
    {
        HashSet<Hex> o = new() { HexAt(startPos) };
        HashSet<Hex> traversed = new();
        __Recur(o, range.Item2);
        void __Recur(HashSet<Hex> roots, int r)
        {
            if (range.Item2 - range.Item1 > r) o.UnionWith(roots);
            if (r == 0) return;
            traversed.UnionWith(roots);
            HashSet<Hex> branches = new();
            foreach (Hex prev in roots)
            {
                foreach (Vector3Int nPos in prev.Position.GetAdjacent())
                {
                    Hex next = HexAt(nPos, false);
                    if (next is null) continue;
                    if (pathCondition(prev, next))
                    {
                        branches.Add(next);
                    }
                }
            }
            branches.ExceptWith(traversed);
            branches.ExceptWith(roots);
            if (branches.Count > 0) __Recur(branches, r - 1);
        }
        o.RemoveWhere(hex => !finalCondition(hex));
        return o;
    }
    */

    public Dictionary<Hex, int> PathFind(Vector3Int startPos, (int min, int max) range, ContinuePathCondition pathCondition, FinalPathCondition finalCondition, PathWeightFunction weightFunction)
    {
        Dictionary<Hex, int> o = new();
        Dictionary<Hex, int> tickers = new() { { HexAt(startPos), 0} };
        for (int r = 0; r < range.max;)
        {
            int minWeight = int.MaxValue;
            foreach (Hex h in new List<Hex>(tickers.Keys))
            {
                int val = tickers[h];
                if (val < minWeight) minWeight = val;
                if (val > 0) continue;
                o.Add(h, r);
                foreach(var npos in h.Position.GetAdjacent())
                {
                    if (HexAt(npos) is not Hex nhex) continue;
                    if (o.ContainsKey(nhex)) continue;
                    if (!pathCondition(h, nhex)) continue;
                    int nval = weightFunction(h, nhex);
                    if (!tickers.TryAdd(nhex, nval) && tickers[nhex] > nval)
                        tickers[nhex] = nval;
                }
                tickers.Remove(h);
            }
            foreach (var t in new List<Hex>(tickers.Keys)) tickers[t] -= minWeight;
            r += minWeight;
        }
        foreach (var hex in new List<Hex>(o.Keys))
            if (!finalCondition(hex) || o[hex] < range.min) o.Remove(hex);
        return o;
    }
}
