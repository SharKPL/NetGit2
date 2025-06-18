#ifdef INTERRA_OBJECT
    void ObjectIntegration_float(float heightOffset, float3 worldPos, float3 tangentViewDirTerrain, float3 worldViewDir, float3 worldNormal, float3 worldTangent, float3 worldBitangent, float4 terrainNormals, float4 objectAlbedo, float3 objectNormal, float4 objectMask, half3 objectEmission, out half3 albedo, out half3 mixedNormal, out half smoothness, out half metallic, out half occlusion, out half3 emission)
#else
    #ifdef INTERRA_MESH_TERRAIN
        void SplatmapMix_float(float3 tangentViewDirTerrain,  float3 worldNormal, float3 worldPos,float3 worldTangent, float3 worldBitangent, out half3 albedo, out half3 mixedNormal, out half smoothness, out half metallic, out half occlusion)

    #else
        #include "InTerra_Functions.hlsl"
        void SplatmapMix(float4 uvMainAndLM, float2 uvS[_LAYER_COUNT], float3 worldNormal, float3 tangentViewDirTerrain, float3 worldPos, out half weight, out half4 mixedDiffuse, out half smoothness, out half metallic, out half occlusion, inout half3 mixedNormal)
    #endif
#endif
{
    half4  mixedMask = 0;
    half4  dMixedMask = 0;
    occlusion = 1;
    metallic = 0;
    #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN)
        half4 mixedDiffuse;       
    #endif

    float2 uvSplat[_LAYER_COUNT];
    half4 mask[_LAYER_COUNT];
    #ifdef TRIPLANAR
        float2 uvSplat_front[_LAYER_COUNT], uvSplat_side[_LAYER_COUNT];
        half4 mask_front[_LAYER_COUNT], mask_side[_LAYER_COUNT];
    #endif
    #ifdef _TERRAIN_DISTANCEBLEND
        float2 distantUV[_LAYER_COUNT];
        half4 dMask[_LAYER_COUNT];
        #ifdef TRIPLANAR
            float2 distantUV_front[_LAYER_COUNT], distantUV_side[_LAYER_COUNT];
            half4 dMask_front[_LAYER_COUNT], dMask_side[_LAYER_COUNT];
        #endif
    #endif
    #ifdef _TRACKS
        float4 trackSplats[_LAYER_COUNT];
        float4 trackSplatsColor[_LAYER_COUNT];
        float4 trackDepth = 0;
    #endif

    //====================================================================================
    //--------------------------------- SPLAT MAP CONTROL --------------------------------
    //====================================================================================
    half4 blendMask[2];

    blendMask[0] = float4(1, 0, 0, 0);
    blendMask[1] = float4(0, 0, 0, 0);

    #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN) 
        float2 terrainUV = (worldPos.xz - _TerrainPosition.xz) * (1 / _TerrainSize.xz);
        #ifndef _LAYERS_ONE     
            float2 splatMapUV = (terrainUV * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
            blendMask[0] = SAMPLE_TEXTURE2D(_Control, sampler_Control, splatMapUV);
            #ifdef _LAYERS_EIGHT
                blendMask[1] = SAMPLE_TEXTURE2D(_Control1, sampler_Control, splatMapUV);
            #endif 
        #endif          
    #else
        float2 terrainUV = uvMainAndLM.xy;
        blendMask[0] = SAMPLE_TEXTURE2D(_Control, sampler_Control, uvMainAndLM.xy);
         #ifdef _LAYERS_EIGHT
            blendMask[1] = SAMPLE_TEXTURE2D(_Control1, sampler_Control, uvMainAndLM.xy);
         #endif 

        #ifdef _LAYERS_EIGHT
            #ifdef TERRAIN_SPLAT_ADDPASS
                weight = 0;
            #else
                weight = 1;
            #endif
        #else
            #if defined(_LAYERS_TWO) && defined(TERRAIN_SPLAT_ADDPASS) 
                blendMask[0] = float4(0, 0, 0, 0);
            #endif
            weight = dot(blendMask[0], 1.0h);

            #ifdef TERRAIN_SPLAT_ADDPASS             
                    clip(weight <= 0.005h ? -1.0h : 1.0h);
                #endif
            #ifndef _TERRAIN_BASEMAP_GEN
                // Normalize weights before lighting and restore weights in final modifier functions so that the overal
                // lighting result can be correctly weighted.
                blendMask[0] /= (weight + HALF_MIN);
            #endif 
        #endif    
    #endif

    #if defined(_LAYERS_TWO) && !defined(INTERRA_MESH_TERRAIN) 
        blendMask[0].r = _ControlNumber == 0 ? blendMask[0].r : _ControlNumber == 1 ? blendMask[0].g : _ControlNumber == 2 ? blendMask[0].b : blendMask[0].a;
        blendMask[0].g = 1 - blendMask[0].r;
    #endif

    float2 tintUV = terrainUV * _TerrainColorTintTexture_ST.xy + _TerrainColorTintTexture_ST.zw;
    float2 normalTintUV = terrainUV * _TerrainNormalTintTexture_ST.xy + _TerrainNormalTintTexture_ST.zw;

    float3 tint = SAMPLE_TEXTURE2D(_TerrainColorTintTexture, SamplerState_Linear_Repeat, tintUV).rgb;
    float3 normalTint = UnpackNormalScale(SAMPLE_TEXTURE2D(_TerrainNormalTintTexture, SamplerState_Linear_Repeat, normalTintUV), 1).xyz;

    #if defined(_TERRAIN_BLEND_HEIGHT) && !defined(_TERRAIN_BASEMAP_GEN) && !defined(_LAYERS_ONE) && !defined(_LAYERS_EIGHT)
        if (_HeightmapBlending == 1)
        {
            blendMask[0] = (blendMask[0].r + blendMask[0].g + blendMask[0].b + blendMask[0].a == 0.0f ? 1.000f : blendMask[0]); //this is preventing the black area when more than one pass
        }
    #endif

    #if !defined(_TERRAIN_BASEMAP_GEN)
        float3 flipUV = worldNormal.rgb < 0 ? -1 : 1;
        float3 triplanarWeights = abs(worldNormal.rgb);
        triplanarWeights = pow(triplanarWeights, _TriplanarSharpness);
        triplanarWeights = triplanarWeights / (triplanarWeights.x + triplanarWeights.y + triplanarWeights.z);

        #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN)
            float weight;
        #endif

        #if defined(INTERRA_OBJECT)
            TriplanarOneToAllSteep(blendMask, (1 - terrainNormals.w), weight);
        #else
            TriplanarOneToAllSteep(blendMask, (1 - triplanarWeights.y), weight);
        #endif       
    #endif  


    half4 origBlendMask[2] = blendMask;

    #if defined(_TERRAIN_DISTANCEBLEND)        
        half4 dBlendMask[2];
        half4 dOrigBlendMask[2] = blendMask;
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

    #ifdef PARALLAX 
        float lod = 0;
        #if defined(INTERRA_OBJECT)  
            UNITY_BRANCH if ((_Terrain_Parallax == 1) || (_Object_Parallax == 1))
        #else  
            UNITY_BRANCH if (_Terrain_Parallax == 1)
        #endif
            {
                lod =  smoothstep(_MipMapFade.x, _MipMapFade.y, (distance(worldPos, _WorldSpaceCameraPos)));
            }
    #endif

    //================================================================================
    //-------------------------------------- UVs -------------------------------------
    //================================================================================
    #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN) 
        #if defined(INTERRA_OBJECT)      
            #ifndef _OBJECT_TRIPLANAR            
                _SteepDistortion = worldNormal.y > 0.5 ? 0 : (1 - worldNormal.y) * _SteepDistortion;
                _SteepDistortion *= objectAlbedo.r;
            #else            
                _SteepDistortion = 0;
            #endif
        #endif

        float3 positionOffset = _WorldMapping ? worldPos : (worldPos - _TerrainPosition);

        #ifndef TRIPLANAR
            UvSplat(uvSplat, positionOffset);
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
        #ifndef TRIPLANAR
            UvSplat(uvSplat, uvS, uvMainAndLM.xy);
        #else
            UvSplat(uvSplat, uvSplat_front, uvSplat_side, worldPos, uvS, uvMainAndLM.xy, flipUV);
        #endif
    #endif          

    //-------------------- PARALLAX OFFSET -------------------------                  
    #if defined(_TERRAIN_PARALLAX) && !defined(_TERRAIN_BASEMAP_GEN)
        float3 viewDirTerrain = float3(0.0f, 0.0f, 0.0f);
        UNITY_BRANCH if (_Terrain_Parallax == 1)
        {
            viewDirTerrain = tangentViewDirTerrain;
            ParallaxUV(uvSplat, viewDirTerrain, blendMask, triplanarWeights.y, lod);
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

    //======================================================================
    //-------------------------  OBJECT INTERSECTION  ----------------------
    //======================================================================
    #ifdef INTERRA_OBJECT	
        float steeptriplanarWeights = _SteepIntersection == 1 ? saturate(worldNormal.y + _Steepness) : 1;
        float intersect1 = smoothstep(_Intersection.y, _Intersection.x, heightOffset) * steeptriplanarWeights;
        float intersect2 = smoothstep(_Intersection2.y, _Intersection2.x, heightOffset) * (1 - steeptriplanarWeights);
        float intersection = intersect1 + intersect2;
        float intersectNormal = smoothstep(_NormIntersect.y, _NormIntersect.x, heightOffset);
              
        if (intersection < 1e-5f)
        {
            blendMask[0] = 0.0f;
            blendMask[1] = 0.0f;
            #if defined(_TERRAIN_DISTANCEBLEND)
                dBlendMask[0] = 0.0f;
                dBlendMask[1] = 0.0f;
            #endif 
        }      
    #endif
        
    //====================================================================================
    //-----------------------------------  MASK MAPS  ------------------------------------
    //====================================================================================  
    #if defined(TRIPLANAR) && !defined(_TERRAIN_BASEMAP_GEN)        
        #ifdef _TERRAIN_TRIPLANAR_ONE
            SampleMask(mask, uvSplat, blendMask, triplanarWeights.y + (1 - blendMask[0].r));
            SampleMaskTOL(mask_front, mask, uvSplat_front, triplanarWeights.z);
            SampleMaskTOL(mask_side, mask, uvSplat_side, triplanarWeights.x);
        #else
            SampleMask(mask, uvSplat, blendMask, triplanarWeights.y);
            SampleMask(mask_front, uvSplat_front, blendMask, triplanarWeights.z);
            SampleMask(mask_side, uvSplat_side, blendMask, triplanarWeights.x);
        #endif 
        MaskWeight(mask, mask_front, mask_side, blendMask, triplanarWeights, _HeightTransition);
    #else
        SampleMask(mask, uvSplat, blendMask, 1.0f);
    #endif
    #ifdef _TERRAIN_DISTANCEBLEND		
        #if defined(TRIPLANAR) && !defined(_TERRAIN_BASEMAP_GEN)          
            #ifdef _TERRAIN_TRIPLANAR_ONE
                SampleMask(dMask, distantUV, dBlendMask, saturate(triplanarWeights.y + (1 - dBlendMask[0].r)));
                SampleMaskTOL(dMask_front, dMask, distantUV_front, blendMask[0].r * triplanarWeights.z  );
                SampleMaskTOL(dMask_side, dMask, distantUV_side, blendMask[0].r * triplanarWeights.x  );
            #else
                SampleMask(dMask, distantUV, dBlendMask, triplanarWeights.y);
                SampleMask(dMask_front, distantUV_front, dBlendMask, triplanarWeights.z);
                SampleMask(dMask_side, distantUV_side, dBlendMask, triplanarWeights.x);
            #endif
            MaskWeight(dMask, dMask_front, dMask_side, dBlendMask, triplanarWeights, _Distance_HeightTransition);
        #else
            SampleMask(dMask, distantUV, dBlendMask, 1.0f);
        #endif
        dBlendMask = dOrigBlendMask;
    #endif
    blendMask = origBlendMask;         
    
    //========================================================================================
    //------------------------------ HEIGHT MAP SPLAT BLENDINGS ------------------------------
    //========================================================================================
    #if defined(_TERRAIN_BLEND_HEIGHT) && !defined(_LAYERS_ONE) && !defined(TERRAIN_SPLAT_ADDPASS)
        #ifdef _TERRAIN_BASEMAP_GEN
        if (_NumLayersCount <= 4)
        #endif
        {
            if (_HeightmapBlending == 1)
            {
                HeightBlend(mask, blendMask, _HeightTransition);
                #ifdef _TERRAIN_DISTANCEBLEND
                    HeightBlend(dMask, dBlendMask, _Distance_HeightTransition);
                #endif
            }
        }  
    #endif
    //-------------------- HEIGHTMAP OBJECT INTERSECTION ----------------
    #ifdef INTERRA_OBJECT
        objectMask.rgba = objectMask.rgba * _MaskMapRemapScale.rgba + _MaskMapRemapOffset.rgba;
        half height = objectMask.b;

        half terrainHeith = 0.5f;
        if (_HeightmapBlending == 1)
        {
            terrainHeith = lerp(HeightSum(mask, blendMask), 1, intersection);
        }

        float2 heightIntersect = (1 / (1 * pow(2, float2(((1 - intersection) * height), (intersection * terrainHeith)) * (-(_Sharpness)))) + 1) * 0.5;
        heightIntersect /= (heightIntersect.r + heightIntersect.g);

        heightSum = (heightSum * heightIntersect.g) + (height * heightIntersect.r);

        if (heightIntersect.g  < 1e-3f)
        {
            blendMask[0] = 0.0f;
            blendMask[1] = 0.0f;
            #if defined(_TERRAIN_DISTANCEBLEND)
                dBlendMask[0] = 0.0f;
                dBlendMask[1] = 0.0f;
            #endif 
        }
    #endif

    MaskSplatWeight(mask, blendMask, mixedMask);
    #ifdef _TERRAIN_DISTANCEBLEND       
        MaskSplatWeight(dMask, dBlendMask, dMixedMask);
        dMixedMask = lerp(mixedMask, dMixedMask, _HT_cover);
        mixedMask = lerp(mixedMask, dMixedMask, dist);
    #endif
    #if defined(_TERRAIN_MASK_MAPS) || defined(_TERRAIN_NORMAL_IN_MASK)
        occlusion = mixedMask.g;
        metallic = mixedMask.r;
    #endif
    #if defined(_TERRAIN_BLEND_HEIGHT) && !defined(_LAYERS_ONE) && !defined(TERRAIN_SPLAT_ADDPASS)
        if (_HeightmapBlending == 1)
        {
            heightSum = mixedMask.b;
            #ifdef _TERRAIN_DISTANCEBLEND
                if(sampleDistMask)
                {
                    dBlendMask[0] *= sampleDistMask;
                    #ifdef _LAYERS_EIGHT
                        dBlendMask[1] *= sampleDistMask;
                    #endif
                }
            #endif
        }
    #endif 

    //=======================================================================
    //--------------------  PUDDLES & RAINDROPS NORMALS  --------------------
    //=======================================================================
    #if !defined(_TERRAIN_BASEMAP_GEN) && defined(_PUDDLES) 
        float2 puddlesHeight = float2(0.0f , 1.0f);
        float horizontalWeight = 0.0f;
        float3 ripNormal = float3(0, 0, 1);

        puddlesHeight = (1 / (1 * pow(2, float2(_InTerra_GlobalPuddles.x, heightSum) * (-(100)))) + 1) * 0.5;
        puddlesHeight /= (puddlesHeight.r + puddlesHeight.g);

        float3 puddleWeight = pow(abs(worldNormal.rgb), _TriplanarSharpness);
        puddleWeight = puddleWeight / (puddleWeight.x + puddleWeight.y + puddleWeight.z);

        horizontalWeight = smoothstep(0.9999f, 1.0f, saturate(puddleWeight.y - ((puddleWeight.x + puddleWeight.z) * 100000000.0f)));
        puddlesHeight.r *= horizontalWeight;
        _InTerra_GlobalRaindropRipples.z = 1.0f / _InTerra_GlobalRaindropRipples.z * 0.1f;

        if (_InTerra_GlobalRaindropRipples.x > 0.0f)
        { 
            if (puddlesHeight.x > 0.5f)
            {
                float rainIndex =  1 / (min(5.0f, _InTerra_GlobalRaindropRipples.x) * 4.0f);

                for (float i = 5.0; i > 4.0; i -= rainIndex)
                {
                    ripNormal = BlendNormal(ripNormal, RainRipples(worldPos.zx * _InTerra_GlobalRaindropRipples.z * i + i*0.24f ,  i, i * 0.25f));
                }
            }

            float2  uvRefractOffset = ripNormal.xy * 0.01;
            for (int i = 0; i < _LAYER_COUNT; ++i)
            {
                uvSplat[i] += uvRefractOffset;
            }

            #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN)
                ripNormal = WorldTangent(worldTangent, worldBitangent, ripNormal);
            #endif 
        }
    #endif

    //========================================================================================
    //-------------------------------  ALBEDO, SMOOTHNESS & NORMAL ---------------------------
    //========================================================================================
    #if !(defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN))
        float3 worldTangent;
        float3 worldBitangent;
    #endif 

    #if defined(TRIPLANAR) && !defined(_TERRAIN_BASEMAP_GEN)
        float4 frontDiffuse;
        float3 frontNormal;
        float4 sideDiffuse;
        float3 sideNormal;

        #ifdef _TERRAIN_TRIPLANAR_ONE
            #ifndef TERRAIN_SPLAT_ADDPASS
                float tolWeightY = saturate(triplanarWeights.y + (1 - blendMask[0].r));
            #else
                float tolWeightY = 1.0f;
            #endif
            SampleSplat(uvSplat, blendMask, tolWeightY, mask, mixedDiffuse, mixedNormal);
            SampleSplatTOL(frontDiffuse, frontNormal, uvSplat_front, blendMask, triplanarWeights.z, mask);
            SampleSplatTOL(sideDiffuse, sideNormal, uvSplat_side, blendMask, triplanarWeights.x, mask);
        #else
            SampleSplat(uvSplat, blendMask, triplanarWeights.y, mask, mixedDiffuse, mixedNormal);
            SampleSplat(uvSplat_front, blendMask, triplanarWeights.z, mask, frontDiffuse, frontNormal);
            SampleSplat(uvSplat_side, blendMask, triplanarWeights.x, mask, sideDiffuse, sideNormal);
        #endif 
    #else
        SampleSplat(uvSplat, blendMask, 1.0f, mask, mixedDiffuse, mixedNormal);
    #endif

    #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN)
        mixedNormal = WorldTangent(worldTangent, worldBitangent, mixedNormal);
    #endif 

    #if defined(TRIPLANAR) && !defined(_TERRAIN_BASEMAP_GEN)
        mixedDiffuse = mixedDiffuse + frontDiffuse + sideDiffuse;
        mixedNormal = TriplanarNormal(mixedNormal, worldTangent, worldBitangent, frontNormal, sideNormal, triplanarWeights, flipUV);
    #endif

    #ifdef _TERRAIN_DISTANCEBLEND     
        float4 distantDiffuse;   
        float3 distantNormal;

        #if defined(TRIPLANAR) && !defined(_TERRAIN_BASEMAP_GEN)
            float4 dFrontDiffuse;
            float3 dFontNormal;
            float4 dSideDiffuse;
            float3 dSideNormal;

            #ifdef _TERRAIN_TRIPLANAR_ONE
                #ifndef TERRAIN_SPLAT_ADDPASS
                    tolWeightY = saturate(triplanarWeights.y + (1 - dBlendMask[0].r));
                #endif
                SampleSplat(distantUV, dBlendMask, tolWeightY, dMask, distantDiffuse, distantNormal);
                SampleSplatTOL(dFrontDiffuse, dFontNormal, distantUV_front, dBlendMask, triplanarWeights.z, dMask);
                SampleSplatTOL(dSideDiffuse, dSideNormal, distantUV_side, dBlendMask, triplanarWeights.x, dMask);
            #else
                SampleSplat(distantUV, dBlendMask, triplanarWeights.y, dMask, distantDiffuse, distantNormal);
                SampleSplat(distantUV_front, dBlendMask, triplanarWeights.z, dMask, dFrontDiffuse, dFontNormal);
                SampleSplat(distantUV_side, dBlendMask, triplanarWeights.x, dMask, dSideDiffuse, dSideNormal);
            #endif
        #else
            SampleSplat(distantUV, dBlendMask, 1.0f, dMask, distantDiffuse, distantNormal);
        #endif
                    
        #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN)
            distantNormal = WorldTangent(worldTangent, worldBitangent, distantNormal);
        #endif

        #if defined(TRIPLANAR) && !defined(_TERRAIN_BASEMAP_GEN)
            distantDiffuse = distantDiffuse + dFrontDiffuse + dSideDiffuse;
            distantNormal = TriplanarNormal(distantNormal, worldTangent, worldBitangent, dFontNormal, dSideNormal, triplanarWeights, flipUV);
        #endif

        distantDiffuse = lerp(mixedDiffuse, distantDiffuse, _HT_cover);
        distantNormal = lerp(mixedNormal, distantNormal, _HT_cover);
        #ifdef _TERRAIN_BASEMAP_GEN            
            mixedDiffuse = distantDiffuse;
        #else
            mixedDiffuse = lerp(mixedDiffuse, distantDiffuse, dist); 
            mixedNormal = lerp(mixedNormal, distantNormal, dist);
        #endif        
    #endif
            
    #if !defined(TRIPLANAR_TINT) 
        mixedDiffuse.rgb = lerp(mixedDiffuse.rgb, (mixedDiffuse.rgb * tint), _TerrainColorTintStrenght).rgb;
    #endif

    float normalDist = smoothstep(_TerrainNormalTintDistance.x, _TerrainNormalTintDistance.y, (distance(worldPos, _WorldSpaceCameraPos)));
    #ifdef INTERRA_OBJECT
        normalTint = WorldTangent(worldTangent, worldBitangent, normalTint);
    #endif   
    mixedNormal = lerp(mixedNormal, BlendNormals(mixedNormal, normalTint), _TerrainNormalTintStrenght * normalDist).rgb; 

    //========================================================================================
    //---------------------------------------  TRACKS   --------------------------------------
    //========================================================================================
    #if defined(_TRACKS) && !defined(_TERRAIN_BASEMAP_GEN)
    if (_Tracks)
    {
        float halfTrackArea = _InTerra_TrackArea * 0.5f;
        float _InTerra_TrackFading = 25;
        float2 trackUV = float2((worldPos.x - _InTerra_TrackPosition.x) + (halfTrackArea), -(worldPos.z - _InTerra_TrackPosition.z - (halfTrackArea))) * 1.0f / _InTerra_TrackArea;
        float trackDist = smoothstep(_InTerra_TrackArea - 1.0f, _InTerra_TrackArea - _InTerra_TrackFading, (distance(worldPos, float3(_WorldSpaceCameraPos.x, _WorldSpaceCameraPos.y, _WorldSpaceCameraPos.z))));

        float2 minDist = step(float2(0.0f, 0.0f), trackUV); 
        float2 maxDist = step(float2(0.0f, 0.0f), 1.0 - trackUV);
        trackDist *= (minDist.x * minDist.y * maxDist.x * maxDist.y);

        UnpackTrackSplatValues(trackSplats);
        UnpackTrackSplatColor(trackSplatsColor);

        float4 trackColor = TrackSplatValues(blendMask, trackSplatsColor);
        float4 trackValues = TrackSplatValues(blendMask, trackSplats);


        #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN)           
            float2 terrainSize = _TerrainSize.xz * 10;
            float2 tUV = terrainUV.xy;
        #else
            float2 terrainSize = _TerrainSizeXZPosY.xy;
            float2 tUV = uvMainAndLM.xy;
        #endif

        float2 trackDetailUV = float2(tUV.x, -tUV.y) * _TrackDetailTexture_ST.xy + _TrackDetailTexture_ST.zw;

         #ifdef _TERRAIN_PARALLAX
            float2 trackParallaxOffset = ParallaxOffset(_InTerra_TrackTexture, SamplerState_Linear_Repeat, _ParallaxTrackSteps,  -trackValues.y, trackUV, float3( -viewDirTerrain.x, viewDirTerrain.y, -viewDirTerrain.z), _ParallaxTrackAffineSteps, _MipMapLevel + (lod * (log2(max(_InTerra_TrackTexture_TexelSize.z, _InTerra_TrackTexture_TexelSize.w)) + 1)), 1 );

              trackUV += trackParallaxOffset;
              trackDetailUV += (trackParallaxOffset ) * (_TrackDetailTexture_ST.xy );
        #endif
        float4 trackDetail = SAMPLE_TEXTURE2D(_TrackDetailTexture, SamplerState_Linear_Repeat, trackDetailUV);
        trackDepth = SAMPLE_TEXTURE2D_LOD(_InTerra_TrackTexture, SamplerState_Linear_Repeat, trackUV, 0);
 
        float normalsOffset = _InTerra_TrackTexture_TexelSize.x * 2.0f;
        float texelArea = _InTerra_TrackTexture_TexelSize.x * 100 * _InTerra_TrackArea;
        float normalStrenghts = _TrackNormalStrenght / texelArea;
        float normalEdgeStrenghts = _TrackEdgeNormals / texelArea;

        float4 heights[4]; 
        heights[0] = (SAMPLE_TEXTURE2D_LOD(_InTerra_TrackTexture, SamplerState_Linear_Repeat, trackUV + float2(0.0f, normalsOffset), 0.0f));
        heights[1] = (SAMPLE_TEXTURE2D_LOD(_InTerra_TrackTexture, SamplerState_Linear_Repeat, trackUV + float2(normalsOffset, 0.0f), 0.0f));
        heights[2] = (SAMPLE_TEXTURE2D_LOD(_InTerra_TrackTexture, SamplerState_Linear_Repeat, trackUV + float2(-normalsOffset, 0.0f), 0.0f));
        heights[3] = (SAMPLE_TEXTURE2D_LOD(_InTerra_TrackTexture, SamplerState_Linear_Repeat, trackUV + float2(0.0f, -normalsOffset), 0.0f));

        for (int i = 0; i < 4; ++i)
        {
            heights[i] *= float4(1.0f, 1.0f, normalStrenghts, normalEdgeStrenghts);
        }

        float3 edgeNormals = float3(float2(heights[1].a - heights[2].a, heights[3].a - heights[0].a), 1.0f);
        float3 trackNormal = float3(float2(heights[1].b - heights[2].b, heights[3].b - heights[0].b), 1.0f);    

        float heightSum =HeightSum(mask, blendMask);
        float trackHeightMap = saturate(trackDepth.b + _TrackHeightOffset);
        float2 trackIntersect = float2(trackDepth.b, 1 - trackDepth.b);
     
        trackIntersect *= (1 / (pow(2, float2(trackHeightMap, heightSum) * (-(_TrackHeightTransition)))) + 1) * 0.5;
        trackIntersect /= (trackIntersect.r + trackIntersect.g);
       
        float track = trackIntersect.r * trackDist;
        float colorOpacity = saturate(track * trackColor.a);

        float3 trackDetailNormal = UnpackNormalScale(SAMPLE_TEXTURE2D(_TrackDetailNormalTexture, SamplerState_Linear_Repeat, trackDetailUV), _TrackDetailNormalStrenght);
        trackDetailNormal.y *= -1;
        trackDetailNormal.z += 1e-5f;

        trackNormal = lerp(trackNormal, normalize(lerp(trackDetailNormal.xyz, trackNormal, 0.5f)), trackValues.a) * trackIntersect.r;
        float trackEdge = saturate(pow(abs(trackDepth.a), _TrackEdgeSharpness));
        float normalOpacity = saturate(trackValues.z * (trackEdge + track)) * trackDist;

        trackNormal = normalize(lerp(edgeNormals, trackNormal, trackDepth.b));
        trackNormal.z += 1e-5f;
        trackColor = lerp(trackColor, (trackColor * trackDetail), trackValues.a);

         #ifdef INTERRA_OBJECT
            trackNormal = WorldTangent(worldTangent, worldBitangent, trackNormal);
        #endif

        mixedNormal = lerp(mixedNormal, trackNormal, normalOpacity);
        mixedDiffuse.rgb = lerp(mixedDiffuse.rgb, trackColor.rgb, colorOpacity);
        mixedDiffuse.a = lerp(mixedDiffuse.a, trackValues.x, track);
        occlusion = lerp(occlusion, _TrackAO, track); 
    }
    #endif
 
    mixedDiffuse.a = lerp(mixedDiffuse.a, 1.0f, _InTerra_GlobalWetness);
    mixedNormal.xy = _InTerra_GlobalWetness > 0.4 ? mixedNormal.xy * (1 - min(0.8f, (_InTerra_GlobalWetness * 2.25f - 1))) : mixedNormal.xy;
   
    #if defined(INTERRA_MESH_TERRAIN)
        albedo = mixedDiffuse.rgb; 
    #endif
        
    //=======================================================================================
    //==============================|   OBJECT INTEGRATION   |===============================
    //=======================================================================================
    #if defined(INTERRA_OBJECT)        
        
        objectAlbedo.a = _HasMask == 1 ? objectMask.a : _Smoothness;
        objectAlbedo.a = _GlobalWetnessDisabled ? objectAlbedo.a : lerp(objectAlbedo.a, 1.0f, _InTerra_GlobalWetness);
        half objectMetallic = _HasMask == 1 ? objectMask.r : _Metallic;
        half objectAo = _HasMask == 1 ? objectMask.g : _Ao;          
        
        UNITY_BRANCH if (_Detail > 0)
        {
            UNITY_BRANCH if (_HasDetailAlbedo > 0)
            {
                half3 dt = SAMPLE_TEXTURE2D(_DetailAlbedoMap, SamplerState_Linear_Repeat, _DetailUV).rgb;
                objectAlbedo.rgb = lerp(objectAlbedo.rgb, half(2.0) * dt, _DetailStrenght).rgb;
            }
            half3 objectNormalD = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormalMap, SamplerState_Linear_Repeat, _DetailUV), _DetailNormalMapScale);
            objectNormal = (lerp(objectNormal, BlendNormalRNM(objectNormal, objectNormalD), _DetailStrenght));
        }

        mixedDiffuse = lerp(mixedDiffuse, objectAlbedo, heightIntersect.r);

        float3 terrainNormal = (mixedNormal.z * terrainNormals.xyz);
        terrainNormal.xy = mixedNormal.xy + terrainNormal.xy;

        mixedNormal = lerp(mixedNormal, terrainNormal, intersectNormal);
        mixedNormal = lerp(mixedNormal, objectNormal, heightIntersect.r);

        metallic = lerp(metallic, objectMetallic, heightIntersect.r);
        occlusion = lerp(occlusion, objectAo, heightIntersect.r);

        emission = 0;

        UNITY_BRANCH if (_EmissionEnabled > 0)
        {
            emission = lerp(0, objectEmission, heightIntersect.r);
        }
        albedo = mixedDiffuse.rgb;
    #endif

    //===============|   PUDDLES & RAINDROPS  FINAL MIX |======================
    #if !defined(_TERRAIN_BASEMAP_GEN) && defined(_PUDDLES) 
        if (_InTerra_GlobalPuddles.y > 0.0f)
        {
            float wettnessAround = smoothstep(_InTerra_GlobalPuddles.y, _InTerra_GlobalPuddles.x, heightSum) * horizontalWeight;
            mixedNormal.xy = wettnessAround > 0.4 ? mixedNormal.xy * (1 - min(0.9f, (wettnessAround * 2.25f - 1))) : mixedNormal.xy;
            mixedDiffuse.a = lerp(mixedDiffuse.a, 1.0f, wettnessAround);
            mixedDiffuse.a = lerp(mixedDiffuse.a, 1.0f, puddlesHeight.r * horizontalWeight);
            mixedNormal = lerp(mixedNormal, ripNormal, puddlesHeight.r * horizontalWeight);  
        }        
    #endif 

    smoothness = mixedDiffuse.a;   

   
}
