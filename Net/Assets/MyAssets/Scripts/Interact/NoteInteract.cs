using MUSOAR;
using UnityEngine;

public class NoteInteract : MonoBehaviour,IInteractable
{
    [SerializeField] private NoteSO note;

    public void Interact()
    {
        GlobalEventManager.ShowNote?.Invoke(note.text);
    }
}
