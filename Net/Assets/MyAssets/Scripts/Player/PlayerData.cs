using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [SerializeField]private string playerName;

    public string PlayerName { get { return playerName; } }


    public void SetPlayerName(string name)
    {
        this.playerName = name;
    }
}
