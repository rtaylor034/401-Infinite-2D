using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// REQUIRES DOCS
/// </summary>
public class Team
{
    public string Name { get; private set; }
    public Pallete Colors { get; private set; }
    public int PerspectiveRotation { get; private set; }

    public Team(string name, Color mainColor, int perspective)
    {
        Name = name;
        Colors = new(mainColor);
        PerspectiveRotation = perspective;
    }

    public struct Pallete
    {
        public Color Primary { get; private set; }
        public Color Unit { get; private set; }
        public Color BaseHex { get; private set; }

        public Pallete(Color primary)
        {
            //placeholder colors for now
            Primary = primary;
            Unit = Color.Lerp(primary, Color.white, 0.2f);
            BaseHex = Color.Lerp(primary, Color.black, 0.0f);
            
        }
    }
    public override string ToString() => Name;
}
public static class TeamExtensions
{
    /// <summary>
    /// Rotates (<see langword="this"/>)<paramref name="coords"/> around <paramref name="anchor"/> to respect <paramref name="team"/>'s perspective.
    /// </summary>
    /// <param name="coords"></param>
    /// <param name="team"></param>
    /// <param name="anchor"></param>
    /// <returns></returns>
    public static HashSet<Vector3Int> RotateForPerspective(this IEnumerable<Vector3Int> coords, Team team, Vector3Int anchor) => coords.Rotate(anchor, team.PerspectiveRotation);
}