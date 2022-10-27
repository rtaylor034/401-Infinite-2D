using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Board _board;

    //public static reference to this single instance (there is and only ever will be 1 GameManager object).
    public static GameManager GAME;


    //-> Called before Start()
    private void Awake()
    {
        GAME = this;
        Debug.Log("GameManager is initialized");

    }

    //-> Called on the very first frame of application run
    private void Start()
    {
        Debug.Log("Game is now running");
        _board.CreateBoard();

    }


    //-> Called every frame
    private void Update()
    {

    }

}