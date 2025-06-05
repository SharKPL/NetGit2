using UnityEngine;
using Mirror;
using System.Collections.Generic;
using UnityEngine.Events;



public enum GameState
{   
    Menu,
    Lobby,
    InGame,
}
public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    [SerializeField]private GameState currentEnumGameState;
    private ILobbyState currentState;

    private MenuLobbyState menuLobbyState;
    private GameLobbyState gameLobbyState;

    public ILobbyState CurrentState { get { return currentState; } }

    public GameState CurrentEnumGameState {  get { return currentEnumGameState; } }

    public UnityEvent<GameState> GameStateEvent;
    public static GameManager Instance=>instance;

    private bool gameInPause = false;

    public bool GameInPause { get { return gameInPause; } }

    private void Awake()
    {
        currentState = menuLobbyState;

        if (instance != null && instance!=this)
        {
            Destroy(gameObject);
        }
        else
        {
           instance = this;
           DontDestroyOnLoad(gameObject);
        }
    }

    public void SwitchState(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                break;
            case GameState.Lobby:
                currentState = menuLobbyState;
                break;
            case GameState.InGame:
                currentState = gameLobbyState;
                break;
            default:
                break;
        }
        currentEnumGameState = state;
        

        GameStateEvent?.Invoke(currentEnumGameState);
    }

    public void SetGamePause(bool pause)
    {
        gameInPause = pause;
    }


}
