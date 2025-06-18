using UnityEngine;

namespace InTerra
{
    [AddComponentMenu("/")]
    [ExecuteInEditMode]
    public class InTerra_HDRPTerrainShaderSelection : MonoBehaviour
    { 
        void Update()
        {
            Shader demoMatShader = gameObject.GetComponent<Terrain>().materialTemplate.shader;
            #if UNITY_2022_2_OR_NEWER
                if (!demoMatShader.isSupported)
                {
                    gameObject.GetComponent<Terrain>().materialTemplate.shader = Shader.Find("InTerra/HDRP Tessellation/Terrain (Lit with Features) 2022.2"); 
                }                
            #endif

            #if UNITY_2023_1_OR_NEWER
                if (!demoMatShader.isSupported)
                {
                    gameObject.GetComponent<Terrain>().materialTemplate.shader = Shader.Find("InTerra/HDRP Tessellation/Terrain (Lit with Features) 2023.1 or Heigher"); 
                }
            #endif
        }
    }
}