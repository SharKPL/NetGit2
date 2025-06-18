#define INTERRA_MESH_TERRAIN

#include "InTerra_HDRP_DefinedGlobalKeywords.hlsl"
#include "InTerra_Functions.hlsl"

//This function is not needed, but is there as a workaround to make Unity to add the line abow
void MeshTerrain_float(float3 normalWS, out float4 wNormal)
{
	wNormal = float4(normalWS, 0);

}

