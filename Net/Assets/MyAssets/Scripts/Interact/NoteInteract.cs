using Mirror;
using MUSOAR;
using UnityEngine;

public class NoteInteract : NetworkBehaviour,IInteractable
{
    [SerializeField] private NoteSO note;

    [SerializeField] private AudioSource noteSource;
    [SerializeField] private AudioClip noteClip;

    [SerializeField] private bool isQuest;
    [SyncVar][SerializeField] private bool isComplete;

    [SerializeField] string stepName;



    public void Interact()
    {
        Debug.LogError("Quest");
        if (isQuest && !isComplete)
        {
            Debug.LogError("QuestNote");
            CmdQuestStepComplete();
            isComplete = true;
        }
        GlobalEventManager.ShowNote?.Invoke(note.text);
        AudioManager.PlaySound(noteSource, noteClip);
    }

    [ClientRpc]
    private void RpcQuestStepComplete()
    {
        GlobalEventManager.OnQuestStepCompleted?.Invoke(stepName);
    }

    [Command(requiresAuthority = false)]
    private void CmdQuestStepComplete()
    {
        RpcQuestStepComplete();
    }
}
