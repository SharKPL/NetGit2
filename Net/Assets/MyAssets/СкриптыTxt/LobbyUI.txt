using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.UI;


public class LobbyUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text LobbyKey;
    [SerializeField] private Button quitLobbyBtn;
    [SerializeField] private Button hostStartBtn;
    [SerializeField] private Button ClientReadyBtn;
    [SerializeField] private Button ClientUnReadyBtn;

    private bool isReady = false;

    private void Start()
    {
        if (LobbySteam.Instance.LobbyKey == null) return;
        LobbyKey.text = $"Ключ:{LobbySteam.Instance.LobbyKey}";

        quitLobbyBtn.onClick.AddListener(QuitLobby);
        hostStartBtn.onClick.AddListener(StartGame);
        ClientReadyBtn.onClick.AddListener(OnReadyClicked);
        ClientUnReadyBtn.onClick.AddListener(OnUnReadyClicked);

        if (netIdentity.isServer)
        {
            ClientReadyBtn.gameObject.SetActive(false);
            ClientUnReadyBtn.gameObject.SetActive(false);
            hostStartBtn.gameObject.SetActive(true);
        }
        else
        {
            ClientReadyBtn.gameObject.SetActive(true);
            ClientUnReadyBtn.gameObject.SetActive(false);
            hostStartBtn.gameObject.SetActive(false);
        }
    }

    private void QuitLobby()
    {
        LobbySteam.Instance.Leave();
    }

    private void StartGame()
    {
        Debug.Log(MyNetworkManager.Instance.AllPlayersReady());
        if (MyNetworkManager.Instance.AllPlayersReady())
        {
            GameManager.Instance.SwitchState(GameState.InGame);
            MyNetworkManager.Instance.ChangeScene(GameState.InGame);
        }
    }

    private void OnReadyClicked()
    {
        isReady = true;
        CmdSetReady(true);
        ClientReadyBtn.gameObject.SetActive(false);
        ClientUnReadyBtn.gameObject.SetActive(true);
    }

    private void OnUnReadyClicked()
    {
        isReady = false;
        CmdSetReady(false);
        ClientReadyBtn.gameObject.SetActive(true);
        ClientUnReadyBtn.gameObject.SetActive(false);
    }

    [Command]
    private void CmdSetReady(bool ready)
    {
        MyNetworkManager.Instance.SetPlayerReady(netIdentity.connectionToClient, ready);
    }

    public void UpdateStartButtonState(bool allReady)
    {
        if (netIdentity.isServer)
            hostStartBtn.interactable = allReady;
    }
}
