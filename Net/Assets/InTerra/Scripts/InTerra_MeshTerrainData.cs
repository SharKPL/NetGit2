using UnityEngine;

namespace InTerra
{
    [AddComponentMenu("/")]
    public class InTerra_MeshTerrainData : MonoBehaviour
    {
        [SerializeField, HideInInspector] public Texture2D ControlMap;
        [SerializeField, HideInInspector] public Texture2D ControlMap1;
        [SerializeField, HideInInspector] public Texture2D HeightMap;
        [SerializeField, HideInInspector] public TerrainLayer[] TerrainLayers = new TerrainLayer[8];
    }
}
