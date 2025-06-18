using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyNetworkManager : NetworkManager 
{
    [Header("PlayerPrefabs")]
    [SerializeField] private GameObject lobbyPlayerPref;
    [SerializeField] private GameObject gamePlayerPref;

    [Header("SceneLink")]
    [Scene] [SerializeField] private string mainScene = string.Empty;


    [SerializeField] private int playerCount = 0;

    private Transform currentSpawnTran;

    public int PlayerCount { get { return playerCount; } }

    public static bool isMulitplayer;

    private static MyNetworkManager instance;
    public static MyNetworkManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            return instance=NetworkManager.singleton as MyNetworkManager;
        }
    }

    private Dictionary<NetworkConnectionToClient, bool> readyStates = new Dictionary<NetworkConnectionToClient, bool>();

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        //base.OnServerAddPlayer(conn);
        switch (GameManager.Instance.CurrentEnumGameState)
        {
            case GameState.Lobby:

                currentSpawnTran = LobbySpawnControl.Instance.GetSpawnPoint(conn.connectionId);
                Debug.Log($"OnAddPlayer1{currentSpawnTran}");
                var player = Connect(conn, lobbyPlayerPref, currentSpawnTran);
                
                CSteamID steamID = SteamMatchmaking.GetLobbyMemberByIndex(LobbySteam.Instance.LobbyID, numPlayers-1);
                
                var playerInfo = conn.identity.GetComponent<LobbyPlayerInfo>();
                playerInfo.SetSteamId(steamID.m_SteamID);
                Debug.Log($"OnAddPlayer2{currentSpawnTran}");
                Debug.Log($"OnAddPlayer3{currentSpawnTran}");
                break;
            case GameState.InGame:

                currentSpawnTran = GetStartPosition();
                Connect(conn, gamePlayerPref, currentSpawnTran);
                CSteamID SteamID = SteamMatchmaking.GetLobbyMemberByIndex(LobbySteam.Instance.LobbyID, numPlayers - 1);
                var name=SteamHelper.GetPlayerName(SteamID);
                conn.identity.GetComponent<PlayerData>().SetPlayerName(name);
                break;
            default:
                break;
        }

    }
    private NetworkIdentity Connect(NetworkConnectionToClient conn,GameObject pref, Transform spawnTransform=null)
    {
        Debug.Log("Connect");
        

        GameObject playerInstance = Instantiate(pref,spawnTransform);


        playerInstance.transform.position = spawnTransform.position;
        playerInstance.transform.rotation = spawnTransform.rotation;
        Debug.Log($"Setting player position to: {spawnTransform.position}");
       
        //NetworkServer.Spawn(playerInstance, conn);
        //playerInstance.name = $"{pref.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, playerInstance);

        //NetworkServer.Spawn(playerInstance, conn);

        IncreaseCounter();
        if (NetworkServer.active)
        {
            readyStates[conn] = true;
        }
        else
        {
            readyStates[conn] = false;
        }
        return playerInstance.GetComponent<NetworkIdentity>();

    }




    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log("Disc");
        base.OnServerDisconnect(conn);
        readyStates.Remove(conn);
        DecreaseCounter();
        NetworkServer.RemovePlayerForConnection(conn);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ChangeScene(GameState state)
    {
        if(state == GameState.InGame) ServerChangeScene(mainScene);
    }
    public override void OnServerChangeScene(string newSceneName)
    {
        base.OnServerChangeScene(newSceneName);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
    }

    public void SetMultiplayer(bool value)
    { 
        isMulitplayer = value;
    }

    public void IncreaseCounter()
    {
        playerCount++;
    }

    public void DecreaseCounter()
    {
        playerCount--;
    }

    public void SetPlayerReady(NetworkConnectionToClient conn, bool ready)
    {
        if (readyStates.ContainsKey(conn))
            readyStates[conn] = ready;
        RpcUpdateReadyStates(AllPlayersReady());
    }

    public bool AllPlayersReady()
    {
        foreach (var state in readyStates.Values)
            if (!state) return false;
        return readyStates.Count > 0;
    }

    private void RpcUpdateReadyStates(bool allReady)
    {
        var lobbyUI = FindObjectOfType<LobbyUI>();
        if (lobbyUI != null)
            lobbyUI.UpdateStartButtonState(allReady);
    }
}
