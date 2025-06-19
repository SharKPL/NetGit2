using MUSOAR;
using UnityEngine;

public class NoteInteract : MonoBehaviour,IInteractable
{
    [SerializeField] private NoteSO note;

    [SerializeField] private AudioSource noteSource;
    [SerializeField] private AudioClip noteClip;



    public void Interact()
    {
        GlobalEventManager.ShowNote?.Invoke(note.text);
        AudioManager.PlaySound(noteSource, noteClip);
    }
}
