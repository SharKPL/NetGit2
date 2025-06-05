using UnityEngine;
using UnityEngine.UI;

public class GamePlayerUI : MonoBehaviour
{
    [SerializeField] private Image healthbar;


    private void Start()
    {

        GlobalEventManager.PlayerHealthChanged.AddListener(SetHealth);
    }
    
    private void SetHealth(float amount)
    {
        Debug.Log("Health");
        if (healthbar != null)
        {
            healthbar.fillAmount = amount;
        }
    }
}
