using UnityEngine;

namespace MUSOAR
{
    public abstract class UIWindow : MonoBehaviour
    {
        [HideInInspector] public bool IsOpen;

        public void OpenWindow()
        {
            gameObject.SetActive(true);
            IsOpen = true;
        }

        public void CloseWindow()
        {
            gameObject.SetActive(false);
            IsOpen = false;
        }
    }
}
