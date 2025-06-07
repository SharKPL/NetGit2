using UnityEngine;
using UnityEngine.UI;
namespace MUSOAR
{
    public class UIPlayerStaminaBar : UIWindow
    {
        [SerializeField] private Slider staminaSlider;

        private void Awake()
        {
            staminaSlider.value = 1f;
        }

        private void OnEnable()
        {
            GlobalEventManager.OnPlayerStaminaChange.AddListener(UpdateStaminaUI);
        }

        private void OnDestroy()
        {
            GlobalEventManager.OnPlayerStaminaChange.RemoveListener(UpdateStaminaUI);
        }

        private void UpdateStaminaUI(float normalizedStamina)
        {
            staminaSlider.value = normalizedStamina;
        }
    }
}
