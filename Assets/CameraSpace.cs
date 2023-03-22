using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSpace : MonoBehaviour
{
    public static Camera ActiveCamera { get; private set; }
    public GameObject test;

    private float _baseSize;
    private HashSet<GameObject> _linkedObjects = new();

    private void Awake()
    {
        SetCamera(Camera.main);
        
    }
    private void Update()
    {
        test.transform.position = ActiveCamera.ViewportToWorldPoint(new Vector3(0.2f, 0.2f, 1));
        test.transform.localScale = Vector3.one * ActiveCamera.orthographicSize / _baseSize;
    }

    private void SetCamera(Camera camera)
    {
        ActiveCamera = camera;
        _baseSize = Camera.main.orthographicSize;
        transform.SetParent(camera.transform);
    }
}
