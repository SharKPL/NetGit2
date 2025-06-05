using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private Image HealthBar;
    [SerializeField] private Image HealthBackImage;
    [SerializeField] private PlayerData playerData;

    private void Start()
    {
        GlobalEventManager.PlayerHealthChanged.AddListener(SetHealthBar);
        playerName.text = playerData.PlayerName;
        if (isLocalPlayer)
        {
            playerName.gameObject.SetActive(false);
            HealthBar.gameObject.SetActive(false);
            HealthBackImage.gameObject.SetActive(false);
        }
    }

    public void SetHealthBar(float normalizedValue)
    {
        if (HealthBar != null && isLocalPlayer)
            HealthBar.fillAmount = normalizedValue;
    }
}
