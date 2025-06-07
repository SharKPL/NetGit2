using System.Collections.Generic;
using UnityEngine;

namespace MUSOAR
{
    public static class SaveableRegistry
    {
        private static List<ISaveable> registeredSaveables = new List<ISaveable>();
        
        public static void Register(ISaveable saveable)
        {
            if (!registeredSaveables.Contains(saveable))
            {
                registeredSaveables.Add(saveable);
                Debug.Log($"Зарегистрирован объект для сохранения: {saveable}");
            }
        }
        
        public static void Unregister(ISaveable saveable)
        {
            if (registeredSaveables.Contains(saveable))
            {
                registeredSaveables.Remove(saveable);
                Debug.Log($"Отменена регистрация объекта: {saveable}");
            }
        }
        
        public static List<ISaveable> GetAllSaveables()
        {
            return new List<ISaveable>(registeredSaveables);
        }
        
        public static void ClearAll()
        {
            registeredSaveables.Clear();
            Debug.Log("Реестр сохраняемых объектов очищен");
        }
    }
}