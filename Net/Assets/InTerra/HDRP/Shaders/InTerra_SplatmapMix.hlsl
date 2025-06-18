#ifdef INTERRA_OBJECT
    void ObjectIntegration_float(float heightOffset, float3 tangentViewDirTerrain, float3 worldViewDir, float3 worldNormal, float3 worldTangent, float3 worldBitangent, float2 detailUV, float3 worldPos, float4 terrainNormals, float4 objectAlbedo, float3 objectNormal, float4 objectMask, float3 objectEmission, out float3 albedo, out float3 mixedNormal, out float smoothness, out float metallic, out float occlusion, out float3 emission)
#else
    #ifdef INTERRA_MESH_TERRAIN
        #ifndef TESSELLATION_ON
            void SplatmapMix_float(float3 tangentViewDirTerrain,  float3 worldNormal, float3 worldPos,float3 worldTangent, float3 worldBitangent, out half3 mixedAlbedo, out half3 mixedNormal, out half smoothness, out half metallic, out half occlusion)
        #else
            void SplatmapMix_float(float3 worldNormal, float3 worldPos, float3 worldTangent, float3 worldBitangent, out half3 mixedAlbedo, out half3 mixedNormal, out half smoothness, out half metallic, out half occlusion)
        #endif
    #else
        #ifndef TESSELLATION_ON
            #include "InTerra_Functions.hlsl"
        #endif
        void SplatmapMix(float2 splatBaseUV, float3 worldNormal, float3 tangentViewDirTerrain, float3 worldPos, out float3 mixedAlbedo, out float smoothness, out float metallic, out float occlusion, inout float3 mixedNormal)
    #endif
