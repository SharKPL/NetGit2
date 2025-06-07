using UnityEngine;
using UnityEngine.UI;

namespace MUSOAR
{
    public class UIHeat : UIWindow
    {
        [SerializeField] private Slider heatSlider;

        private void Awake()
        {
            heatSlider.value = 1f;
        }

        private void OnEnable()
        {
            GlobalEventManager.OnPlayerHeatChange.AddListener(UpdateHeatUI);
        }

        private void OnDestroy()
        {
            GlobalEventManager.OnPlayerHeatChange.RemoveListener(UpdateHeatUI);
        }

        private void UpdateHeatUI(float normalizedHeat)
        {
            heatSlider.value = normalizedHeat;
        }
    }
}
