using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Steamworks;
using TMPro;

public class Menu : MonoBehaviour
{
    [SerializeField] private Button playBtn;
    [SerializeField] private Button quitBtn;
    [SerializeField] private Button createBtn;
    [SerializeField] private Button connectBtn;
    [SerializeField] private Button closeConnectUIBtn;

    [SerializeField] private TMP_InputField keyField;
    [SerializeField] private GameObject LobbyUIPrefab;
    [SerializeField] private GameObject menuPrefab;
    void Start()
    {
        playBtn.onClick.AddListener(OpenLobby);
        quitBtn.onClick.AddListener(QuitGame);

        createBtn.onClick.AddListener(StartGame);
        connectBtn.onClick.AddListener(ConnectGame);
        closeConnectUIBtn.onClick.AddListener(CloseLobby);

    }

    private void ConnectGame()
    {
        connectBtn.interactable = false;
        LobbySteam.Instance.JoinForKey(keyField.text, connectBtn);
    }

    private void StartGame()
    {
        createBtn.interactable = false;
        GameManager.Instance.SwitchState(GameState.Lobby);
        LobbySteam.Instance.CreateLobby(ELobbyType.k_ELobbyTypePublic, createBtn);
    }

    private void OpenLobby()
    {
        
        menuPrefab.SetActive(false);
        LobbyUIPrefab.SetActive(true);
    }

    private void CloseLobby()
    {
        
        LobbyUIPrefab.SetActive(false);
        menuPrefab.SetActive(true);
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
