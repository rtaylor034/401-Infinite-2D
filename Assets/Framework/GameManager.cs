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

    //public static reference to this single instance (there is and only ever will be 1 GameManager object).
    public static GameManager GAME;
    public static Inputs INPUT;


    //-> Called before Start()
    private void Awake()
    {
        SSingletons();
        INPUT.Test.Testprompt.performed += TestSelector;
        Debug.Log("GameManager is initialized");
    }

    //-> Called on the very first frame of application run
    private void Start()
    {
        Debug.Log("Game is now running");
        board.CreateBoard();

    }

    #region Setups

    private void SSingletons()
    {
        INPUT = new Inputs();
        INPUT.Enable();
        GAME = this;
    }

    #endregion

    private void TestSelector(InputAction.CallbackContext context)
    {
        selector.Prompt(board.Units, null, x => Debug.Log("Nice"), null);
    }


    //-> Called every frame
    private void Update()
    {

    }

}
