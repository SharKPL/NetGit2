using UnityEngine;

namespace InTerra
{
    [AddComponentMenu("/")]
    [ExecuteInEditMode]
    public class InTerra_DemoTerrainLayersMaskMaps : MonoBehaviour
    {
        public TerrainLayer rock;
        public Texture2D rockDefaoultMaskMap;
        public Texture2D rockNormalMaskMap;

        public TerrainLayer snow;
        public Texture2D snowDefaoultMaskMap;
        public Texture2D snowNormalMaskMap;


        void Update()
        {
            Terrain terrain = GetComponent<Terrain>();
            if (InTerra_Data.GetGlobalData().maskMapMode == 1)
            {
                if(terrain.terrainData.terrainLayers[0] == rock)
                {
                    rock.maskMapTexture = rockDefaoultMaskMap;
                }
                if (terrain.terrainData.terrainLayers[1] == snow)
                {
                    snow.maskMapTexture = snowDefaoultMaskMap;
                }
            }
            if (InTerra_Data.GetGlobalData().maskMapMode == 2)
            {
                if (terrain.terrainData.terrainLayers[0] == rock)
                {
                    rock.maskMapTexture = rockNormalMaskMap;
                }
                if (terrain.terrainData.terrainLayers[1] == snow)
                {
                    snow.maskMapTexture = snowNormalMaskMap;
                }
            }


        }
    }
}