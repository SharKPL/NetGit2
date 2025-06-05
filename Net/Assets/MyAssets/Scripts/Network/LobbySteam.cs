using System;
using Steamworks;
using System.Collections;
using Mirror;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class Lobby
{
    public CSteamID lobbyID;
    public string name;
    public string lobbyKey;

    public Lobby(CSteamID lobbyID, string name,string key)
    {
        this.lobbyID = lobbyID;
        this.name = name;
        this.lobbyKey = key;
    }
}

public struct LobbyMemberData
{
    public string Name;
    public Texture AvatarTexture;
}



public class LobbySteam : MonoBehaviour
{
    private const string HOST_ADDRESS_KEY = "HostAddress";
    private const string LOBBY_KEY = "KeyOfLobby";

    [SerializeField] private string lobbyKey;

    private static LobbySteam instance;

    private Dictionary<CSteamID, LobbyMemberData> LobbyMembers = new Dictionary<CSteamID, LobbyMemberData>();
    public static LobbySteam Instance { get { return instance; } }
    public CSteamID LobbyID;

    public string LobbyKey {  get { return lobbyKey; } }


    public List<Lobby> allLobbies = new List<Lobby>();



    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> joinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<LobbyChatUpdate_t> lobbyChatUpdate;

    protected Callback<LobbyMatchList_t> lobbyMatchList;


    private void Awake()
    {
        if (instance == null)
            instance = this;

        
    }

    private void Start()
    {
        if (!SteamManager.Initialized) return;

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobbyMatchList = Callback<LobbyMatchList_t>.Create(OnLobbyMatchList);
        lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);


    }

    public void CreateLobby(ELobbyType lobbyType, Button btn)
    {
        lobbyKey = GetRandomKey();
        FindLobby(LobbyKey);
        StartCoroutine(StartLobbyRoutine(lobbyType, btn));
    }

    private IEnumerator StartLobbyRoutine(ELobbyType lobbyType, Button btn,float time=1f)
    {
        yield return new WaitForSeconds(time);
        btn.interactable = true;
        Debug.Log("StartLobby");
        SteamMatchmaking.CreateLobby(lobbyType, MyNetworkManager.Instance.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        Debug.Log("LobbyCreated");
        if (callback.m_eResult != EResult.k_EResultOK && CheckLobbyKey()) 
        {
            GameManager.Instance.SwitchState(GameState.Menu);
            return; 
        }

        string lobbyName = SteamFriends.GetFriendPersonaName(SteamUser.GetSteamID());

        LobbyID = new CSteamID(callback.m_ulSteamIDLobby);

        SteamMatchmaking.SetLobbyData(LobbyID, HOST_ADDRESS_KEY, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(LobbyID, LOBBY_KEY, lobbyKey);
        SteamMatchmaking.SetLobbyData(LobbyID, "name", lobbyName);

        if (!NetworkServer.active)
        {
            Debug.Log("StartHost");
            MyNetworkManager.Instance.StartHost();
        }

    }

    private string GetRandomKey()
    {
        string id = string.Empty;
        for(int i = 0; i < 7; i++)
        {
            int random = UnityEngine.Random.Range(0, 36);
            if (random < 26)
            {
                id += (char)(random+65);
            }
            else
            {
                id += (random - 26).ToString();
            }
        }
        return id;
    }

    private void FindLobby(string key)
    {
        Debug.Log(LobbyKey);
        SteamMatchmaking.AddRequestLobbyListStringFilter(LOBBY_KEY, key,ELobbyComparison.k_ELobbyComparisonEqual);
        SteamMatchmaking.RequestLobbyList();
    }



    private void OnLobbyMatchList(LobbyMatchList_t callback)
    {
        allLobbies.Clear();
        for (int i = 0; i < callback.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            CSteamID ownerID = SteamMatchmaking.GetLobbyOwner(lobbyID);
            string lobbyKey = SteamMatchmaking.GetLobbyData(lobbyID, LOBBY_KEY);

            allLobbies.Add(new Lobby(lobbyID, SteamMatchmaking.GetLobbyData(lobbyID, "name"), lobbyKey));
        }

        allLobbies.Sort((a, b) => SteamMatchmaking.GetNumLobbyMembers(b.lobbyID).CompareTo(SteamMatchmaking.GetNumLobbyMembers(a.lobbyID)));
    }

    public bool CheckLobbyKey()
    {
        return allLobbies.Count > 0;
    }

    private Lobby GetLobbyByKey(string key)
    {
        Debug.Log($"Count:{allLobbies.Count > 0}");
        foreach (var lobby in allLobbies)
        {
            if (lobby.lobbyKey == key)
            {
                return lobby;
            }
        }
        return null;
    }


    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        Debug.Log("OnLobbyChatUpdate");
        if(callback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeEntered)
        {
            string name = SteamFriends.GetFriendPersonaName((CSteamID)callback.m_ulSteamIDUserChanged);
            Debug.Log($"Name:{name}");
        }
        else if(callback.m_rgfChatMemberStateChange== (uint)EChatMemberStateChange.k_EChatMemberStateChangeLeft ||
            callback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeDisconnected)
        {
            string name = SteamFriends.GetFriendPersonaName((CSteamID)callback.m_ulSteamIDUserChanged);
            Debug.Log($"Name:{name}");
        }
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (callback.m_bLocked)
        {
            SteamMatchmaking.LeaveLobby((CSteamID)callback.m_ulSteamIDLobby);
            return;
        }


        Debug.Log($"Entered Lobby {LobbyID}");
        LobbyID = new CSteamID(callback.m_ulSteamIDLobby);


        if (NetworkServer.active)
            return;
        MyNetworkManager.Instance.SetMultiplayer(true);

        MyNetworkManager.Instance.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(LobbyID.m_SteamID), HOST_ADDRESS_KEY);
        lobbyKey = SteamMatchmaking.GetLobbyData(new CSteamID(LobbyID.m_SteamID), LOBBY_KEY);
        if(NetworkClient.active) return;
        MyNetworkManager.Instance.StartClient();
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    public void JoinForKey(string key, Button btn)
    {
        StartCoroutine(JoinForKeyRoutine(key, btn));
    }
    private IEnumerator JoinForKeyRoutine(string key, Button btn, float time = 1f)
    {
        FindLobby(key);
        yield return new WaitForSeconds(time);
        Lobby lobby = GetLobbyByKey(key);
        if (lobby != null)
        {
            SteamMatchmaking.JoinLobby(lobby.lobbyID);
        }
        else
        {
            Debug.Log("Cant Find Lobby");
            btn.interactable = true;
        }
    }

    public void Leave()
    {
        SteamMatchmaking.LeaveLobby(LobbyID);
        Debug.Log(LobbyID);
        if (NetworkServer.active)
        {
            MyNetworkManager.Instance.StopHost();
        }
        if (NetworkClient.active)
        {
            MyNetworkManager.Instance.StopClient();
        }


    }

}
