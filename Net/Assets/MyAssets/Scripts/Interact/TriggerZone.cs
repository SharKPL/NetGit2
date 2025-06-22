using Mirror;
using UnityEngine;
using System.Collections;

public class TriggerZone : NetworkBehaviour
{
    [SerializeField] private float timeToEnd;

    [SerializeField] private string endText;

    private void OnTriggerEnter(Collider other)
    {
        Debug.LogError("End");
        CmdEndGame();
    }

    private IEnumerator EndGameRoutine()
    {
        Debug.Log("EndGame1");
        if (InputManager.Instance != null)
            InputManager.Instance.TurnAllControl(false);
        else
            Debug.LogWarning("InputManager.Instance is null!");
        GlobalEventManager.ShowEndGameText?.Invoke(endText);
        yield return new WaitForSeconds(timeToEnd);
        MyNetworkManager.Instance.ChangeScene(GameState.Menu);
        Debug.Log("EndGame2");
    }

    [Command(requiresAuthority = false)]
    private void CmdEndGame()
    {
        Debug.LogError("CmdEndGame");
        RpcEndGame();
    }

    [ClientRpc]
    private void RpcEndGame()
    {
        Debug.LogError("RpcEndGame");
        StartCoroutine(EndGameRoutine());
    }
}
