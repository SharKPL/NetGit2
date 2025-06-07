using UnityEngine;

namespace MUSOAR
{
    public class SettingOption<T>
    {
        private T[] values;
        private int currentIndex;

        public T CurrentValue => values[currentIndex];

        public SettingOption(T[] possibleValues, T defaultValue)
        {
            values = possibleValues;
            currentIndex = System.Array.IndexOf(values, defaultValue);
        }

        public T Next()
        {
            currentIndex = (currentIndex + 1) % values.Length;
            return CurrentValue;
        }

        public T Previous()
        {
            currentIndex = (currentIndex - 1 + values.Length) % values.Length;
            return CurrentValue;
        }
    }
}
