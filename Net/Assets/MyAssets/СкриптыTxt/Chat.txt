using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Mirror;
using UnityEngine.InputSystem;
using Steamworks;

public class Chat : NetworkBehaviour
{
    [SerializeField] private TMP_InputField input;
    [SerializeField] private Button msgBtn;
    [Range(5,50)]
    [SerializeField] private int msgLong=20;

    
    [SerializeField] private Message message;
    [SerializeField] private GameObject content;
    [SerializeField] private ScrollRect scroll;



    [SerializeField] private int startMsgCount=10;

    private CustomPool<Message> msgPool;

    private System.Action<InputAction.CallbackContext> openChatDelegate;
    private System.Action<InputAction.CallbackContext> sendMessageDelegate;


    public override void OnStartClient()
    {
        base.OnStartClient();
        msgPool = new CustomPool<Message>(message, startMsgCount, content.transform);
        for (int i = 0; i < startMsgCount; i++) 
        {
            msgPool.GetByIndex(i).SetPoolLink(msgPool);
        }
        msgBtn.onClick.AddListener(SendMessage);

        if (InputManager.Instance == null) return;
        

        openChatDelegate = ctx => OpenChat();
        sendMessageDelegate = ctx => SendMessage();
        
        InputManager.Instance.GetChatAction().performed += openChatDelegate;
        InputManager.Instance.GetSendMsgAction().performed += sendMessageDelegate;

        GlobalEventManager.TurnSettings.AddListener(CloseChatBySettings);
    }


    public override void OnStopClient()
    {
        base.OnStopClient();
        msgBtn.onClick.RemoveListener(SendMessage);
        if (InputManager.Instance == null) return;
        
        if (openChatDelegate != null)
            InputManager.Instance.GetChatAction().performed -= openChatDelegate;
        
        if (sendMessageDelegate != null)
            InputManager.Instance.GetSendMsgAction().performed -= sendMessageDelegate;
    }

    private void CloseChatBySettings(bool turn)
    {
        if(!gameObject.activeSelf) return;
        OpenChat();
    }

    private void OpenChat()
    {
        if (GameManager.Instance.GameInPause) return;
        Debug.Log("OpenChat");
        gameObject.SetActive(!gameObject.activeSelf);
        GlobalEventManager.TurnPlayerControl?.Invoke(gameObject.activeSelf);

    }

    [Command(requiresAuthority = false)]
    private void CmdSendMessage(string messageText, ulong senderSteamID)
    {
        if (string.IsNullOrEmpty(messageText)) return;
        RpcSendMessage(messageText, senderSteamID);
    }

    [ClientRpc]
    private void RpcSendMessage(string messageText, ulong senderSteamID)
    {
        Debug.Log("SendMessage");
        var messages = content.GetComponentsInChildren<Message>();
        if (messages.Length >= startMsgCount)
        {
            messages[0].ReleaseSelf();
        }

        var msg = msgPool.Get();

        CSteamID steamID = new CSteamID(senderSteamID);
        string playerName = SteamFriends.GetFriendPersonaName(steamID);

        string messageContent = messageText;
        if (messageContent.Length > msgLong)
        {
            messageContent = messageContent.Substring(0, msgLong) + "...";
        }

        string msgText = $"{playerName} : {messageContent}";
        msg.SetMsgText(msgText);
        msg.transform.SetAsLastSibling();
        scroll.verticalNormalizedPosition = 0;
        ScrollToBottom();
    }

    private void SendMessage()
    {
        if (string.IsNullOrEmpty(input.text)) return;

        // Получаем Steam ID текущего игрока
        ulong currentPlayerSteamID = SteamUser.GetSteamID().m_SteamID;

        CmdSendMessage(input.text, currentPlayerSteamID);
        input.text = "";
    }
    private void ScrollToBottom()
{
    StartCoroutine(ScrollToBottomNextFrame());
}

private IEnumerator ScrollToBottomNextFrame()
{
    yield return new WaitForEndOfFrame();
    
    scroll.verticalNormalizedPosition = 0; 
}
}
