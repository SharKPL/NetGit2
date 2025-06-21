using UnityEngine;
using Mirror;

[RequireComponent(typeof(BoxCollider))]
public class TeleportZone : NetworkBehaviour
{
    //[SerializeField] private Vector3 coll_size;

    [SerializeField] private BoxCollider coll;


    //private void Start()
    //{
    //    coll.size = coll_size;
    //}
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.GetComponent<MUSOAR.PlayerMovement>())
        {
            //var tran = LobbySpawnControl.Instance.GetOnlySpawnPoint();
            var tran = MyNetworkManager.Instance.GetStartPosition();
            other.GetComponent<MUSOAR.PlayerMovement>().CmdTeleport(tran.position);
        }

    }
}
