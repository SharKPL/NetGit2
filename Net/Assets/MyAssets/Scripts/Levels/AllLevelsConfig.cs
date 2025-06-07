using System.Collections.Generic;
using UnityEngine;

namespace MUSOAR
{
    [CreateAssetMenu(fileName = "AllLevelsConfig", menuName = "MUSOAR/SCENE/AllLevelsConfig")]
    public class AllLevelsConfig : ScriptableObject
    {
        [SerializeField] private List<LevelConfig> levels;

        public List<LevelConfig> Levels => levels;
    }
}
