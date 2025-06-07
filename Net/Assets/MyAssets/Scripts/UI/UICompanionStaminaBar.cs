using UnityEngine;
using UnityEngine.UI;
namespace MUSOAR
{
    public class UICompanionStamina : MonoBehaviour
    {
        [SerializeField] private Slider staminaSlider;

        private void Awake()
        {
            staminaSlider.value = 1f;
        }

        private void OnEnable()
        {
            GlobalEventManager.OnCompanionStaminaChange.AddListener(UpdateStaminaUI);
        }

        private void OnDisable()
        {
            GlobalEventManager.OnCompanionStaminaChange.RemoveListener(UpdateStaminaUI);
        }

        private void UpdateStaminaUI(float normalizedStamina)
        {
            staminaSlider.value = normalizedStamina;
        }
    }
}
