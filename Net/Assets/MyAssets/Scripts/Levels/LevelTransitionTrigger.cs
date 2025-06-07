using UnityEngine;
using Zenject;

namespace MUSOAR
{
    public class LevelTransitionTrigger : MonoBehaviour
    {
        [Header("Настройки перехода")]
        [SerializeField] private LevelConfig targetLevel;
        [SerializeField] private bool isTransitionActive = true;
        [SerializeField] private bool showTransitionWindow = true;

        private LevelTransitionWindow transitionWindow;

        [Inject]
        private void Construct(LevelTransitionWindow transitionWindow)
        {
            this.transitionWindow = transitionWindow;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isTransitionActive || !other.CompareTag("Player")) return;

            transitionWindow.SetStartTransition(targetLevel, showTransitionWindow);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!isTransitionActive || !other.CompareTag("Player")) return;

            transitionWindow.SetEndTransition();
        }
    }
}
