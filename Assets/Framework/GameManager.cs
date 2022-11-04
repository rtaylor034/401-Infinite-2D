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

    //TBI
    public Player CurrentPlayer { get; private set; }

    private LinkedList<Player> _turnOrder;

    //TBI
    private List<GameAction> _game;


    //Single instances
    public static GameManager GAME;
    public static Selector SELECTOR;
    public static Inputs INPUT;

    #region Setups

    private void SSingletons()
    {
        INPUT = new Inputs();
        INPUT.Enable();
        GAME = this;
        SELECTOR = selector;
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

    public void StartGame()
    {
        _turnOrder.AddFirst(new Player(Player.ETeam.Blue));
        _turnOrder.AddAfter(_turnOrder.First, new Player(Player.ETeam.Red));
        
        _game = new();
    }

    public void FinalizeGameAction(GameAction action)
    {
        _game.Add(action);
        action.Perform();

        //Turn Handling
        if (action is GameAction.Turn turn) HandleTurnAction(turn);
    }

    public void HandleTurnAction(GameAction.Turn turn, bool undo = false)
    {
        if (undo)
        {
            CurrentPlayer = turn.FromPlayer;
            return;
        }

        CurrentPlayer = turn.ToPlayer;

    }

}
