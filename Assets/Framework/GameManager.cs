using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

    /// <summary>
    /// The <see cref="Player"/> that currently has control of the Turn.
    /// </summary>
    public Player CurrentPlayer { get; private set; }
    /// <summary>
    /// The <see cref="GameSettings"/> of the current game.
    /// </summary>
    public GameSettings Settings { get; private set; }

    /// <summary>
    /// Contains all <see cref="Player"/> objects that are participating in the game, in correct turn order.
    /// </summary>
    private LinkedList<Player> _turnOrder;

    /// <summary>
    /// The stack of all <see cref="GameAction"/> objects that make up this game. <br></br>
    /// <i>Every game of 401 can be represented by a sequence of actions.</i>
    /// </summary>
    /// <remarks>
    /// <i>Referred to as the "main action stack" in docs.</i>
    /// </remarks>
    private Stack<GameAction> _game;

    /// <summary>
    /// TRUE if there is a game currently happening.
    /// </summary>
    private bool _gameActive = false;


    //Singleton instances
    /// <summary>
    /// [Singleton] <br></br>
    /// Main <see cref="GameManager"/> object.
    /// </summary>
    public static GameManager GAME;
    /// <summary>
    /// [Singleton] <br></br>
    /// Main <see cref="Selector"/> object.
    /// </summary>
    public static Selector SELECTOR;
    /// <summary>
    /// [Singleton] <br></br>
    /// Main <see cref="Inputs"/> object.
    /// </summary>
    public static Inputs INPUT;

    #region Setups

    private void SSingletons()
    {
        SELECTOR = new Selector();
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
        StartGame(GameSettings.STANDARD);
    }
    #endregion

    private void StartGame(GameSettings settings)
    {
        if (_gameActive) throw new Exception("Game is already active!");

        _gameActive = true;

        _game = new();

        _turnOrder = new();
        for (int i = 0; i < settings.TurnOrder.Count; i++)
        {
            _turnOrder.AddLast(settings.TurnOrder[i].CreateInstance());
        }

        Settings = settings;
        CurrentPlayer = Player.DummyPlayer;
        AbilityRegistry.Initialize(settings);

        board.CreateBoard();

        NextTurn();

        //TEST MOVEMENT
        INPUT.Test.moveprompt.performed += async _ =>
        {
            var sel = await SELECTOR.Prompt(board.Units.Where(u => u.Team == CurrentPlayer.Team));

            if (sel.Selection is not Unit u) return;
            //funny lazer  test
            GameAction.Declare(
                await GameAction.Move.Prompt(
                new GameAction.Move.PromptArgs.Pathed
                (CurrentPlayer, u, 10)
                {
                    CustomPathingRestrictions = new()
                    {
                        (prev, next) =>
                        {
                            foreach (var i in BoardCoords.Indicies)
                                if (next.Position[i] == u.Position[i]) return true;
                            return false;
                        }
                    },
                    MinDistance = 0
                },
                _ => print("MOVE CANCELLED")));

        };

        //TEST UNDO
        INPUT.Test.undo.performed += _ =>
        {
            Debug.Log($"UNDO CALL: {_game.Peek()}\n {UndoLastGameAction(true)}");
            
        };

        //TEST EFFECT
        INPUT.Test.effect.performed += async _ =>
        {
            var sel = await SELECTOR.Prompt(board.Units);
            if (sel.Selection is not Unit u) return;

            GameAction.Declare(new GameAction.InflictEffect(CurrentPlayer, new UnitEffect.Silence(1), u));
        };

        //TEST TURN
        INPUT.Test.turn.performed += _ =>
        {
            NextTurn();
        };

        //TEST ABILITIES
        INPUT.Test.ability1.performed += async _ => await __AbilityTest(0);
        INPUT.Test.ability2.performed += async _ => await __AbilityTest(2);
        async Task __AbilityTest(int id)
        {
            GameAction.Declare
                (await GameAction.PlayAbility.Prompt
                    (new GameAction.PlayAbility.PromptArgs
                    (CurrentPlayer, AbilityRegistry.Registry[id], board),
                    _ => Debug.Log("ABILITY CANCELLED")));
        }

        GameAction.Turn.ExternalResultantEvent += async _ =>
        {
            await Task.Delay(1000);
            Debug.Log("bruh1");
        };
        GameAction.Turn.ExternalResultantEvent += async _ =>
        {
            await Task.Delay(1000);
            Debug.Log("bruh2");
        };
    }

    //TBI
    private void EndGame()
    {
        if (!_gameActive) throw new Exception("Game is not active!");
        _gameActive = false;
    }
    
    /// <summary>
    /// Declares a <see cref="GameAction.Turn"/> action, transfering the turn to the next Player in the turn rotation. <br></br>
    /// > Also declares <see cref="GameAction.EnergyChange"/> resultant actions for standard energy gain.
    /// </summary>
    private void NextTurn()
    {
        var cnode = _turnOrder.Find(CurrentPlayer);
        var nextPlayer = (cnode is not null && cnode.Next is not null) ? cnode.Next.Value : _turnOrder.First.Value;

        GameAction.Declare(new GameAction.Turn(CurrentPlayer, nextPlayer)
            .AddResultant(new GameAction.EnergyChange(nextPlayer, nextPlayer, e => e + 2))
            .AddResultant(new GameAction.EnergyChange(nextPlayer, CurrentPlayer, e => e = 0))
            );
    }

    /// <summary>
    /// Adds <paramref name="action"/> to the main action stack.<br></br>
    /// </summary>
    /// <param name="action"></param>
    /// <remarks>
    /// <b>SAFETY:</b> Only should be called by <see cref="GameAction.Declare(GameAction)"/>.
    /// </remarks>
    public void PushGameAction(GameAction action)
    {
        _game.Push(action);
        if (action is GameAction.Turn turn) HandleTurnAction(turn);
    }

    //Consider making this part of GameSettings in the future
    //Maybe make a non-static GameActionManager object to handle events (so that event listeners dont have to be unsubscribed, a new GameActionManager can just be created).

    #region Turn GameAction Handling

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
        if (action is GameAction.Turn)
        {
            if (!canUndoTurns) return false;
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
    public void HandleTurnAction(GameAction.Turn turn, bool undo = false)
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
