using UnityEngine;
using Mirror;

public class QuestTrigger : NetworkBehaviour
{
    public string StepName;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CmdQuestStepComplete();
        }

    }

    [Command(requiresAuthority = false)]
    private void CmdQuestStepComplete()
    {
        RpcQuestStepComplete();
    }
    [ClientRpc]
    private void RpcQuestStepComplete()
    {
        GlobalEventManager.OnQuestStepCompleted.Invoke(StepName);
    }

}
