using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;

public class GameControlManager : NetworkBehaviour
{
    public static GameControlManager Instance { get; private set; }

    [SyncVar] private Transform curTrans;


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
    public void InitializePlayer(NetworkConnection conn, ref NetworkIdentity player, Transform tran)
    {
        curTrans= tran;
        Debug.Log($"[Server] Initializing player {player.netId}");
        TargetSetupPlayer(conn, player, curTrans.position, curTrans.rotation);
    }

    [TargetRpc]
    public void TargetSetupPlayer(NetworkConnection target, NetworkIdentity player, Vector3 position, Quaternion rotation)
    {
        Debug.Log("TargetSetupPlayer" + player.name);

        StartCoroutine(TeleportPlayerRepeatedly(player.transform, position, rotation));
    }

    private IEnumerator TeleportPlayerRepeatedly(Transform player, Vector3 position, Quaternion rotation)
    {
        //yield return new WaitForSeconds(1);
        Debug.Log(player.position);
        Debug.Log($"CorStart, pos{position}");
        float endTime = Time.time + 0.5f;
        player.SetParent(curTrans);

        while (Time.time < endTime)
        {
            //player.transform.position = position;
            //player.transform.rotation = rotation;
            player.localPosition = Vector3.zero;
            player.localRotation = Quaternion.identity;
            Debug.Log("11");
            yield return null;
        }
        player.GetComponent<Animator>().applyRootMotion = true;

        Debug.Log($"CorStart, pos{position} end");
    }
}
