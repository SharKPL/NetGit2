using UnityEngine;
using UnityEngine.UI;

namespace MUSOAR
{
    public class UIOxygen : UIWindow
    {
        [SerializeField] private Slider oxygenSlider;

        private void Awake()
        {
            oxygenSlider.value = 1f;
        }

        private void OnEnable()
        {
            GlobalEventManager.OnPlayerOxygenChange.AddListener(UpdateOxygenUI);
        }

        private void OnDestroy()
        {
            GlobalEventManager.OnPlayerOxygenChange.RemoveListener(UpdateOxygenUI);
        }

        private void UpdateOxygenUI(float normalizedOxygen)
        {
            oxygenSlider.value = normalizedOxygen;
        }
    }
}
