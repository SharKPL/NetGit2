#if (defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN)) && !defined(TESSELLATION_ON)
    #define TESSELLATION_ON
#endif

#define TESSELLATION_SAMPLING
#undef _TERRAIN_PARALLAX

#if !(defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN))
    #include "InTerra_Functions.hlsl"
#endif


#define Height(i, lod) SAMPLE_TEXTURE2D_LOD(_Mask##i, sampler_Mask0, uv[i], (_MipMapLevel + (lod * (log2(max(_Mask##i##_TexelSize.z, _Mask##i##_TexelSize.w)) + 1))));

#define RemapHeight(i) mask[i] * _MaskMapRemapScale##i + _MaskMapRemapOffset##i;

#define TessellationHeight(i) ((mask[i] - _DiffuseRemapOffset##i.y) * DiffuseRemap(i).w * 0.01) + ((_DiffuseRemapOffset##i.z ) - ((DiffuseRemap(i).w * 0.01)/2));

void SampleHeights(out float4 mask[_LAYER_COUNT], float2 uv[_LAYER_COUNT], float4 blendMask[2], float lod)
{

#define SampleHeight(i, blendMask)                                      \
        UNITY_BRANCH if (blendMask > 0  && _LayerHasMask##i  > 0 )      \
        {                                                               \
            mask[i] = Height(i, lod);                                   \
            mask[i] = RemapHeight(i);                                   \
        }                                                               \
        else                                                            \
        {                                                               \
            mask[i] = float4(0, 0, 0.5, 0);                             \
        }                                                               \

    SampleHeight(0, blendMask[0].r);
    #ifndef _LAYERS_ONE
        SampleHeight(1, blendMask[0].g);
        #ifndef _LAYERS_TWO
            SampleHeight(2, blendMask[0].b);
            SampleHeight(3, blendMask[0].a);
            #ifdef _TERRAIN_8_LAYERS
                SampleHeight(4, blendMask[1].r);
                SampleHeight(5, blendMask[1].g);
                SampleHeight(6, blendMask[1].b);
                SampleHeight(7, blendMask[1].a);
            #endif
        #endif
    #endif
#undef SampleHeight
}

void SampleHeightTOL(out float4 mask[_LAYER_COUNT], float4 noTriplanarMask[_LAYER_COUNT], float2 uv[_LAYER_COUNT], float lod)
{
    mask[0] = Height(0, lod);
                        
    mask[1] = noTriplanarMask[1];
    #ifndef _LAYERS_TWO
        mask[2] = noTriplanarMask[2];            
        mask[3] = noTriplanarMask[3];
        #ifdef _TERRAIN_8_LAYERS
            mask[4] = noTriplanarMask[4];
            mask[5] = noTriplanarMask[5];
            mask[6] = noTriplanarMask[6];
            mask[7] = noTriplanarMask[7];
        #endif
    #endif
}

#if !(defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN))
    void Tessellation(float3 worldPos, float3 worldNormal, float2 splatBaseUV, out float3 displacement)
    {
        #ifdef ENABLE_TERRAIN_PERPIXEL_NORMAL
            float2 terrainNormalMapUV = (splatBaseUV + 0.5f) * _TerrainHeightmapRecipSize.xy;
            splatBaseUV *= _TerrainHeightmapRecipSize.zw;
            #ifndef TERRAIN_PERPIXEL_NORMAL_OVERRIDE
                float3 normalOS = SAMPLE_TEXTURE2D_LOD(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, terrainNormalMapUV, 0).rgb * 2 - 1;
                worldNormal = mul((float3x3)GetObjectToWorldMatrix(), normalOS);
            #endif
        #endif
#else
    #ifdef INTERRA_OBJECT
        void Tessellation_float(float3 worldPos, float3 normal, float4 mUV, float4 terrainNormals, float heightOffset, out float3 displacement, out float tessFactor, out float3 oWorldPos)
        {
        oWorldPos = worldPos;
        float4  worldNormal = float4(normal, 0);
    #else
        void Tessellation_float(float3 worldPos, float4 worldNormal, out float3 displacement, out float tessFactor)
        {
    #endif   
#endif


    #include "InTerra_SplatMapControl.hlsl"


    SampleHeights(mask, uvSplat, blendMask, lod);
    #if defined(TRIPLANAR) && !defined(TERRAIN_SPLAT_ADDPASS)
        #ifdef _TERRAIN_TRIPLANAR_ONE
            SampleHeightTOL(mask_front, mask, uvSplat_front, lod);
            SampleHeightTOL(mask_side, mask, uvSplat_side, lod);
        #else
            SampleHeights(mask_front, uvSplat_front, blendMask, lod);
            SampleHeights(mask_side, uvSplat_side, blendMask, lod);
        #endif 
        MaskWeight(mask, mask_front, mask_side, blendMask, triplanarWeights, _HeightTransition);
    #endif         


    #ifdef _TERRAIN_BLEND_HEIGHT
        #if !defined(_LAYERS_ONE)
            HeightBlend(mask, blendMask, _Tessellation_HeightTransition);
        #endif  
    #endif

    heightSum = HeightSum(mask, blendMask);

    float2 puddlesHeight = float2(0.0f, 1.0f);
    float horizontalWeight = 0.0f;
    float3 ripNormal = float3(0, 0, 1);

    puddlesHeight = (1 / (1 * pow(2, float2(_InTerra_GlobalPuddles.x, heightSum) * (-(100)))) + 1) * 0.5;
    puddlesHeight /= (puddlesHeight.r + puddlesHeight.g);
    horizontalWeight = smoothstep(0.999999f, 1.0f, saturate(triplanarWeights.y - ((triplanarWeights.x + triplanarWeights.z) * 100000000)));
    puddlesHeight.r *= horizontalWeight;


    #define TessllationHeight(i, blendMask)                             \
        UNITY_BRANCH if (blendMask > 0 && _LayerHasMask##i > 0 )        \
        {                                                               \
            mask[i] = ( puddlesHeight.r > mask[i]) ? ( _InTerra_GlobalPuddles.x ) : mask[i];\
            mask[i] = TessellationHeight(i);                            \
                                                                        \
        }                                                               \
        else                                                            \
        {                                                               \
            mask[i] = float4(0, 0, _DiffuseRemapOffset##i.z, 0);        \
        }                                                               \

        TessllationHeight(0, blendMask[0].r);
        #ifndef _LAYERS_ONE
            TessllationHeight(1, blendMask[0].g);
            #ifndef _LAYERS_TWO
                TessllationHeight(2, blendMask[0].b);
                TessllationHeight(3, blendMask[0].a);
                #ifdef _TERRAIN_8_LAYERS
                    TessllationHeight(4, blendMask[1].r);
                    TessllationHeight(5, blendMask[1].g);
                    TessllationHeight(6, blendMask[1].b);
                    TessllationHeight(7, blendMask[1].a);
                #endif
            #endif
        #endif
    #undef TessllationHeight

    float terrainHeightSum = HeightSum(mask, blendMask);

    #ifdef _TRACKS    
    if (_Tracks == 1)
    {
        UnpackTrackSplatValues(trackSplats);
        trackDepth = SAMPLE_TEXTURE2D_LOD(_InTerra_TrackTexture, SamplerState_Linear_Repeat, trackUV, 0).r;

        float4 td = TrackSplatValues(blendMask, trackSplats) * trackDist;

        float2 trackIntersect = float2(trackDepth.b, 1 - trackDepth.b);

        trackIntersect = (1 / (pow(2, float2(trackDepth.b, heightSum) * (-(_TrackTessallationHeightTransition)))) + 1) * 0.5;
        trackIntersect /= (trackIntersect.r + trackIntersect.g);

        terrainHeightSum = terrainHeightSum + (trackIntersect.r * - td.y * min(trackDepth.a, 0.35));
    }
    #endif

    #ifdef INTERRA_OBJECT
        float objectHeightMap = SAMPLE_TEXTURE2D_LOD(_MaskMap, sampler_MaskMap, mUV.xy * _BaseColorMap_ST.xy + _BaseColorMap_ST.zw, _MipMapLevel + (lod * _MipMapCount)).b * _MaskMapRemapScale.b + _MaskMapRemapOffset.b;
        float heightIntersect = ObjectTerrainIntersection(normal, heightOffset, mask, blendMask, objectHeightMap, _Tessellation_Sharpness);

        displacement = lerp((terrainHeightSum + _TerrainTessOffset), (objectHeightMap * _TessellationDisplacement + _TessellationOffset), heightIntersect.r);
        displacement *= normal.xyz;      
   
    #else
        displacement = terrainHeightSum.xxx;

        #if defined(TRIPLANAR) 
            displacement *= float3(worldNormal.x, worldNormal.y * triplanarWeights.y, worldNormal.z);
        #else
            displacement *=  worldNormal.xyz;
        #endif
     #endif

    #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN)
        #if SHADERPASS == SHADERPASS_SHADOWS
            tessFactor = _TessellationFactor * _TessellationShadowQuality;
        #else
            tessFactor = _TessellationFactor;
        #endif 
    #endif
}

#undef TESSELLATION_SAMPLING
