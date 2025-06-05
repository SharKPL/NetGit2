using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;
using Steamworks;

public class LobbyPlayerInfo : NetworkBehaviour
{
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private Image playerAvatar;

    private Callback<AvatarImageLoaded_t> avatarImageLoaded;

    [SyncVar(hook = nameof(HandleSteamIDChange))] private ulong steamid;

    public override void OnStartClient()
    {
        base.OnStartClient();
        avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
    }

    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        Debug.Log("OnLoadAvatar");
        if (callback.m_steamID.m_SteamID != steamid) return;
        Texture2D textureAvatar = SteamHelper.GetAvatar(callback.m_steamID);
        playerAvatar.sprite = SteamHelper.ConvertTextureToSprite(textureAvatar);
        Debug.Log(textureAvatar);
    }

    public void SetSteamId(ulong steamId)   
    {
        steamid = steamId;
    }

    private void HandleSteamIDChange(ulong oldSteamId, ulong newSteamId)
    {
        Debug.Log("Handle");
        var cSteamID = new CSteamID(newSteamId);
        playerName.text = SteamFriends.GetFriendPersonaName(cSteamID);

        Texture2D textureAvatar = SteamHelper.GetAvatar(cSteamID);

        playerAvatar.sprite = SteamHelper.ConvertTextureToSprite(textureAvatar);

    }
}
