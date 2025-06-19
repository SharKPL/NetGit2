using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameUIControl: MonoBehaviour
{
    [SerializeField] private TMP_Text LobbyKey;
    [SerializeField] private GameObject PausePanel;
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button leaveBtn;

    [SerializeField] private GameObject interBtn;

    [SerializeField] private GameObject point;

    private System.Action<InputAction.CallbackContext> pauseDelegate;

    private void Start()
    {
        LobbyKey.text = $"Ключ:{LobbySteam.Instance.LobbyKey}";
        continueBtn.onClick.AddListener(OpenClosePanel);
        leaveBtn.onClick.AddListener(LeaveToMenu);

        pauseDelegate = ctx => OpenClosePanel();
        InputManager.Instance.GetPause().performed += pauseDelegate;

        GlobalEventManager.TurnPlayerControl.AddListener(ControlUI);

        Cursor.lockState = CursorLockMode.Locked;

        GlobalEventManager.showInteract.AddListener(ShowBtn);
    }

    private void ShowBtn(bool vis)
    {
        Debug.Log("ShowBtn");
        interBtn.SetActive(vis);
    }

    private void OnDestroy()
    {
        if (pauseDelegate != null)
            InputManager.Instance.GetPause().performed -= pauseDelegate;
    }

    private void OpenClosePanel()
    {
        PausePanel.SetActive(!PausePanel.activeSelf);
        GlobalEventManager.TurnSettings?.Invoke(PausePanel.activeSelf);
        GlobalEventManager.TurnPlayerControl?.Invoke(PausePanel.activeSelf);
        GameManager.Instance.SetGamePause(PausePanel.activeSelf);
    }

    private void ControlUI(bool isActive){
        InputManager.Instance.TurnPlayerControls(!isActive);
        var curCount = InputManager.Instance.TurnCursor(isActive);
        Debug.Log($"curCount:{curCount}");
        if (curCount > 0)
        {
            point.SetActive(false);
            return;
        }
        point.SetActive(true);
    }

    private void LeaveToMenu()
    {
        GameManager.Instance.SwitchState(GameState.Menu);
        LobbySteam.Instance.Leave();
    }
}
