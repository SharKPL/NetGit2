using System;
using UnityEngine;
using Zenject;

namespace MUSOAR
{
    public class CursorController : IInitializable, IDisposable
    {
        private bool isCursorVisible = false;

        public void Initialize()
        {
            GlobalEventManager.OnPauseStateChanged.AddListener(ToggleCursor);
            GlobalEventManager.OnShowCursor.AddListener(ToggleCursor);
        }

        public void Dispose()
        {
            GlobalEventManager.OnPauseStateChanged.RemoveListener(ToggleCursor);
            GlobalEventManager.OnShowCursor.RemoveListener(ToggleCursor);
        }

        private void ToggleCursor(bool isPaused)
        {
            Debug.Log("ToggleCursor: " + isPaused);
            isCursorVisible = isPaused;
            SetCursor(isCursorVisible);
        }

        private void SetCursor(bool isVisible)
        {
            Cursor.visible = isVisible;
            Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        }

        //TODO: Убрать установку курсора из awake камеры
    }
}