#endif
{ 
    float4 mixedDiffuse;
    mixedNormal = float3(0, 0, 1);
    #include "InTerra_SplatMapControl.hlsl"
    
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
            blendMask[0] *= 0.0f;
            blendMask[1] *= 0.0f;
            #if defined(_TERRAIN_DISTANCEBLEND)
                dBlendMask[0] *= 0.0f;
                dBlendMask[1] *= 0.0f;
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
        if (_HeightmapBlending == 1)
        {
            HeightBlend(mask, blendMask, _HeightTransition);
            #ifdef _TERRAIN_DISTANCEBLEND
                HeightBlend(dMask, dBlendMask, _Distance_HeightTransition);
            #endif
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
            blendMask[0] *= 0.0f;
            blendMask[1] *= 0.0f;
            #if defined(_TERRAIN_DISTANCEBLEND)
                dBlendMask[0] *= 0.0f;
                dBlendMask[1] *= 0.0f;
            #endif 
        }
    #endif

    MaskSplatWeight(mask, blendMask, mixedMask);
    #ifdef _TERRAIN_DISTANCEBLEND       
        MaskSplatWeight(dMask, dBlendMask, dMixedMask);
        dMixedMask = lerp(mixedMask, dMixedMask, _HT_cover);
        mixedMask = lerp(mixedMask, dMixedMask, dist);
    #endif

    occlusion = mixedMask.g;
    metallic = mixedMask.r;
    heightSum = mixedMask.b;

    #if defined(_TERRAIN_BLEND_HEIGHT) && !defined(_LAYERS_ONE) && !defined(TERRAIN_SPLAT_ADDPASS)
        if (_HeightmapBlending == 1)
        {   
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
    #ifndef _TERRAIN_BASEMAP_GEN
        float3 ripNormal = float3(0, 0, 1);
        float raindropSize = 1.0f / _InTerra_GlobalRaindropRipples.z * 0.1f;
        float2 puddlesHeight = float2(0.0f, 1.0f);

        float3 puddleWeight = pow(abs(worldNormal.rgb), 10.0f);
        puddleWeight = puddleWeight / (puddleWeight.x + puddleWeight.y + puddleWeight.z);

        puddlesHeight = (1 / (1 * pow(2, float2(_InTerra_GlobalPuddles.x, heightSum) * (-(100)))) + 1) * 0.5;
        puddlesHeight /= (puddlesHeight.r + puddlesHeight.g);
        float horizontalWeight = smoothstep(0.9999f, 1.0f, saturate(puddleWeight.y - ((puddleWeight.x + puddleWeight.z) * 100000000)));
        puddlesHeight.r *= horizontalWeight;
        
        if (_InTerra_GlobalRaindropRipples.x > 0.0f)
        { 
            if (puddlesHeight.x > 0.5f)
            {
                float rainIndex =  1 / (min(5.0f, _InTerra_GlobalRaindropRipples.x) * 4.0f);

                for (float i = 5.0; i > 4.0; i -= rainIndex)
                {
                    ripNormal = BlendNormal(ripNormal, RainRipples(worldPos.zx * raindropSize * i + i * 0.25f ,  i, i * 0.25f));
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
            SampleSplat(uvSplat, blendMask, saturate(triplanarWeights.y + (1 - blendMask[0].r)), mask, mixedDiffuse, mixedNormal);
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
                SampleSplat(distantUV, dBlendMask, saturate(triplanarWeights.y + (1 - dBlendMask[0].r)), dMask, distantDiffuse, distantNormal);
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
            
    float3 tint = SAMPLE_TEXTURE2D(_TerrainColorTintTexture, SamplerState_Linear_Repeat, tintUV).rgb;
    #if !defined(TRIPLANAR_TINT) 
        mixedDiffuse.rgb = lerp(mixedDiffuse.rgb, (mixedDiffuse.rgb * tint), _TerrainColorTintStrenght).rgb;
    #endif

    float normalDist = smoothstep(_TerrainNormalTintDistance.x, _TerrainNormalTintDistance.y, (distance(worldPos, _WorldSpaceCameraPos)));
    float3 normalTint = UnpackNormals(SAMPLE_TEXTURE2D(_TerrainNormalTintTexture, sampler_Splat0, normalTintUV), 1);
    #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN) 
        normalTint = WorldTangent(worldTangent, worldBitangent, normalTint);
    #endif   
    mixedNormal = lerp(mixedNormal, BlendNormals(mixedNormal, normalTint), _TerrainNormalTintStrenght * normalDist).rgb;


    //========================================================================================
    //---------------------------------------  TRACKS   --------------------------------------
    //========================================================================================
    #if defined(_TRACKS) && !defined(_TERRAIN_BASEMAP_GEN)
        if (_Tracks == 1)
        {
        UnpackTrackSplatValues(trackSplats);
        UnpackTrackSplatColor(trackSplatsColor);

        float4 trackColor = TrackSplatValues(tBlendMask, trackSplatsColor);
        float4 trackValues = TrackSplatValues(tBlendMask, trackSplats);

        #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN) 
            float2 terrainSize = _TerrainSize.xz;
        #else
            float2 terrainSize = _TerrainSizeXZPosY.xy;
        #endif
        
        float2 trackDetailUV = (float2(splatBaseUV.x, -splatBaseUV.y) * _TrackDetailTexture_ST.xy  * terrainSize + _TrackDetailTexture_ST.zw);

        #if defined(PARALLAX) && defined(_TERRAIN_PARALLAX)
        if (_Terrain_Parallax == 1)
        {
            float2 trackParallaxOffset = ParallaxOffset(_InTerra_TrackTexture, SamplerState_Linear_Repeat, _ParallaxTrackSteps,  -trackValues.y, trackUV, float3( -tangentViewDirTerrain.x, tangentViewDirTerrain.y, -tangentViewDirTerrain.z), _ParallaxTrackAffineSteps, _MipMapLevel + (lod * (log2(max(_InTerra_TrackTexture_TexelSize.z, _InTerra_TrackTexture_TexelSize.w)) + 1)), 1 );

              trackUV += trackParallaxOffset;
              trackDetailUV += (trackParallaxOffset ) * (_TrackDetailTexture_ST.xy * _InTerra_TrackArea);
        }
        #endif
        float4 trackDetail = SAMPLE_TEXTURE2D(_TrackDetailTexture, SamplerState_Linear_Repeat, trackDetailUV);
        trackDepth = SAMPLE_TEXTURE2D_LOD(_InTerra_TrackTexture, SamplerState_Linear_Repeat, trackUV, 0);
 
        float normalsOffset = _InTerra_TrackTexture_TexelSize.x;
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
            heights[i] *= float4(1.0f, 1.0f, normalStrenghts * 2, normalEdgeStrenghts * 2);
        }

        float3 edgeNormals = (float3(float2(heights[2].a - heights[1].a, heights[0].a - heights[3].a), 1.0f));
        float3 trackNormal =  float3(float2(heights[2].b - heights[1].b, heights[0].b - heights[3].b), 1.0f);        

        float4 trackDetailNormal = SAMPLE_TEXTURE2D(_TrackDetailNormalTexture, SamplerState_Linear_Repeat, trackDetailUV);
        trackDetailNormal.xyz = UnpackNormalScale(float4(trackDetailNormal.x, trackDetailNormal.y, 0, 1-trackDetailNormal.w), _TrackDetailNormalStrenght);
        trackDetailNormal.z += 1e-5f;

        float heightSum = HeightSum(mask, blendMask);
        float trackHeightMap = saturate(trackDepth.b + _TrackHeightOffset);
        float2 trackIntersect = float2(trackDepth.b, 1 - trackDepth.b);

        trackIntersect *= (1 / (pow(2, float2(trackHeightMap, heightSum) * (-(_TrackHeightTransition)))) + 1) * 0.5;
        trackIntersect /= (trackIntersect.r + trackIntersect.g);

        trackNormal = (lerp(trackNormal, normalize(lerp(trackDetailNormal.xyz, trackNormal, 0.5f)), trackValues.a)) * trackIntersect.r;
        float trackEdge = saturate(pow(abs(trackDepth.a), _TrackEdgeSharpness));
  
        float track = trackIntersect.r * trackDist;
        float colorOpacity = saturate(track * trackColor.a);
        float normalOpacity = saturate(trackValues.z * (trackEdge + track)) * trackDist;

        #if defined(_NORMALMAPS)
            trackNormal = normalize(lerp(edgeNormals, trackNormal, trackDepth.b));
            trackNormal.z += 1e-5f;
            trackColor = lerp(trackColor, (trackColor * trackDetail), trackValues.a);

            #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN) 
                trackNormal.xy *= -1;
                edgeNormals.xy *= -1;
                trackNormal = WorldTangent(worldTangent, worldBitangent, trackNormal);
            #endif
        
            mixedNormal = lerp(mixedNormal, trackNormal, normalOpacity);
            mixedNormal.z += +1e-5f;
        #endif
        mixedDiffuse.rgb = lerp(mixedDiffuse.rgb, trackColor.rgb, colorOpacity);
        mixedDiffuse.a = lerp(mixedDiffuse.a, trackValues.x, track);
        occlusion = lerp(occlusion, _TrackAO, track);
        }
    #endif

    mixedDiffuse.a = lerp(mixedDiffuse.a, 1.0f, _InTerra_GlobalWetness);
    mixedNormal.xy = _InTerra_GlobalWetness > 0.4 ? mixedNormal.xy * (1 - min(0.7f, (_InTerra_GlobalWetness * 2.25f - 1))) : mixedNormal.xy;
    mixedNormal.z += +1e-5f;

    //=======================================================================================
    //==============================|   OBJECT INTEGRATION   |===============================
    //=======================================================================================
    #ifdef INTERRA_OBJECT	
        objectAlbedo.a = _HasMask == 1 ? objectMask.a : _Smoothness;
        objectAlbedo.a = _GlobalWetnessDisabled ? objectAlbedo.a : lerp(objectAlbedo.a, 1.0f, _InTerra_GlobalWetness);
        float objectMetallic = _HasMask == 1 ? objectMask.r : _Metallic;
        float objectAo = _HasMask == 1 ? objectMask.g : _Ao;

        float3 dt = float3(0, 0, 0);
        UNITY_BRANCH if (_Detail > 0)
        {
            UNITY_BRANCH if (_HasDetailAlbedo > 0)
            {
                float3 dt = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, detailUV).rgb;
                objectAlbedo.rgb = lerp(objectAlbedo.rgb, half(2.0) * dt, _DetailStrenght).rgb;
            }

            float3 objectNormalD = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUV), _DetailNormalMapScale);
            objectNormal = (lerp(objectNormal, BlendNormalRNM(objectNormal, objectNormalD), _DetailStrenght));
        }

        mixedDiffuse = lerp(mixedDiffuse, objectAlbedo, heightIntersect.r);

        float3 terrainNormal = (mixedNormal.z * terrainNormals.xyz) + 1e-5f;
        terrainNormal.xy = mixedNormal.xy + terrainNormal.xy;
        mixedNormal = lerp(mixedNormal, terrainNormal, intersectNormal);
        mixedNormal = lerp(mixedNormal, objectNormal, heightIntersect.r);

        metallic = lerp(metallic, objectMetallic, heightIntersect.r);
        occlusion = lerp(occlusion, objectAo, heightIntersect.r);
        albedo = mixedDiffuse.rgb;
        emission = 0;

        UNITY_BRANCH if (_EmissionEnabled > 0)
        {
            emission = lerp(0, objectEmission, heightIntersect.r);
        }
    #else
        mixedAlbedo = mixedDiffuse.rgb;
    #endif
       
    //===============|   PUDDLES & RAINDROPS  FINAL MIX |======================
    #ifndef _TERRAIN_BASEMAP_GEN 
        if (_InTerra_GlobalPuddles.y > 0.0f)
        {
            float wettnessAround = smoothstep(_InTerra_GlobalPuddles.y, _InTerra_GlobalPuddles.x, heightSum) * horizontalWeight;
            mixedNormal.xy = wettnessAround > 0.4 ? mixedNormal.xy * (1 - min(0.9f, (wettnessAround * 2.25f - 1))) : mixedNormal.xy;
            mixedDiffuse.a = lerp(mixedDiffuse.a, 1.0f, wettnessAround);
            mixedDiffuse.a = lerp(mixedDiffuse.a, 1.0f, puddlesHeight.r * horizontalWeight);
            mixedNormal = lerp(mixedNormal, ripNormal, puddlesHeight.r * horizontalWeight); 
            occlusion = lerp(occlusion, lerp(occlusion, 0.9f, 0.75f) , puddlesHeight.r * horizontalWeight);
        }        
    #endif 
    smoothness = mixedDiffuse.a; 
    mixedNormal.z += +1e-5f;
   
    //=========================================================================================             
}
