using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// [ : ] <see cref="MonoBehaviour"/>
/// </summary>
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

        board.CreateBoard();

        NextTurn();

        
        //TEST MOVEMENT
        INPUT.Test.moveprompt.performed += c =>
        {
            Debug.Log("moveprompted");
            SELECTOR.Prompt(board.Units, Confirm);

            void Confirm(Selector.SelectorArgs sel)
            {
                if (sel.Selection is not Unit u) return;
                //funny lazer movement
                GameAction.Move.Prompt(new GameAction.Move.PathArgs(CurrentPlayer, u, 10) { CustomPathingRestrictions = new() { (prev, next) 
                    => { foreach (var i in BoardCoords.Indicies) if (next.Position[i] == u.Position[i]) return true; return false; } 
                }, MinDistance = 1}, _ => Debug.Log("moved"));
            }
            
        };
        //test undo
        INPUT.Test.undo.performed += c =>
        {
            Debug.Log($"UNDO CALL: {_game.Peek()}\n {UndoLastGameAction(false)}");
            
        };
    }

    //TBI
    private void EndGame()
    {
        if (!_gameActive) throw new Exception("Game is not active!");
        _gameActive = false;
        GameAction.Turn.OnPerform -= OnTurn;
    }

    /// <summary>
    /// Declares a <see cref="GameAction.Turn"/> action, transfering the turn to the next Player in the turn rotation.
    /// </summary>
    private void NextTurn()
    {
        var cnode = _turnOrder.Find(CurrentPlayer);
        GameAction.Turn.Declare(CurrentPlayer, (cnode is not null) ? cnode.Next.Value : _turnOrder.First.Value);
    }

    /// <summary>
    /// Subscribed to <see cref="GameAction.Turn.OnPerform"/><br></br>
    /// += from <see cref="StartGame"/> <br></br><br></br>
    /// (<inheritdoc cref="GameAction.Turn.OnPerform"/>)
    /// </summary>
    /// <param name="action"></param>
    private void OnTurn(GameAction.Turn action)
    {
        GameAction.EnergyChange.DeclareAsResultant(action, action.ToPlayer, e => e + 2);
        GameAction.EnergyChange.DeclareAsResultant(action, action.FromPlayer, e => 0);
    }

    #region GameActions

    /// <summary>
    /// Adds <paramref name="action"/> to the main action stack and performs it.<br></br>
    /// > Called by <see cref="GameAction.FinalizeDeclare(GameAction)"/>. <br></br>
    /// </summary>
    /// <param name="action"></param>
    /// <remarks>
    /// <i>All static Declare() methods of GameActions call FinalizeDeclare(). </i>
    /// </remarks>
    public void PushGameAction(GameAction action)
    {
        _game.Push(action);
        action.Perform();

        if (action is GameAction.Turn turn) HandleTurnAction(turn);
    }

    /// <summary>
    /// Undoes the last performed action and removes it from the main action stack.
    /// </summary>
    /// <param name="canUndoTurns"></param>
    /// <remarks>
    /// If <paramref name="canUndoTurns"/> is FALSE, this method will not undo <see cref="GameAction.Turn"/> actions. <br></br>
    /// Returns FALSE if the last action cannot be undone.
    /// </remarks>
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

    /// <summary>
    /// Acts as a <see cref="GameAction.Turn"/>'s Perform() method. <br></br>
    /// ><i> This method exists because <see cref="GameAction"/> does not have access to turn order.</i>
    /// </summary>
    /// <param name="turn"></param>
    /// <param name="undo"></param>
    /// <remarks>
    /// If <paramref name="undo"/> is TRUE, this acts as its Undo() method.
    /// </remarks>
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
