using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace MUSOAR
{
    public static class GlobalEventManager
    {
        public static UnityEvent<float> OnPlayerHealthChange = new UnityEvent<float>();
        public static UnityEvent<float> OnPlayerStaminaChange = new UnityEvent<float>();
        public static UnityEvent OnPlayerDie = new UnityEvent();
        public static UnityEvent<float> OnPlayerOxygenChange = new UnityEvent<float>();
        public static UnityEvent<float> OnPlayerHeatChange = new UnityEvent<float>();
        public static UnityEvent OnSuitUpgrade = new UnityEvent();
        //
        public static UnityEvent<bool> OnPauseStateChanged = new UnityEvent<bool>();
        public static UnityEvent<bool> OnShowCursor = new UnityEvent<bool>();
        public static UnityEvent<bool> OnPlayerUseWorkbench = new UnityEvent<bool>();

        //
        public static UnityEvent<bool> OnCharacterSwitch = new UnityEvent<bool>();
        public static UnityEvent<float> OnCompanionStaminaChange = new UnityEvent<float>();
        public static UnityEvent OnCompanionSuitUpgrade = new UnityEvent();
        //
        public static UnityEvent OnQuestCompleted = new UnityEvent();
        public static UnityEvent<object> OnConditionCompleted = new UnityEvent<object>();
        public static UnityEvent OnQuestViewUpdated = new UnityEvent();
        public static UnityEvent OnLevelLoaded = new UnityEvent();

    }
}
