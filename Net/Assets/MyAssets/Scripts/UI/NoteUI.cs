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
        GlobalEventManager.TurnSettings.AddListener(CloseNoteBySettings);
    }

    private void ShowNote(string text)
    {
        this.text.text = text;
        note.SetActive(true);
        GlobalEventManager.TurnPlayerControl?.Invoke(true);
        //GlobalEventManager.showInteract?.Invoke(false);
        GameManager.Instance.SetGamePause(true);
    }

    private void CloseNote()
    {
        note.SetActive(false);
        GameManager.Instance.SetGamePause(false);
        GlobalEventManager.TurnPlayerControl?.Invoke(false);
        //GlobalEventManager.showInteract?.Invoke(true);
    }

    private void CloseNoteBySettings(bool turn)
    {
        if (!gameObject.activeSelf) return;
        note.SetActive(false);
        GlobalEventManager.TurnPlayerControl?.Invoke(false);
        //GlobalEventManager.showInteract?.Invoke(true);
    }


}
