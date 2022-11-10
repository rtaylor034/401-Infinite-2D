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

    public Player CurrentPlayer { get; private set; }

    private LinkedList<Player> _turnOrder;
    private Stack<GameAction> _game;
    private bool _gameActive = false;


    //Singleton instances
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
        StartGame();
    }

    private void Update()
    {

    }
    #endregion

    private void StartGame()
    {
        if (_gameActive) throw new Exception("Game is already active!");

        _gameActive = true;

        _game = new();

        _turnOrder = new();
        _turnOrder.AddLast(new Player(Player.ETeam.Blue));
        _turnOrder.AddLast(new Player(Player.ETeam.Red));

        CurrentPlayer = Player.DummyPlayer;
        GameAction.Turn.OnPerform += OnTurn;
        NextTurn();

        board.CreateBoard();
    }

    //TBI
    private void EndGame()
    {
        if (!_gameActive) throw new Exception("Game is not active!");
        _gameActive = false;

        GameAction.Turn.OnPerform -= OnTurn;
    }

    private void NextTurn()
    {
        var cnode = _turnOrder.Find(CurrentPlayer);
        GameAction.Turn.Declare(CurrentPlayer, (cnode is not null) ? cnode.Next.Value : _turnOrder.First.Value);
        
    }

    private void OnTurn(GameAction.Turn action)
    {
        GameAction.EnergyChange.DeclareAsResultant(action, action.ToPlayer, e => e + 2);
        GameAction.EnergyChange.DeclareAsResultant(action, action.FromPlayer, e => 0);
        //Debug.Log($"OnTurn() {action}");
    }

    #region GameActions

    /// <summary>
    /// Adds <paramref name="action"/> to this game's main GameAction stack and performs it.<br></br>
    /// (Should be called in every GameActions static Declare() method)
    /// </summary>
    /// <param name="action"></param>
    public void PushGameAction(GameAction action)
    {
        _game.Push(action);
        action.Perform();

        if (action is GameAction.Turn turn) HandleTurnAction(turn);
    }

    private bool UndoLastGameAction(bool canUndoTurns)
    {
        GameAction action = _game.Peek();
        if (action is GameAction.Turn turn)
        {
            if (!canUndoTurns) return false;
            HandleTurnAction(turn, true);
        }

        action.Undo();
        _game.Pop();
        return true;
    }

    private void HandleTurnAction(GameAction.Turn turn, bool undo = false)
    {
        if (undo)
        {
            CurrentPlayer = turn.FromPlayer;
            return;
        }

        CurrentPlayer = turn.ToPlayer;
    }

    #endregion
}
