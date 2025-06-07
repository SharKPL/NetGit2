using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace MUSOAR
{
    public class CreditsWindow : UIWindow
    {
        [Header("Кнопки")]
        [SerializeField] private Button backButton;

        private IReturnableWindow menuManager;

        public void Initialize(IReturnableWindow manager)
        {
            menuManager = manager;
            OpenWindow();
        }

        private void Awake()
        {
            backButton.onClick.AddListener(OnBackButtonClick);
        }

        private void OnBackButtonClick()
        {
            menuManager?.ReturnToPrevious();
            CloseWindow();
        }
    }
}
