using Steamworks;
using System.Collections.Generic;
using UnityEngine;

public static class SteamHelper
{

    public static Texture2D GetAvatar(CSteamID steamID)
    {
        int imageID = SteamFriends.GetLargeFriendAvatar(steamID);
        if (imageID == -1) return null;

        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(imageID, out uint width, out uint height);

        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(imageID, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, false);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        return texture;
    }



    public static Sprite ConvertTextureToSprite(Texture2D texture)
    {
        if (texture == null) return null;
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }


    public static List<CSteamID> GetMembersInLobby()
    {
        List<CSteamID> members = new List<CSteamID>();
        int memberAmount = SteamMatchmaking.GetNumLobbyMembers(LobbySteam.Instance.LobbyID);

        for (int i = 0; i < memberAmount; i++)
        {
            CSteamID cSteamID = SteamMatchmaking.GetLobbyMemberByIndex(LobbySteam.Instance.LobbyID, i);
            members.Add(cSteamID);
        }

        return members;
    }

    public static string GetPlayerName(CSteamID? playerid=null,CSteamID? lobbyid = null, int index = -1)
    {
        if(playerid.HasValue && playerid.Value.m_SteamID != 0){
            var playerName = SteamFriends.GetFriendPersonaName(playerid.Value);
            return playerName;
        }
        if (lobbyid != null && lobbyid.HasValue && index != -1)
        {
            CSteamID steamID = SteamMatchmaking.GetLobbyMemberByIndex(lobbyid.Value, index - 1);
            var playerName = SteamFriends.GetFriendPersonaName(steamID);
            return playerName;
        }
        return null;
    }
}

