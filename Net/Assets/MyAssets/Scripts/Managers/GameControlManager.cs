using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using Mirror.Examples.Benchmark;
using MUSOAR;

public class GameControlManager : NetworkBehaviour
{
    public static GameControlManager Instance { get; private set; }

    [SyncVar] private NetworkIdentity curIdentity;


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
    public void InitializePlayer(NetworkConnection conn, ref NetworkIdentity player, NetworkIdentity teleportIden)
    {

        //curTrans= teleportIden.transform;
        //Debug.Log($"[Server] Initializing player {player.netId}");

        //if (GameManager.Instance.CurrentEnumGameState == GameState.InGame)
        //{
        //    player.GetComponent<MUSOAR.PlayerMovement>().CmdTeleport(curTrans.position);
        //    return;
        //}
        curIdentity = teleportIden;

        //TargetSetupPlayer(conn, player, curTrans.position, curTrans.rotation);
        if (curIdentity == null)
        {
            Debug.LogError("teleportIden is NULL");
            
        }
        else if (player == null)
        {
            Debug.LogError("player is NULL");
        }
        else
        {
            RpcSetPlayer(player, curIdentity);
        }
    }


    [TargetRpc]
    public void TargetSetupPlayer(NetworkConnection target, NetworkIdentity player, Transform teleportTran)
    {
        Debug.Log("TargetSetupPlayer" + player.name);

        StartCoroutine(TeleportPlayerRepeatedly(player.transform, teleportTran));
    }
    [ClientRpc]
    private void RpcSetPlayer(NetworkIdentity player, NetworkIdentity teleportIden)
    {
        StartCoroutine(TeleportPlayerRepeatedly(player.transform, teleportIden.transform));
    }

    private IEnumerator TeleportPlayerRepeatedly(Transform player, Transform teleportTran)
    {
        if (teleportTran == null)
        {
            Debug.LogError("teleportTran null");
            yield return null;
        } 
        Debug.LogError($"teleportTran: {teleportTran==null}");
        //yield return new WaitForSeconds(1);
        Debug.Log(player.position);
        Debug.Log($"CorStart, pos{teleportTran.position}");
        float endTime = Time.time + 0.5f;
        player.SetParent(teleportTran);

        while (Time.time < endTime)
        {
            //player.transform.position = position;
            //player.transform.rotation = rotation;
            player.localPosition = Vector3.zero;
            player.localRotation = Quaternion.identity;
            Debug.Log("11");
            yield return null;
        }
        player.SetParent(null);
        //player.GetComponent<Animator>().applyRootMotion = true;

        Debug.Log($"CorStart, pos{teleportTran.position} end");
    }
}
