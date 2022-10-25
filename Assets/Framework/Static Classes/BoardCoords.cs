
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoardCoords
{
    /// <summary>
    /// Directly up/forward in board coordinates. <br></br><b>(0, 1, -1)</b>
    /// </summary>
    public static Vector3Int up { get => new Vector3Int(0, 1, -1); }
    /// <summary>
    /// Right-up diagonal in board coordinates. <br></br><b>(1, 0, -1)</b>
    /// </summary>
    public static Vector3Int right { get => new Vector3Int(1, 0, -1); }
    /// <summary>
    /// Left-up diagonal in board coordinates. <br></br><b>(-1, 1, 0)</b>
    /// </summary>
    public static Vector3Int left { get => new Vector3Int(-1, 1, 0); }
    public static int[] Indicies => new int[] { 0, 1, 2 };

    /// <summary>
    /// Rotates a board coordinate 60 degrees (1 axis) clockwise around another board coord, n times.
    /// </summary>
    /// <param name="coord"></param>
    /// <param name="around"></param>
    /// <param name="rotations"></param>
    /// <returns></returns>
    public static Vector3Int Rotate(this Vector3Int coord, Vector3Int around, int n)
    {
        if (n < 0) n = 6 - n;
        if (n == 0) return coord;

        Vector3Int rot = coord - around;
        //shift indicies
        rot = new Vector3Int(-rot.z, -rot.x, -rot.y);

        return Rotate(around + rot, around, n - 1);
    }
    /// <summary>
    /// Rotates a set of board coordinates 60 degrees (1 axis) clockwise around another board coord, n times.
    /// </summary>
    /// <param name="coord"></param>
    /// <param name="around"></param>
    /// <param name="rotations"></param>
    /// <returns></returns>
    public static List<Vector3Int> Rotate(this IEnumerable<Vector3Int> coords, Vector3Int around, int n)
    {

        var result = new List<Vector3Int>();
        foreach (Vector3Int v in coords)
        {
            result.Add(Rotate(v, around, n));
        }
        return result;
    }


    /// <summary>
    /// Gets all board coordinates that are adjacent to the one specified.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static List<Vector3Int> GetAdjacent(this Vector3Int pos)
    {
        var adj = new List<Vector3Int>();
        for (int r = 0; r < 6; r++) adj.Add(Rotate(pos + up, pos, r));
        return adj;
    }

    /// <summary>
    /// Mirrors a board coordinate about an axis. <br><br></br></br>([<paramref name="axis"/>] is the unchanged index)
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="axis"></param>
    /// <returns></returns>
    public static Vector3Int Mirror(this Vector3Int coord, byte axis)
    {
        byte[] flips = OtherIndices(axis);

        int swap = coord[flips[0]];
        coord[flips[0]] = coord[flips[1]];
        coord[flips[1]] = swap;

        return coord;
    }

    /// <summary>
    /// Mirrors a set of board coordinates about an axis. <br><br></br></br>([<paramref name="axis"/>] is the unchanged index).
    /// </summary>
    /// <param name="posSet"></param>
    /// <param name="axis"></param>
    /// <returns></returns>
    public static List<Vector3Int> Mirror(this IEnumerable<Vector3Int> coords, byte axis)
    {
        var result = new List<Vector3Int>();
        foreach (Vector3Int v in coords)
        {
            result.Add(Mirror(v, axis));
        }
        return result;
    }


    /// <summary>
    /// Gets the spaces that intersect with the straight line between pos1 and pos2.
    /// </summary>
    /// <param name="pos1"></param>
    /// <param name="pos2"></param>
    /// <param name="exactEdgePairs"></param>
    /// <returns></returns>
    /// <remarks>
    /// <i>out </i><paramref name="exactEdgePairs"/>: Space-pairs that the line goes exactly in-between. (null if none)
    /// </remarks>
    public static List<Vector3Int> GetLineIntersections(this Vector3Int pos1, Vector3Int pos2, out List<Vector3Int[]> exactEdgePairs)
    {
        //find maxindex AND check for exact straight.
        List<byte> ind = new List<byte> { 0, 1, 2 };
        int maxdiff = 0;
        byte maxindex = 0;
        foreach (byte i in ind)
        {
            if (Mathf.Abs((pos1 - pos2)[i]) > maxdiff)
            {
                maxindex = i;
                maxdiff = Mathf.Abs((pos1 - pos2)[i]);
            }
            if ((pos1 - pos2)[i] == 0)
            {
                ind.Remove(i);
                exactEdgePairs = null;
                return ExactStraight(ind[0], ind[1], pos1, pos2, false);
            }
        }
        ind.Remove(maxindex);
        List<Vector3Int> intersectingSpaces = new List<Vector3Int>();

        //check for exact edge-axis line
        if ((pos1 - pos2)[ind[0]] == (pos1 - pos2)[ind[1]])
        {
            exactEdgePairs = new List<Vector3Int[]>();

            //**this could be done alot more efficiently, this is just the most understandable way.**
            for (Vector3Int diff = pos1 - pos2; diff != Vector3Int.zero;)
            {
                Vector3Int[] pair = new Vector3Int[2];

                diff[maxindex] -= (int)Mathf.Sign(diff[maxindex]);

                diff[ind[0]] -= (int)Mathf.Sign(diff[ind[0]]); //original subtract of ind[0]
                pair[0] = pos2 + diff;

                diff[ind[0]] += (int)Mathf.Sign(diff[ind[1]]); //re-add ind[0]
                diff[ind[1]] -= (int)Mathf.Sign(diff[ind[1]]);
                pair[1] = pos2 + diff;

                diff[ind[0]] -= (int)Mathf.Sign(diff[ind[0]]); //and then re-subtract ind[0]. its retarded but i cant be bothered.
                diff[maxindex] -= (int)Mathf.Sign(diff[maxindex]);
                intersectingSpaces.Add(pos2 + diff);
                exactEdgePairs.Add(pair);
            }
            intersectingSpaces.RemoveAt(intersectingSpaces.Count - 1);
            return intersectingSpaces;

        }
        //possible area of spaces hueristic if not exact straight or exact axis
        List<Vector3Int> possibleSpaces = new List<Vector3Int>();
        foreach (Vector3Int p in SpacesToNearestAxis(maxindex, ind[0], pos1, pos2, true))
        {
            foreach (Vector3Int s in SpacesToNearestAxis(ind[1], maxindex, p, pos2, true))
            {
                possibleSpaces.Add(s);
            }
        }
        //insane angle point math
        possibleSpaces.Remove(pos1);
        possibleSpaces.Remove(pos2);

        Vector3Int rpos1 = pos1 - pos2;
        foreach (Vector3Int h in possibleSpaces)
        {
            Vector3Int rh = h - pos2;
            for (int pn = 0; pn < 5; pn++)
            {
                if (Mathf.Sign(Vector2.SignedAngle(CartesianCoordsOf(rh) + new Vector2(Mathf.Cos(pn * 60 * Mathf.Deg2Rad), Mathf.Sin(pn * 60 * Mathf.Deg2Rad)), CartesianCoordsOf(rpos1))) !=
                    Mathf.Sign(Vector2.SignedAngle(CartesianCoordsOf(rh) + new Vector2(Mathf.Cos((pn + 1) * 60 * Mathf.Deg2Rad), Mathf.Sin((pn + 1) * 60 * Mathf.Deg2Rad)), CartesianCoordsOf(rpos1))))
                {
                    intersectingSpaces.Add(h);
                    break;
                }
            }

        }
        exactEdgePairs = null;
        return intersectingSpaces;
    }

    private static List<Vector3Int> ExactStraight(byte i1, byte i2, Vector3Int pos1, Vector3Int pos2, bool includeInitials = false)
    {
        Vector3Int diff = pos1 - pos2;
        List<Vector3Int> spaces = new List<Vector3Int>();
        while (diff[i1] != 0)
        {
            int inc = 1;
            if (diff[i1] > 0) inc = -1;

            diff[i1] += inc;
            diff[i2] -= inc;
            spaces.Add(pos2 + diff);

        }
        if (!includeInitials) spaces.RemoveAt(spaces.Count - 1);
        if (includeInitials) spaces.Add(pos1);
        return spaces;
    }

    private static List<Vector3Int> SpacesToNearestAxis(byte i1, byte i2, Vector3Int pos1, Vector3Int pos2, bool includeInitials = false)
    {
        List<Vector3Int> spaces = new List<Vector3Int>();
        Vector3Int stepper = pos1;

        while (stepper[i1] != pos2[i1] && stepper[i2] != pos2[i2])
        {
            stepper[i1] -= (int)Mathf.Sign((pos1 - pos2)[i1]);
            stepper[i2] += (int)Mathf.Sign((pos1 - pos2)[i1]);
            spaces.Add(stepper);
        }

        if (!includeInitials) spaces.RemoveAt(spaces.Count - 1);
        if (includeInitials) spaces.Add(pos1);
        return spaces;
    }


    /// <summary>
    /// Converts board coordinates to regular XY plane coordinates.
    /// </summary>
    /// <param name="BoardCoords"></param>
    /// <returns></returns>
    public static Vector2 CartesianCoordsOf(this Vector3Int BoardCoords)
    {
        return new Vector2(BoardCoords.x + BoardCoords.y * Mathf.Cos(120 * Mathf.Deg2Rad) + BoardCoords.z * Mathf.Cos(240 * Mathf.Deg2Rad), BoardCoords.y * Mathf.Sin(120 * Mathf.Deg2Rad) + BoardCoords.z * Mathf.Sin(240 * Mathf.Deg2Rad));
    }

    /// <summary>
    /// Removes the specified index from the array {0, 1, 2} and returns it.
    /// </summary>
    /// <param name="remove"></param>
    /// <remarks><i>Example: (1) returns: {0, 2} </i></remarks>
    /// <returns></returns>
    public static byte[] OtherIndices(byte remove)
    {
        List<byte> output = new List<byte>() { 0, 1, 2 };
        output.Remove(remove);
        return output.ToArray();
    }
    /// <summary>
    /// Removes the specified indices from the array {0, 1, 2} and returns it.
    /// </summary>
    /// <param name="remove"></param>
    /// <remarks><i>Example: ({1, 2}) returns: {0} </i></remarks>
    /// <returns></returns>
    public static byte OtherIndex(byte[] remove)
    {
        List<byte> output = new List<byte>() { 0, 1, 2 };
        foreach (byte r in remove) output.Remove(r);
        return output[0];
    }

    /// <summary>
    /// Returns actual board coordinates (x, y, z) for simplified (left, up, right) notation.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="up"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static Vector3Int Simple(int left, int up, int right)
    {
        return (left * BoardCoords.left) + (up * BoardCoords.up) + (right * BoardCoords.right);
    }

    /// <summary>
    /// Returns actual board coordinates (x, y, z) for simplified (left, up, right) notation.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="up"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static Vector3Int Simple(this Vector3Int simple)
    {
        return (left * simple.x) + (up * simple.y) + (right * simple.z);
    }

    /// <summary>
    /// Gets the radius distance between 2 board coordinates. 
    /// </summary>
    /// <param name="pos1"></param>
    /// <param name="pos2"></param>
    /// <returns></returns>
    public static int RadiusBetween(this Vector3Int pos1, Vector3Int pos2)
    {
        int r = 0;
        pos2 -= pos1;
        foreach (int i in Indicies)
        {
            r += Mathf.Abs(pos2[i]);
        }

        return r / 2;
    }


}

