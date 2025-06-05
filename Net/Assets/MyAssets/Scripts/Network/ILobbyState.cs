using Mirror;
using UnityEngine;

public interface ILobbyState
{
    public void OnConnect(NetworkConnectionToClient conn);

    public void OnDisconnect(NetworkConnectionToClient conn);
    
}
