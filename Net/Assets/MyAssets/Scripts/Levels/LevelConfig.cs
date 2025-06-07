using UnityEngine;

namespace MUSOAR
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "MUSOAR/SCENE/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Название сцены в Unity")]
        [SerializeField] private LevelData levelData;

        [Header("Основная информация")]
        [SerializeField] private string levelName;
        [SerializeField] private string description;
        [SerializeField] private bool isResearched;
        [SerializeField] private bool isVisibleInGame;

        public LevelData LevelData => levelData;
        public string LevelName => levelName;
        public string Description => description;
        public bool IsResearched => isResearched;
        public bool IsVisibleInGame => isVisibleInGame;
    }
}
