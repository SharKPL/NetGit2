#define INTERRA_MESH_TERRAIN

#include "InTerra_HDRP_DefinedGlobalKeywords.hlsl"
#include "InTerra_Functions.hlsl"

void MeshTerrain_float(float3 viewDirWS, float3 normalWS, out float3 tangentViewDir)
{
    tangentViewDir = 0;
    #if defined (_TERRAIN_PARALLAX) 
        half3 axisSign = normalWS < 0 ? -1 : 1;
        half3 tangentY = normalize(cross(normalWS.xyz, half3(1e-5f, 1e-5f, axisSign.y)));
        half3 bitangentY = normalize(cross(tangentY.xyz, normalWS.xyz)) * axisSign.y;
        half3x3 tbnY = half3x3(tangentY, bitangentY, normalWS);
        tangentViewDir =  mul(tbnY, viewDirWS);
    #endif
}