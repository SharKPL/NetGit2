using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;

public class GameControlManager : NetworkBehaviour
{
    public static GameControlManager Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    [Server]
    public void InitializePlayer(NetworkConnection conn, NetworkIdentity player, Transform tran)
    {
        Debug.Log($"[Server] Initializing player {player.netId}");
        TargetSetupPlayer(conn, player, tran.position, tran.rotation);

    }

    [TargetRpc]
    public void TargetSetupPlayer(NetworkConnection target, NetworkIdentity player, Vector3 position, Quaternion rotation)
    {
        Debug.Log("TargetSetupPlayer вызван " + player.name);

        StartCoroutine(TeleportPlayerRepeatedly(player, position, rotation));

 
    }

    private IEnumerator TeleportPlayerRepeatedly(NetworkIdentity player, Vector3 position, Quaternion rotation)
    {
        //yield return new WaitForSeconds(1);
        Debug.Log($"CorStart, pos{position}");
        float endTime = Time.time + 1f;

        while (Time.time < endTime)
        {
            player.transform.position = position;
            player.transform.rotation = rotation;
            yield return null;
        }
    }
}
