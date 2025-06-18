using UnityEngine;

namespace InTerra
{
    public class InTerra_GlobalData : ScriptableObject
    {
        [SerializeField, HideInInspector] public int trackTextureSize = 2048;
        [SerializeField, HideInInspector] public float trackArea = 40;
        [SerializeField, HideInInspector] public LayerMask trackLayer;
        [SerializeField, HideInInspector] public float trackUpdateTime = 0.1f;

        [SerializeField, HideInInspector] public int maskMapMode = 1;
        [SerializeField, HideInInspector] public bool disableTracks;
        [SerializeField, HideInInspector] public bool disableHeightmapBlending;
        [SerializeField, HideInInspector] public bool disableTerrainParallax;
        [SerializeField, HideInInspector] public bool disableObjectParallax;
        [SerializeField, HideInInspector] public bool disableNormalmap;
        [SerializeField, HideInInspector] public bool disablePuddles;

        [SerializeField, HideInInspector] public bool eightLayersPass;
    }
}
