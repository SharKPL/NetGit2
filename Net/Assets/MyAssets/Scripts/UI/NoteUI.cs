using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoteUI : MonoBehaviour
{
    [SerializeField] private GameObject note;

    [SerializeField] private Button CloseBtn;

    [SerializeField] private TMP_Text text;

    private void Start()
    {
        note.SetActive(false);
        CloseBtn.onClick.AddListener(CloseNote);
        GlobalEventManager.ShowNote.AddListener(ShowNote);
    }

    private void ShowNote(string text)
    {
        this.text.text = text;
        note.SetActive(true);
        GlobalEventManager.TurnPlayerControl?.Invoke(true);
        GlobalEventManager.showInteract?.Invoke(false);
        Cursor.lockState = note.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = note.activeSelf;
    }

    private void CloseNote()
    {
        note.SetActive(false);
        GlobalEventManager.TurnPlayerControl?.Invoke(false);
        GlobalEventManager.showInteract?.Invoke(true);
        Cursor.lockState = note.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = note.activeSelf;
    }


}
