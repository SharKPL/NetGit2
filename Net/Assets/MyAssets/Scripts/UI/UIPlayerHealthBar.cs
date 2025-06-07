using UnityEngine;
using UnityEngine.UI;

namespace MUSOAR
{
    public class UIPlayerHealthBar : UIWindow
    {
        [SerializeField] private Slider healthSlider;

        private void Awake()
        {
            healthSlider.value = 1f;
        }

        private void OnEnable()
        {
            GlobalEventManager.OnPlayerHealthChange.AddListener(UpdateHealthUI);
        }

        private void OnDestroy()
        {
            GlobalEventManager.OnPlayerHealthChange.RemoveListener(UpdateHealthUI);
        }

        private void UpdateHealthUI(float normalizedHealth)
        {
            healthSlider.value = normalizedHealth;
        }
    }
}
