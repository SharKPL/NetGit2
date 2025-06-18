    float2 uvSplat[_LAYER_COUNT];
    float4 mask[_LAYER_COUNT];
    #ifdef TRIPLANAR
        float2 uvSplat_front[_LAYER_COUNT], uvSplat_side[_LAYER_COUNT];
        float4 mask_front[_LAYER_COUNT], mask_side[_LAYER_COUNT];
    #endif
    #ifdef _TERRAIN_DISTANCEBLEND
        float2 distantUV[_LAYER_COUNT];
        float4 dMask[_LAYER_COUNT];
        #ifdef TRIPLANAR
            float2 distantUV_front[_LAYER_COUNT], distantUV_side[_LAYER_COUNT];
            float4 dMask_front[_LAYER_COUNT], dMask_side[_LAYER_COUNT];
        #endif
    #endif

    #ifdef _TRACKS
        float4 trackSplats[_LAYER_COUNT];
        float4 trackSplatsColor[_LAYER_COUNT];
    #endif

    //-------------------- MIP MAP LOD ------------------------- 
    #if defined(PARALLAX) || defined(TESSELLATION_ON)
        float lod = smoothstep(_MipMapFade.x, _MipMapFade.y, (distance(worldPos, _WorldSpaceCameraPos)));       
    #endif

    //====================================================================================
    //--------------------------------- SPLAT MAP CONTROL --------------------------------
    //====================================================================================
    float4 blendMask[2];
    blendMask[0] = 0;
    blendMask[1] = 0;
    float4 trackDepth = 0;
    half4  mixedMask = 0;
    half4  dMixedMask = 0;

    #ifndef TESSELLATION_SAMPLING
        occlusion = mixedMask.g;
        metallic = mixedMask.r;
    #endif

    float halfTrackArea = _InTerra_TrackArea * 0.5f;
    float _InTerra_TrackFading = 28;
    float2 trackUV = float2((worldPos.x - _InTerra_TrackPosition.x) + (halfTrackArea), -(worldPos.z - _InTerra_TrackPosition.z - (halfTrackArea))) * 1.0f / _InTerra_TrackArea;

    float trackDist = smoothstep(_InTerra_TrackArea - 1.0f, _InTerra_TrackArea - _InTerra_TrackFading, (distance(worldPos, float3(_WorldSpaceCameraPos.x, _WorldSpaceCameraPos.y, _WorldSpaceCameraPos.z))));

    float2 minDist = step(float2(0.0f, 0.0f), trackUV);
    float2 maxDist = step(float2(0.0f, 0.0f), 1.0 - trackUV);
    trackDist *= (minDist.x * minDist.y * maxDist.x * maxDist.y);

    #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN) 
        float2 splatBaseUV = (worldPos.xz - _TerrainPosition.xz) * (1 / _TerrainSize.xz);

        #ifndef _LAYERS_ONE     
            float2 splatMapUV = (splatBaseUV * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy; 

            #ifndef TESSELLATION_SAMPLING
                blendMask[0] = SAMPLE_TEXTURE2D(_Control, sampler_Control, splatMapUV);
            #else
                blendMask[0] = SAMPLE_TEXTURE2D_LOD(_Control, sampler_Control, splatMapUV, 0);
            #endif
            #ifdef _TERRAIN_8_LAYERS
                #ifndef TESSELLATION_SAMPLING
                    blendMask[1] = SAMPLE_TEXTURE2D(_Control1, sampler_Control, splatMapUV);
                #else
                    blendMask[1] = SAMPLE_TEXTURE2D_LOD(_Control1, sampler_Control, splatMapUV, 0);
                #endif
            #endif 
        #else
            blendMask[0] = float4(1, 0, 0, 0);
            blendMask[1] = float4(0, 0, 0, 0);
        #endif 
    #else
        float2 blendUV0 = (splatBaseUV.xy * (_Control0_TexelSize.zw - 1.0f) + 0.5f) * _Control0_TexelSize.xy;                      
        #ifndef TESSELLATION_SAMPLING
            blendMask[0] = SAMPLE_TEXTURE2D(_Control0, sampler_Control0, blendUV0);
        #else
            blendMask[0] = SAMPLE_TEXTURE2D_LOD(_Control0, sampler_Control0, blendUV0, 0);
        #endif

        #ifdef _TERRAIN_8_LAYERS
            #ifndef TESSELLATION_SAMPLING
                blendMask[1] = SAMPLE_TEXTURE2D(_Control1, sampler_Control0, blendUV0);
            #else
                blendMask[1] = SAMPLE_TEXTURE2D_LOD(_Control1, sampler_Control0, blendUV0, 0);
            #endif           
        #endif
    #endif

    float2 tintUV = splatBaseUV * _TerrainColorTintTexture_ST.xy + _TerrainColorTintTexture_ST.zw;
    float2 normalTintUV = splatBaseUV * _TerrainNormalTintTexture_ST.xy + _TerrainNormalTintTexture_ST.zw;  
 

    float3 flipUV = worldNormal.rgb < 0 ? -1 : 1;
    float3  triplanarWeights = abs(worldNormal.rgb);
    triplanarWeights = pow(triplanarWeights, _TriplanarSharpness);
    triplanarWeights = triplanarWeights / (triplanarWeights.x + triplanarWeights.y + triplanarWeights.z);

    #ifdef INTERRA_OBJECT            
        TriplanarOneToAllSteep(blendMask, (1 - terrainNormals.w));
    #else
        TriplanarOneToAllSteep(blendMask, (1 - triplanarWeights.y));
    #endif
 

    #if defined(_LAYERS_TWO) && defined(INTERRA_OBJECT)
            blendMask[0].r = _ControlNumber == 0 ? blendMask[0].r : _ControlNumber == 1 ? blendMask[0].g : _ControlNumber == 2 ? blendMask[0].b : blendMask[0].a;
            blendMask[0].g = 1 - blendMask[0].r;
    #endif

    #if defined(_TERRAIN_BLEND_HEIGHT) && !defined(_TERRAIN_BASEMAP_GEN) && !defined(_LAYERS_ONE) 
       float4 splatControlSum = blendMask[0] + blendMask[1];
       blendMask[0] = (splatControlSum.r + splatControlSum.g + splatControlSum.b + splatControlSum.a == 0.0f ? 1 : blendMask[0]); //this is preventing the black area when more than one pass
    #endif

    #if defined(_TERRAIN_BASEMAP_GEN_TRIPLANAR) || defined(_LAYERS_ONE) 
        blendMask[0] = float4(1, 0, 0, 0);
        blendMask[1] = float4(0, 0, 0, 0);
    #endif

    #ifdef _TRACKS
        float4 tBlendMask[2];
        tBlendMask[0] = blendMask[0];
        tBlendMask[1] = blendMask[1];
    #endif

    float4 origBlendMask[2] = blendMask;
    #if defined(_TERRAIN_DISTANCEBLEND)        
        float4 dBlendMask[2];
        float4 dOrigBlendMask[2] = blendMask;
        dBlendMask[0] = blendMask[0];
        dBlendMask[1] = blendMask[1];

        float dist = smoothstep(_HT_distance.x, _HT_distance.y, (distance(worldPos, _WorldSpaceCameraPos)));
        float sampleDistMask = dist < 0.001f ? 1 : 0;
        if(sampleDistMask)
        {
            dBlendMask[0] *= sampleDistMask;
           #ifdef _LAYERS_EIGHT
               dBlendMask[1] *= sampleDistMask;
           #endif
        }
    #endif

    float heightSum = 0.5f;
    float dHeightSum = 0.5f;

    //================================================================================
    //-------------------------------------- UVs -------------------------------------
    //================================================================================
    #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN)
        float3 positionOffset = _WorldMapping ? worldPos : (worldPos - _TerrainPosition);

        #ifndef TRIPLANAR
            float distortion = 0;
            #if defined(INTERRA_OBJECT) && !defined(TESSELLATION_SAMPLING)
                distortion = worldNormal.y > 0.5 ? 0 : (1 - worldNormal.y) * _SteepDistortion * objectAlbedo.r;
            #endif

            UvSplat(uvSplat, positionOffset, distortion);
        #else
            float offsetZ = -flipUV.z * worldPos.y;
            float offsetX = -flipUV.x * worldPos.y;
            #if defined(INTERRA_OBJECT) 
                offsetZ = _DisableOffsetY == 1 ? -flipUV.z * worldPos.y : heightOffset * -flipUV.z + (worldPos.z);
                offsetX = _DisableOffsetY == 1 ? -flipUV.x * worldPos.y : heightOffset * -flipUV.x + (worldPos.x);
            #endif
              
            offsetZ -= _TerrainPosition.z;
            offsetX -= _TerrainPosition.x;

            UvSplat(uvSplat, uvSplat_front, uvSplat_side, positionOffset, offsetZ, offsetX, flipUV);
        #endif
    #else
        
        #ifdef _TERRAIN_BASEMAP_GEN
            float2 uv = splatBaseUV.xy;
        #else
            float2 uv = _WorldMapping ? (worldPos.xz / _TerrainSizeXZPosY.xy) : splatBaseUV.xy;
        #endif
        #ifndef TRIPLANAR
            UvSplat(uvSplat, uv);
        #else
            UvSplat(uvSplat, uvSplat_front, uvSplat_side, worldPos, uv, flipUV);
        #endif
    #endif          

    //-------------------- PARALLAX OFFSET -------------------------                  
    #if defined(PARALLAX) && defined(_TERRAIN_PARALLAX) && !defined(TESSELLATION_ON) && !defined(_TERRAIN_BASEMAP_GEN)
    if (_Terrain_Parallax == 1)
    {
        ParallaxUV(uvSplat, tangentViewDirTerrain, lod);
    }
    #endif

    //--------------------- DISTANCE UV ------------------------
    #ifdef _TERRAIN_DISTANCEBLEND
        DistantUV(distantUV, uvSplat);
        #ifdef TRIPLANAR
            #ifdef _TERRAIN_TRIPLANAR_ONE
                distantUV_front[0] = uvSplat_front[0] * (_DiffuseRemapOffset0.r + 1) * _HT_distance_scale;
                distantUV_side[0] = uvSplat_side[0] * (_DiffuseRemapOffset0.r + 1) * _HT_distance_scale;
            #else
                DistantUV(distantUV_front, uvSplat_front);
                DistantUV(distantUV_side, uvSplat_side);
            #endif  
        #endif
    #endif
