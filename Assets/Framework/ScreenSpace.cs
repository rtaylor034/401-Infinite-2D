using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

public class ScreenSpace : MonoBehaviour
{
    public static Camera ActiveCamera { get; private set; }

    private static ScreenSpace _instance;

    private float _baseSize;
    private Dictionary<GameObject, Vector2> _links = new();

    private void Awake()
    {
        SetCamera(Camera.main, true);
        _instance = this;
    }
    private void Update()
    {
        foreach (var link in _links)
        {
            link.Key.transform.position = ActiveCamera.ViewportToWorldPoint(new Vector3(link.Value.x, link.Value.y, 1));
            link.Key.transform.localScale = Vector3.one * ActiveCamera.orthographicSize / _baseSize;
        }
    }

    /// <summary>
    /// Sets the camera that defines the "screen space".<br></br>
    /// > If <paramref name="setBaseSize"/> is <see langword="true"/>, objects will appear as the size that they appear *right now* when linked.
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="setBaseSize"></param>
    public void SetCamera(Camera camera, bool setBaseSize)
    {
        ActiveCamera = camera;
        if (setBaseSize) _baseSize = Camera.main.orthographicSize;
        transform.SetParent(camera.transform);
    }

    //returns false if obj is already linked. still updates position

    /// <inheritdoc cref="Link(GameObject, Vector2)"/>
    public static bool Link(GameObject obj, float x, float y) => Link(obj, new Vector2(x, y));

    /// <summary>
    /// Links <paramref name="obj"/> to screen space at the position <paramref name="pos"/>.<br></br>
    /// (<paramref name="pos"/> coordinates range from (0, 0) bottom-left to (1, 1) top-right)
    /// <br></br><br></br>
    /// Objects linked to screen space move and scale with the camera's view.<br></br>
    /// <i>i.e. Linked objects will act as a HUD element, fixed to the player's screen.</i>
    /// </summary>
    /// <remarks>
    /// <i>Camera space will entirely override a linked object's transform.</i>
    /// </remarks>
    /// <param name="obj"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static bool Link(GameObject obj, Vector2 pos)
    {
        if (_instance._links.TryAdd(obj, pos)) return true;
        _instance._links[obj] = pos;
        return false;
    }
    /// <summary>
    /// Links <paramref name="obj"/> to screen space at its current position on-screen.<br></br><br></br>
    /// <i>(See <see cref="Link(GameObject, Vector2)"/>)</i>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool LinkAtCurrentPosition(GameObject obj) => Link(obj, ActiveCamera.WorldToViewportPoint(obj.transform.position));

    /// <summary>
    /// Unlinks <paramref name="obj"/> from screen space.
    /// </summary>
    /// <remarks>
    /// <i><paramref name="obj"/> will keep the transform changes that screen space applied while linked.</i>
    /// </remarks>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool Unlink(GameObject obj)
    {
        return _instance._links.Remove(obj);
    }
}
