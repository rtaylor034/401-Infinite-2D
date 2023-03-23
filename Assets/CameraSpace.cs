using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

public class CameraSpace : MonoBehaviour
{
    public static Camera ActiveCamera { get; private set; }
    public GameObject test;

    private static CameraSpace _instance;

    private float _baseSize;
    private Dictionary<GameObject, Vector2> _links = new();

    private void Awake()
    {
        SetCamera(Camera.main, true);
        _instance = this;
        Link(test, 0.2f, 0.8f);
    }
    private void Update()
    {
        foreach (var link in _links)
        {
            link.Key.transform.position = ActiveCamera.ViewportToWorldPoint(new Vector3(link.Value.x, link.Value.y, 1));
            link.Key.transform.localScale = Vector3.one * ActiveCamera.orthographicSize / _baseSize;
        }
    }

    private void SetCamera(Camera camera, bool setBaseSize)
    {
        ActiveCamera = camera;
        if (setBaseSize) _baseSize = Camera.main.orthographicSize;
        transform.SetParent(camera.transform);
    }

    //returns false if obj is already linked. still updates position
    public static bool Link(GameObject obj, float x, float y) => Link(obj, new Vector2(x, y));
    public static bool Link(GameObject obj, Vector2 pos)
    {
        obj.transform.SetParent(_instance.transform);
        if (_instance._links.TryAdd(obj, pos)) return true;
        _instance._links[obj] = pos;
        return false;
    }
    public static bool LinkAtCurrentPosition(GameObject obj) => Link(obj, ActiveCamera.WorldToViewportPoint(obj.transform.position));
}
