using UnityEngine;

namespace MUSOAR
{
    public interface ISaveable
    {
        public void GetSaveData(SaveData saveData);
        public void SetSaveData(SaveData saveData);
        public void RegisterSaveable();
        public void UnregisterSaveable();
    }
}
