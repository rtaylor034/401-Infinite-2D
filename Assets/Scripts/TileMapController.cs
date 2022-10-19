using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Vector3 = UnityEngine.Vector3;


public class TileMapController : MonoBehaviour
{

    public Tilemap tiles;
    public Vector3Int location;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        Vector3 mousePos = Input.mousePosition;
        location = tiles.WorldToCell(mousePos);
        print("Tile");
        
    }
}
