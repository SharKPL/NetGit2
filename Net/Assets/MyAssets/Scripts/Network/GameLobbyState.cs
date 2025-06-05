using Mirror;
using UnityEngine;

public class GameLobbyState : MonoBehaviour,ILobbyState
{
    public void OnConnect(NetworkConnectionToClient conn)
    {
        Debug.Log("GameLobbyConnect");
    }


    public void OnDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log("GameLobbyDisconnect");
    }


}
