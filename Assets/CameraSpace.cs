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
    private HashSet<GameObject> _linkedObjects;

    private void Awake()
    {
        SetCamera(Camera.main);
        _instance = this;
    }
    private void Update()
    {
        test.transform.position = ActiveCamera.ViewportToWorldPoint(new Vector3(0f, 0.2f, 1));
        test.transform.localScale = Vector3.one * ActiveCamera.orthographicSize / _baseSize;
    }

    private void FixedUpdate()
    {
        print(ActiveCamera.pixelRect);
    }
    private void OnViewportChange()
    {
        foreach (var obj in _linkedObjects)
        {

        }
    }
    private void OnCameraResize()
    {

    }

    private void SetCamera(Camera camera)
    {
        ActiveCamera = camera;
        _baseSize = Camera.main.orthographicSize;
        transform.SetParent(camera.transform);
    }

    //returns false if obj is already linked. still updates position
    public static bool Link(GameObject obj)
    {
        obj.transform.SetParent(_instance.transform);
        return _instance._linkedObjects.Add(obj);
        
    }
}
