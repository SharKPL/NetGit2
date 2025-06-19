using Mirror;
using UnityEngine;
using System.Collections;

public class TriggerZone : NetworkBehaviour
{
    [SerializeField] private float timeToEnd;

    [SerializeField] private string endText;

    private void OnTriggerEnter(Collider other)
    {
        CmdEndGame();
    }

    private IEnumerator EndGameRoutine()
    {
        Debug.Log("EndGame1");
        InputManager.Instance.TurnAllControl(false);
        GlobalEventManager.ShowEndGameText?.Invoke(endText);
        yield return new WaitForSeconds(timeToEnd);
        MyNetworkManager.Instance.ChangeScene(GameState.Menu);
        Debug.Log("EndGame2");
    }

    [Command(requiresAuthority = false)]
    private void CmdEndGame()
    {
        RpcEndGame();
    }

    [ClientRpc]
    private void RpcEndGame()
    {
        StartCoroutine(EndGameRoutine());
    }
}
