using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private Board board;
    [SerializeField]
    private Selector selector;

    //Single instances
    public static GameManager GAME;
    public static Inputs INPUT;

    #region Setups

    private void SSingletons()
    {
        INPUT = new Inputs();
        INPUT.Enable();
        GAME = this;
    }

    #endregion

    #region Unity Messages
    private void Awake()
    {
        SSingletons();
    }

    private void Start()
    {
        board.CreateBoard();
    }

    private void Update()
    {

    }
    #endregion
}
