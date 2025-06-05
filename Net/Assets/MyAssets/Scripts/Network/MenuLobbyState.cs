using Mirror;
using UnityEngine;

public class MenuLobbyState : MonoBehaviour,ILobbyState
{
    public void OnConnect(NetworkConnectionToClient conn)
    {
        Debug.Log("MenuLobbyConnect");
    }


    public void OnDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log("MenuLobbyDisconnect");
    }
}
