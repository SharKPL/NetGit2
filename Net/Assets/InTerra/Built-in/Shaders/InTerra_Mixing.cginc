//====================================================================
//===========================| VERTEX |===============================
//====================================================================
#ifndef TERRAIN_BASEGEN
void SplatmapVert(inout appdata_full v, out Input data)
{
    UNITY_INITIALIZE_OUTPUT(Input, data);
    #if !defined(INTERRA_TERRAIN)
        data.worldNormal = UnityObjectToWorldNormal(v.normal);
        data.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    #endif
    
    //=================== TERRAIN INSTANCING ====================
    #if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X) && !defined(INTERRA_OBJECT) && !defined(INTERRA_MESH_TERRAIN)
        float2 patchVertex = v.vertex.xy;
        float4 instanceData = UNITY_ACCESS_INSTANCED_PROP(Terrain, _TerrainPatchInstanceData);

        float4 uvscale = instanceData.z * _TerrainHeightmapRecipSize;
        float4 uvoffset = instanceData.xyxy * uvscale;
        uvoffset.xy += 0.5f * _TerrainHeightmapRecipSize.xy;
        float2 sampleCoords = (patchVertex.xy * uvscale.xy + uvoffset.xy);

        float hm = UnpackHeightmap(tex2Dlod(_TerrainHeightmapTexture, float4(sampleCoords, 0, 0)));
        v.vertex.xz = (patchVertex.xy + instanceData.xy) * _TerrainHeightmapScale.xz * instanceData.z; 
        v.vertex.y = hm * _TerrainHeightmapScale.y;
        v.vertex.w = 1.0f;

        v.texcoord.xy = (patchVertex.xy * uvscale.zw + uvoffset.zw);
        v.texcoord3 = v.texcoord2 = v.texcoord1 = v.texcoord;  

        #ifdef TERRAIN_INSTANCED_PERPIXEL_NORMAL
            v.normal = float3(0, 1, 0); 
            data.tc.zw = sampleCoords;
        #else
            float3 nor = tex2Dlod(_TerrainNormalmapTexture, float4(sampleCoords, 0, 0)).xyz;
            v.normal = 2.0f * nor - 1.0f;
        #endif
 
        float3 nor = tex2Dlod(_TerrainNormalmapTexture, float4(sampleCoords, 0, 0)).xyz;
        data.terrainNormals = UnityObjectToWorldNormal(2.0f * nor - 1.0f);      
    #endif
    //==============================================================

    float3 wDir = WorldSpaceViewDir(v.vertex);

    #if defined(INTERRA_TERRAIN)
        data.tc.xy = v.texcoord.xy;

        #ifdef TERRAIN_BASE_PASS
            #ifdef UNITY_PASS_META
                data.tc.xy = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
            #endif
        #else
            float4 pos = UnityObjectToClipPos(v.vertex);
            UNITY_TRANSFER_FOG(data, pos);
        #endif
        
        v.tangent.xyz = cross(v.normal, float3(0, 0, 1));
        v.tangent.w = -1;

        #ifdef PARALLAX
            data.tangentViewDir = TangentViewDir(v.normal, v.tangent, wDir);
        #endif
    #else
        #if defined(_OBJECT_PARALLAX) || (defined(INTERRA_MESH_TERRAIN) && defined(_TERRAIN_PARALLAX)) && !defined(DIFFUSE)
            data.tangentViewDirObject = TangentViewDir(v.normal, v.tangent, ObjSpaceViewDir(v.vertex));
        #endif 

        float2 hmUV = float2 ((data.worldPos.x - _TerrainPosition.x) * (1 / _TerrainSize.x), (data.worldPos.z - _TerrainPosition.z) * (1 / _TerrainSize.z));

        float4 ts = float4(_TerrainHeightmapTexture_TexelSize.x, _TerrainHeightmapTexture_TexelSize.y, 0, 0);
        float4 hsX = _TerrainHeightmapScale.w / _TerrainHeightmapScale.x;
        float4 hsZ = _TerrainHeightmapScale.w / _TerrainHeightmapScale.z;

        float4 heightToNormal;
        float3 terrainNormal;

        heightToNormal[0] = UnpackHeightmap(tex2Dlod(_TerrainHeightmapTexture, float4(hmUV + float2(ts * float2(0, -1)), 0, 0))).r * hsZ;
        heightToNormal[1] = UnpackHeightmap(tex2Dlod(_TerrainHeightmapTexture, float4(hmUV + float2(ts * float2(-1, 0)), 0, 0))).r * hsX;
        heightToNormal[2] = UnpackHeightmap(tex2Dlod(_TerrainHeightmapTexture, float4(hmUV + float2(ts * float2(1, 0)), 0, 0))).r * hsX;
        heightToNormal[3] = UnpackHeightmap(tex2Dlod(_TerrainHeightmapTexture, float4(hmUV + float2(ts * float2(0, 1)), 0, 0))).r * hsZ;

        terrainNormal.x = heightToNormal[1] - heightToNormal[2];
        terrainNormal.z = heightToNormal[0] - heightToNormal[3];
        terrainNormal.y = 1;

        float3 height = UnpackHeightmap(tex2Dlod(_TerrainHeightmapTexture, float4(hmUV, 0, 0)));

        float heightOffset = data.worldPos.y - _TerrainPosition.y + (height * -_TerrainHeightmapScale.y);

        float3 tWeights = pow(abs(terrainNormal), _TriplanarSharpness);
        tWeights = tWeights / (tWeights.x + tWeights.y + tWeights.z);

        data.mainTC_tWeightY_hOffset = float4(v.texcoord.xy, tWeights.y, heightOffset);

        #if defined(PARALLAX) && defined(_TERRAIN_PARALLAX)
        if (_Terrain_Parallax)
        {
            float intersection = smoothstep(_NormIntersect.y, _NormIntersect.x, heightOffset);
            float3 mixedNormal = lerp(data.worldNormal, normalize(terrainNormal), intersection);
            half3 axisSign = sign(mixedNormal);
            half3 tangentY = normalize(cross(mixedNormal, half3(0, 0, axisSign.y)));
            half3 bitangentY = normalize(cross(tangentY, mixedNormal)) * axisSign.y;
            half3x3 tbnY = half3x3(tangentY, bitangentY, mixedNormal);
            data.tangentViewDir = mul(tbnY, wDir);
        }
        #endif

        float3 wTangent = UnityObjectToWorldDir(v.tangent);
        float3 wBTangent = normalize(cross(data.worldNormal, wTangent)) * v.tangent.w * unity_WorldTransformParams.w;
        float3x3 tangentTransform_World = float3x3(wTangent, wBTangent, data.worldNormal);
        data.terrainNormals = mul(tangentTransform_World, terrainNormal);
    #endif
}
#endif


//==============================================================================
//==========================|   FRAGMENT MIXING    |============================
//==============================================================================
#ifdef DIFFUSE
    void SplatmapMix(Input IN, out half weight, out fixed4 mixedDiffuse, out fixed3 mixedNormal)
#else
    #ifdef INTERRA_OBJECT
        void SplatmapMix(Input IN, out half weight, out fixed4 mixedDiffuse, inout fixed3 mixedNormal, out float ao, out half metallic, out half3 emission)
    #else
        void SplatmapMix(Input IN, out half weight, out fixed4 mixedDiffuse, inout fixed3 mixedNormal, out float ao, out half metallic)
    #endif
#endif
{
    //---------------- VARIABLES -----------------
    half4 blendMask[2];
    blendMask[0] = half4(1, 0, 0, 0);
    blendMask[1] = half4(0, 0, 0, 0);
    
    mixedDiffuse = 1;
    mixedNormal = float3( 0 ,0, 1);

    half4 mixedMask = 0;
    half heightSum = 0.5f;
    half dHeightSum = 0.5f;

    #ifndef DIFFUSE  
        ao = 1;
        metallic = 0.0f;
    #endif

    float2 uvSplat[_LAYER_COUNT];
    half4 splat[_LAYER_COUNT], mask[_LAYER_COUNT];

    float2 uvSplat_front[_LAYER_COUNT], uvSplat_side[_LAYER_COUNT];
    half4 splat_front[_LAYER_COUNT], splat_side[_LAYER_COUNT], mask_front[_LAYER_COUNT], mask_side[_LAYER_COUNT];

    #ifdef _TERRAIN_DISTANCEBLEND
        float2 distantUV[_LAYER_COUNT];
        half4 dSplat[_LAYER_COUNT], dMask[_LAYER_COUNT];
        float2 distantUV_front[_LAYER_COUNT], distantUV_side[_LAYER_COUNT];
        #ifdef TRIPLANAR
            half4 dSplat_front[_LAYER_COUNT], dSplat_side[_LAYER_COUNT], dMask_front[_LAYER_COUNT], dMask_side[_LAYER_COUNT];
        #endif
    #endif

    #ifdef _TRACKS
        float4 trackSplats[_LAYER_COUNT];
        float4 trackSplatsColor[_LAYER_COUNT];
        float4 trackDepth = 0;
    #endif

    float3 worldTangent = WorldNormalVector(IN, float3(1, 0, 0));
    float3 worldBitangent = WorldNormalVector(IN, float3(0, 1, 0));
    

    //---------------- CLIP HOLES ----------------
    #if defined(_ALPHATEST_ON) && defined(INTERRA_TERRAIN)
        ClipHoles(IN.tc.xy);
    #endif

    //================================================================
    //----------------------- SPLAT MAP CONTROL ----------------------
    //================================================================
    #ifndef INTERRA_TERRAIN
        float2 terrainUV = (IN.worldPos.xz - _TerrainPosition.xz) * (1 / _TerrainSize.xz);
        #ifndef _LAYERS_ONE     
            float2 splatMapUV = (terrainUV * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
        #endif
        half2 tintUV = terrainUV * _TerrainColorTintTexture_ST.xy + _TerrainColorTintTexture_ST.zw;
        half2 normalTintUV = terrainUV * _TerrainNormalTintTexture_ST.xy + _TerrainNormalTintTexture_ST.zw;
    #else
        float2 splatMapUV = (IN.tc.xy * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
        half2 tintUV = IN.tc.xy * _TerrainColorTintTexture_ST.xy + _TerrainColorTintTexture_ST.zw;
        half2 normalTintUV = IN.tc.xy * _TerrainNormalTintTexture_ST.xy + _TerrainNormalTintTexture_ST.zw;
    #endif

    #if !defined(_LAYERS_ONE)
        blendMask[0] = UNITY_SAMPLE_TEX2D(_Control, splatMapUV);
        #ifdef _LAYERS_EIGHT
            blendMask[1] = UNITY_SAMPLE_TEX2D_SAMPLER(_Control1, _Control, splatMapUV);
        #endif 
    #endif

    #if defined(INTERRA_TERRAIN)
        #if defined(_LAYERS_EIGHT)
            weight = 1;
        #else
            weight = dot(blendMask[0], half4(1, 1, 1, 1));
            #if !defined(SHADER_API_MOBILE) && defined(TERRAIN_SPLAT_ADDPASS)
                clip(weight == 0.0f ? -1 : 1);
            #endif
            blendMask[0] /= (weight + 1e-3f);
        #endif      
    #endif

    #if !defined(INTERRA_TERRAIN) || defined(TRIPLANAR) || defined(_PUDDLES) || defined(PARALLAX)
        #ifdef UNITY_INSTANCING_ENABLED
            float3 wNormal = IN.terrainNormals;
        #else
            float3 wNormal = WorldNormalVector(IN, float3(0, 0, 1));
        #endif
        float3 flipUV = wNormal < 0 ? -1 : 1;
        float3  triplanarWeights = abs(wNormal);
        triplanarWeights = pow(triplanarWeights, _TriplanarSharpness);
        triplanarWeights = triplanarWeights / (triplanarWeights.x + triplanarWeights.y + triplanarWeights.z);

        #if defined(INTERRA_OBJECT)
            TriplanarOneToAllSteep(blendMask, (1 - IN.mainTC_tWeightY_hOffset.z), weight);
        #else
            TriplanarOneToAllSteep(blendMask, (1 - triplanarWeights.y), weight);
        #endif   
    #endif

    #if defined(INTERRA_OBJECT) && defined(_LAYERS_TWO)
        blendMask[0].r = _ControlNumber == 0 ? blendMask[0].r : _ControlNumber == 1 ? blendMask[0].g : _ControlNumber == 2 ? blendMask[0].b : blendMask[0].a;
        blendMask[0].g = 1 - blendMask[0].r;
    #else
        if (_TwoLayersOnly > 0)
        {
            blendMask[0].r = 1 - blendMask[0].g;
            blendMask[0].ba = 0;
            blendMask[1] = 0;
        }
    #endif



    #if defined(_TERRAIN_BLEND_HEIGHT) && !defined(TERRAIN_SPLAT_ADDPASS) && !defined(_LAYERS_EIGHT) 
        blendMask[0] = (blendMask[0].r + blendMask[0].g + blendMask[0].b + blendMask[0].a == 0.0f ? 1.0f : blendMask[0]); //this is preventing the black area when more than one pass
    #endif

    half4 origBlendMask[2] = blendMask;
        
    #if defined(_TERRAIN_DISTANCEBLEND)
        float dist = smoothstep(_HT_distance.x, _HT_distance.y, (distance(IN.worldPos, _WorldSpaceCameraPos)));
        half4 dBlendMask[2];
        half4 dOrigBlendMask[2] = blendMask;
        dBlendMask[0] = blendMask[0];
        dBlendMask[1] = blendMask[1];
        half4 dMixedMask = 0;
        float sampleDistMask = dist < 0.01f ? 1 : 0;
        if(sampleDistMask)
        {
            dBlendMask[0] *= 0.0f;
           #ifdef _LAYERS_EIGHT
               dBlendMask[1] *= 0.0f;
           #endif
        }
    #endif

    #if defined(PARALLAX) && !defined(DIFFUSE)
        int lod = smoothstep(_MipMapFade.x, _MipMapFade.y, (distance(IN.worldPos, _WorldSpaceCameraPos)));
    #endif

    #ifndef INTERRA_TERRAIN       
        float2 mainUV = TRANSFORM_TEX(IN.mainTC_tWeightY_hOffset.xy, _MainTex);
        #if defined(PARALLAX) && defined(_OBJECT_PARALLAX)
        float2 mainParallaxOffset = 0;
        if (_Object_Parallax)
        {                      
            mainParallaxOffset = ParallaxOffset(_MainMask, _ParallaxSteps, _ParallaxHeight, mainUV, normalize(IN.tangentViewDirObject), _ParallaxAffineSteps, _MipMapLevel + (_MipMapLevel + (lod * _MipMapCount)), 0);
            mainUV += mainParallaxOffset;
        }
        #endif

        float4 objectAlbedo = UNITY_SAMPLE_TEX2D(_MainTex, mainUV) * _Color;

        weight = 0;
    #endif	

    //===========================================================
    //-------------------------- UVs ----------------------------
    //===========================================================
    #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN) 
        #if defined(INTERRA_OBJECT)      
            #ifndef _OBJECT_TRIPLANAR            
                _SteepDistortion = wNormal.y > 0.5 ? 0 : (1 - wNormal.y) * _SteepDistortion;
                _SteepDistortion *= objectAlbedo.r;
            #else            
                _SteepDistortion = 0;
            #endif
        #endif

        float3 positionOffset = _WorldMapping ? IN.worldPos : (IN.worldPos - _TerrainPosition);

        #ifndef TRIPLANAR
            UvSplat(uvSplat, positionOffset);
        #else

            float offsetZ = -flipUV.z * IN.worldPos.y;
            float offsetX = -flipUV.x * IN.worldPos.y;
            #if defined(INTERRA_OBJECT) 
                offsetZ = _DisableOffsetY == 1 ? -flipUV.z * IN.worldPos.y : IN.mainTC_tWeightY_hOffset.w * -flipUV.z + (IN.worldPos.z);
                offsetX = _DisableOffsetY == 1 ? -flipUV.x * IN.worldPos.y : IN.mainTC_tWeightY_hOffset.w * -flipUV.x + (IN.worldPos.x);
            #endif
            offsetZ -= _TerrainPosition.z;
            offsetX -= _TerrainPosition.x;

            UvSplat(uvSplat, uvSplat_front, uvSplat_side, positionOffset, offsetZ, offsetX, flipUV);
        #endif
    #else
        float2 uv = _WorldMapping ? (IN.worldPos.xz / _TerrainSizeXZPosY) : IN.tc.xy;
        #ifndef TRIPLANAR
            UvSplat(uvSplat, uv);
        #else
            UvSplat(uvSplat, uvSplat_front, uvSplat_side, IN.worldPos, uv, flipUV);
        #endif
    #endif  

    //---------------- DISTANCE UV -----------------
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

    //--------------- PARALLAX OFFSET -----------------     
    #if defined(PARALLAX) && defined(_TERRAIN_PARALLAX)
    if(_Terrain_Parallax)
    { 
        ParallaxUV(uvSplat, normalize(IN.tangentViewDir), blendMask, lod, triplanarWeights.y);
    }
    #endif

    //======================================================================
    //-------------------------  OBJECT INTERSECTION  ----------------------
    //======================================================================
    #ifdef INTERRA_OBJECT	
        float steepWeights = _SteepIntersection == 1 ? saturate(wNormal.y + _Steepness) : 1;
        float intersect1 = smoothstep(_Intersection.y, _Intersection.x, IN.mainTC_tWeightY_hOffset.w) * steepWeights;
        float intersect2 = smoothstep(_Intersection2.y, _Intersection2.x, IN.mainTC_tWeightY_hOffset.w) * (1 - steepWeights);
        float intersection = intersect1 + intersect2;
              
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

    //====================================================================
    //-----------------------------  MASK MAPS  --------------------------
    //====================================================================      	
    #if defined(TRIPLANAR) && !defined(_TERRAIN_BASEMAP_GEN)      
        #ifdef _TERRAIN_TRIPLANAR_ONE
            #ifndef TERRAIN_SPLAT_ADDPASS
                float tolWeightY = saturate(triplanarWeights.y + (1 - blendMask[0].r));
            #else
                float tolWeightY = 1.0f;
            #endif
            SampleMask(mask, uvSplat, blendMask, tolWeightY);
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
                #ifndef TERRAIN_SPLAT_ADDPASS
                    float dTolWeightY = saturate(triplanarWeights.y + (1 - dBlendMask[0].r));
                #else
                    float dTolWeightY = 1.0f;
                #endif
                SampleMask(dMask, distantUV, dBlendMask, dTolWeightY);
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
   
    //====================================================================
    //------------------------- HEIGHTMAP BLENDINGS ----------------------
    //====================================================================
    #if defined(_TERRAIN_BLEND_HEIGHT) && !defined(_LAYERS_ONE) && !defined(TERRAIN_SPLAT_ADDPASS)
        #if defined( _TERRAIN_BASEMAP_GEN) && !defined(_LAYERS_EIGHT) 
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
        #ifdef DIFFUSE
            half height = objectAlbedo.a * _MaskMapRemapScale.b + _MaskMapRemapOffset.b;
        #else 	
            half4 objectMask = UNITY_SAMPLE_TEX2D_SAMPLER(_MainMask, _MainTex, mainUV);
            objectMask.rgba = objectMask.rgba * _MaskMapRemapScale.rgba + _MaskMapRemapOffset.rgba;
            half height = objectMask.b;
        #endif

        half terrainHeith = 0.5f;
        #if defined(_TERRAIN_BLEND_HEIGHT) && !defined(TERRAIN_SPLAT_ADDPASS)
            terrainHeith = lerp(heightSum, 1, intersection);
        #endif 

        float2 heightIntersect = (1.0f / (pow(2.0f, float2(((1.0f - intersection) * height), (intersection * terrainHeith)) * (-(_Sharpness)))) + 1.0f) * 0.5f;
        heightIntersect /= (heightIntersect.r + heightIntersect.g);

        heightSum = (heightSum * heightIntersect.g) + (height * heightIntersect.r);

        if (heightIntersect.g < 1e-5f)
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
    #if !defined(DIFFUSE)
        ao = mixedMask.g;
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
    #if !defined(DIFFUSE) && defined(_PUDDLES)
        float2 puddlesHeight = float2(0.0f , 1.0f);
        float horizontalWeight = 0.0f;
        float3 ripNormal = float3(0, 0, 1);

        float3 puddleWeight = pow(abs(wNormal), 10.0f);
        puddleWeight = puddleWeight / (puddleWeight.x + puddleWeight.y + puddleWeight.z);

        puddlesHeight = (1 / (1 * pow(2, float2(max(0.0f, _InTerra_GlobalPuddles.x), heightSum) * (-(100.0f)))) + 1) * 0.5;
        puddlesHeight /= (puddlesHeight.r + puddlesHeight.g);
        horizontalWeight = smoothstep(0.9999f, 1.0f, saturate(puddleWeight.y - ( (puddleWeight.x + puddleWeight.z) * 100000000.0f)));
        puddlesHeight.r *= horizontalWeight;
        _InTerra_GlobalRaindropRipples.z = 1.0f / _InTerra_GlobalRaindropRipples.z * 0.1f;

        if (_InTerra_GlobalRaindropRipples.x > 0.0f)
        { 
            if (puddlesHeight.x > 0.8f)
            {
                float rainIndex =  1 / (min(5.0f, _InTerra_GlobalRaindropRipples.x) * 4.0f);

                for (float i = 5.0; i > 4.0; i -= rainIndex)
                {
                    ripNormal = BlendNormal(ripNormal, RainRipples(IN.worldPos.zx * _InTerra_GlobalRaindropRipples.z * i + i * 0.25f , i, i * 0.25f));
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
        
    //=======================================================================
    //--------------------  ALBEDO, SMOOTHNESS & NORMAL ---------------------
    //=======================================================================
    #if defined(TRIPLANAR) && !defined(_TERRAIN_BASEMAP_GEN)
        float4 frontDiffuse;
        float3 frontNormal;
        float4 sideDiffuse;
        float3 sideNormal;

        #ifdef _TERRAIN_TRIPLANAR_ONE  
            SampleSplat(uvSplat, blendMask, tolWeightY, mask, mixedDiffuse, mixedNormal, splat);
            SampleSplatTOL(frontDiffuse, frontNormal, uvSplat_front, blendMask, triplanarWeights.z, mask);
            SampleSplatTOL(sideDiffuse, sideNormal, uvSplat_side, blendMask, triplanarWeights.x, mask);
        #else
            SampleSplat(uvSplat, blendMask, triplanarWeights.y, mask, mixedDiffuse, mixedNormal, splat);
            SampleSplat(uvSplat_front, blendMask, triplanarWeights.z, mask, frontDiffuse, frontNormal, splat);
            SampleSplat(uvSplat_side, blendMask, triplanarWeights.x, mask, sideDiffuse, sideNormal, splat);
        #endif       
    #else
        SampleSplat(uvSplat, blendMask, 1.0f, mask, mixedDiffuse, mixedNormal, splat);
    #endif

    #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN)
        mixedNormal = WorldTangent(worldTangent, worldBitangent, mixedNormal);
    #endif 

    #if defined(TRIPLANAR) && !defined(_TERRAIN_BASEMAP_GEN)
        mixedDiffuse = mixedDiffuse + frontDiffuse + sideDiffuse;
        mixedNormal = TriplanarNormal(mixedNormal, worldTangent, worldBitangent, frontNormal, sideNormal, triplanarWeights, flipUV);
    #endif

    //---------------------- HIDE TILING --------------------
    #ifdef _TERRAIN_DISTANCEBLEND     
        float4 distantDiffuse;   
        float3 distantNormal;
       
        #if defined(TRIPLANAR) && !defined(_TERRAIN_BASEMAP_GEN)
            float4 dFrontDiffuse;
            float3 dFontNormal;
            float4 dSideDiffuse;
            float3 dSideNormal;

            #ifdef _TERRAIN_TRIPLANAR_ONE
                SampleSplat(distantUV, dBlendMask, dTolWeightY, dMask, distantDiffuse, distantNormal, dSplat);
                SampleSplatTOL(dFrontDiffuse, dFontNormal, distantUV_front, dBlendMask, triplanarWeights.z, dMask);
                SampleSplatTOL(dSideDiffuse, dSideNormal, distantUV_side, dBlendMask, triplanarWeights.x, dMask);
            #else
                SampleSplat(distantUV, dBlendMask, triplanarWeights.y, dMask, distantDiffuse, distantNormal, dSplat);
                SampleSplat(distantUV_front, dBlendMask, triplanarWeights.z, dMask, dFrontDiffuse, dFontNormal, dSplat);
                SampleSplat(distantUV_side, dBlendMask, triplanarWeights.x, dMask, dSideDiffuse, dSideNormal, dSplat);
            #endif
        #else
            SampleSplat(distantUV, dBlendMask, 1.0f, dMask, distantDiffuse, distantNormal, dSplat);
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

           
    //===================================================================
    //------------------ TINT & ADDITIONAL NORMALS ----------------------
    //===================================================================
   #if defined(TERRAIN_BASE_PASS)
        half4 tint = tex2D(_TerrainColorTintTexture, tintUV);
    #else
        half4 tint = UNITY_SAMPLE_TEX2D_SAMPLER(_TerrainColorTintTexture, _Splat0, tintUV);
        #ifdef _NORMALMAPS
            float3 normalTint = UnpackNormalWithScale(UNITY_SAMPLE_TEX2D_SAMPLER(_TerrainNormalTintTexture, _Splat0, normalTintUV), 1.0f);
            #ifndef INTERRA_TERRAIN
                normalTint = WorldTangent(worldTangent, worldBitangent, normalTint);
            #endif
            float normalDist = smoothstep(_TerrainNormalTintDistance.x, _TerrainNormalTintDistance.y, (distance(IN.worldPos, _WorldSpaceCameraPos)));
            #if !defined(TERRAIN_BASE_PASS)
                mixedNormal = lerp(mixedNormal, BlendNormals(mixedNormal, normalTint), _TerrainNormalTintStrenght * normalDist).rgb;     
            #endif
        #endif  
    #endif

    //===================================================================
    //----------------------------   TRACKS   ---------------------------
    //===================================================================
    #if defined(_TRACKS) && !defined(_TERRAIN_BASEMAP_GEN) 
    if(_Tracks)
    { 
        float halfTrackArea = _InTerra_TrackArea * 0.5f;
        float _InTerra_TrackFading = 30;
        float2 trackUV = float2((IN.worldPos.x - _InTerra_TrackPosition.x) + (halfTrackArea), -(IN.worldPos.z - _InTerra_TrackPosition.z - (halfTrackArea))) * 1.0f / _InTerra_TrackArea;

        float trackDist = smoothstep(_InTerra_TrackArea - 1.0f, _InTerra_TrackArea - _InTerra_TrackFading, (distance(IN.worldPos, float3(_WorldSpaceCameraPos.x, _WorldSpaceCameraPos.y, _WorldSpaceCameraPos.z))));

        float2 minDist = step(float2(0.0f, 0.0f), trackUV);
        float2 maxDist = step(float2(0.0f, 0.0f), 1.0 - trackUV);
        trackDist *= (minDist.x * minDist.y * maxDist.x * maxDist.y);

        if(trackDist > 0.0001f)
        { 
            UnpackTrackSplatValues(trackSplats);
            UnpackTrackSplatColor(trackSplatsColor);

            float4 trackColor = TrackSplatValues(blendMask, trackSplatsColor) ;
            float4 trackValues = TrackSplatValues(blendMask, trackSplats);

            #ifndef INTERRA_TERRAIN
                float2 trackDetailUV = float2(terrainUV.x, -terrainUV.y) * _TrackDetailTexture_ST.xy * _TerrainSize.xz + _TrackDetailTexture_ST.zw;
            #else
                float2 trackDetailUV = float2(IN.tc.x, -IN.tc.y) * _TrackDetailTexture_ST.xy * _TerrainSizeXZPosY.xy + _TrackDetailTexture_ST.zw;
            #endif
       
            #if defined(PARALLAX) && defined(_TERRAIN_PARALLAX)
            if (_Terrain_Parallax)
            {
                float3  tangentViewDirTerrain = normalize(IN.tangentViewDir);
                float2 trackParallaxOffset = ParallaxOffset(_InTerra_TrackTexture, _ParallaxTrackSteps, -trackValues.y, trackUV, float3(-tangentViewDirTerrain.x, tangentViewDirTerrain.y, -tangentViewDirTerrain.z), _ParallaxTrackAffineSteps, 0, 1);

                trackUV += trackParallaxOffset;
                trackDetailUV += (trackParallaxOffset) * (_TrackDetailTexture_ST.xy * _InTerra_TrackArea);
            }
            #endif

            trackDepth = UNITY_SAMPLE_TEX2D_SAMPLER(_InTerra_TrackTexture, _Control, trackUV);          
            float4 trackDetail = UNITY_SAMPLE_TEX2D_SAMPLER(_TrackDetailTexture, _Splat0, trackDetailUV);

            float trackHeightMap = saturate(trackDepth.b + _TrackHeightOffset);
            float2 trackIntersect = float2(trackDepth.b, 1- trackDepth.b);
            trackIntersect *= (1 / (pow(2, float2(trackHeightMap, heightSum) * (-(_TrackHeightTransition)))) + 1) * 0.5;
            trackIntersect /= (trackIntersect.r + trackIntersect.g);

            float trackEdge = saturate(pow(trackDepth.a, _TrackEdgeSharpness));
            float track = trackIntersect.r * trackDist;
            float colorOpacity = saturate(track * trackColor.a);

            trackColor = lerp(trackColor, (trackColor * trackDetail), trackValues.a);
            mixedDiffuse.rgb = lerp(mixedDiffuse, trackColor, colorOpacity);
            mixedDiffuse.a = lerp(mixedDiffuse.a, trackValues.x, track);
       
            #if defined(_NORMALMAPS) || defined(_TERRAIN_NORMAL_IN_MASK)
            if (trackDepth.a > 0.0001f)
            {
            
                float3 trackDetailNormal = UnpackNormalWithScale(UNITY_SAMPLE_TEX2D_SAMPLER(_TrackDetailNormalTexture, _Splat0, trackDetailUV), _TrackDetailNormalStrenght);
                trackDetailNormal.y *= -1;

                float normalsOffset = _InTerra_TrackTexture_TexelSize * 2.0f;
                float texelArea = _InTerra_TrackTexture_TexelSize * 100 * _InTerra_TrackArea;
                float normalStrenghts = _TrackNormalStrenght / texelArea;
                float normalEdgeStrenghts = _TrackEdgeNormals / texelArea;

                float4 heights[4];
                 heights[0] = UNITY_SAMPLE_TEX2D_SAMPLER(_InTerra_TrackTexture, _Control, trackUV + float2(0.0,  normalsOffset));
                 heights[1] = UNITY_SAMPLE_TEX2D_SAMPLER(_InTerra_TrackTexture, _Control, trackUV + float2( normalsOffset, 0.0));
                 heights[2] = UNITY_SAMPLE_TEX2D_SAMPLER(_InTerra_TrackTexture, _Control, trackUV + float2(-normalsOffset, 0.0));
                 heights[3] = UNITY_SAMPLE_TEX2D_SAMPLER(_InTerra_TrackTexture, _Control, trackUV + float2(0.0, -normalsOffset));

                 for (int i = 0; i < 4; ++i)
                 {
                     heights[i] *= float4(1.0f ,1.0f, normalStrenghts, normalEdgeStrenghts);
                 }

                float3 edgeNormals = float3(float2(heights[1].a - heights[2].a, heights[3].a - heights[0].a), 1.0f);
                float3 trackNormal = float3(float2(heights[1].b - heights[2].b, heights[3].b - heights[0].b), 1.0f)  ;
                trackNormal = (lerp((trackNormal), BlendNormals(trackDetailNormal, trackNormal), trackValues.a)) * trackIntersect.r;
                trackNormal = normalize(lerp(edgeNormals, trackNormal, trackDepth.b));

                trackNormal.z += 1e-5f;
                trackNormal = (lerp(edgeNormals, trackNormal, trackDepth.b));
 
                float normalOpacity = saturate(trackValues.z * (trackEdge + track)) * trackDist;

                #ifndef INTERRA_TERRAIN
                    trackNormal = WorldTangent(worldTangent, worldBitangent, trackNormal);
                #endif

                mixedNormal = lerp(mixedNormal, trackNormal, normalOpacity);
            }
            #endif       

            #ifndef DIFFUSE
                ao = lerp(ao, _TrackAO, track);
            #endif
        }
    }
    #endif
    
    #ifndef DIFFUSE
        mixedDiffuse.a = lerp(mixedDiffuse.a, 1.0f, _InTerra_GlobalWetness);
        mixedNormal.xy = _InTerra_GlobalWetness > 0.4 ? mixedNormal.xy * (1 - min(0.8f, (_InTerra_GlobalWetness * 2.25f - 1))) : mixedNormal.xy;
    #endif
        
    //=======================================================================
    //=====================|   OBJECT INTEGRATION   |========================
    //=======================================================================
    #ifdef INTERRA_OBJECT
        #ifndef DIFFUSE
            objectAlbedo.a = _HasMask == 1 ? objectMask.a : _Glossiness;
            objectAlbedo.a = _GlobalWetnessDisabled ? objectAlbedo.a : lerp(objectAlbedo.a, 1.0f, _InTerra_GlobalWetness);

            half objectMetallic = _HasMask == 1 ? objectMask.r : _Metallic;
            half objectAo = _HasMask == 1 ? objectMask.g : _Ao;
        #endif

        float3 mainNormD = float3(0, 0, 1);
        if (_Detail > 0.0f)
        {
            float2 detailUV = TRANSFORM_TEX(IN.mainTC_tWeightY_hOffset.xy, _DetailAlbedoMap);
            #if defined(PARALLAX) && defined(_OBJECT_PARALLAX)
            if (_Object_Parallax)
            {
                detailUV += mainParallaxOffset * (_DetailAlbedoMap_ST.xy / _MainTex_ST.xy);
            }
            #endif
            #if !defined(_LAYERS_ONE)
                half3 detailTexture = UNITY_SAMPLE_TEX2D_SAMPLER(_DetailAlbedoMap, _MainTex, detailUV);
                mainNormD = UnpackNormalWithScale(UNITY_SAMPLE_TEX2D_SAMPLER(_DetailNormalMap, _MainTex, detailUV), _DetailNormalMapScale);
            #else
                half4 detailTexture = tex2D(_DetailAlbedoMap, detailUV);
                mainNormD = UnpackNormalWithScale(tex2D(_DetailNormalMap, detailUV), _DetailNormalMapScale);
            #endif              
            objectAlbedo.rgb = lerp(objectAlbedo.rgb, objectAlbedo.rgb * (detailTexture * unity_ColorSpaceDouble.r), _DetailStrenght);
        }

        fixed3 mainNorm = fixed3(0,0,1);
        #if !defined(_LAYERS_ONE)
            mainNorm = UnpackNormalWithScale(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, mainUV), _BumpScale);
        #else
            mainNorm = UnpackNormalWithScale(tex2D(_BumpMap, mainUV), _BumpScale);
        #endif          

        if (_Detail > 0.0f)
        {
            mainNorm = (lerp(mainNorm, BlendNormals(mainNorm, mainNormD), _DetailStrenght));
        }

        mixedDiffuse = lerp(mixedDiffuse, objectAlbedo, heightIntersect.r);
        float3 terrainNormal = float3(mixedNormal.z * IN.terrainNormals) + 1e-5f;
        terrainNormal.xy += mixedNormal.xy;

        float intersectNormal = smoothstep(_NormIntersect.y, _NormIntersect.x, IN.mainTC_tWeightY_hOffset.w);
        mixedNormal = lerp(mixedNormal, terrainNormal, intersectNormal);
        mixedNormal = lerp(mixedNormal, mainNorm, heightIntersect.r);

        #ifndef DIFFUSE
            metallic = lerp(metallic, objectMetallic, heightIntersect.r);
            ao = lerp(ao, objectAo, heightIntersect.r);

            emission = 0;
            UNITY_BRANCH if (_EmissionEnabled > 0)
            {
                float2 emissionUV = TRANSFORM_TEX(IN.mainTC_tWeightY_hOffset.xy, _EmissionMap);
                #if defined(PARALLAX) && defined(_OBJECT_PARALLAX)
                if (_Object_Parallax)
                {
                    emissionUV += mainParallaxOffset * (_EmissionMap_ST.xy / _MainTex_ST.xy);
                }
                #endif
                #if !defined(_LAYERS_ONE)
                    half4 emissionTexture = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap, _MainTex, emissionUV);
                #else
                    half4 emissionTexture = tex2D(_EmissionMap, emissionUV);
                #endif                 
                emission = lerp(half4(0, 0, 0, 0), emissionTexture * _EmissionColor, heightIntersect.r);
            }
        #endif           
    #endif

    //===============|   PUDDLES & RAINDROPS FINAL MIX   |====================
    #if !defined(DIFFUSE) && defined(_PUDDLES)
    if (_InTerra_GlobalPuddles.y > 0.0f)
    {
        float wettnessAround = smoothstep(_InTerra_GlobalPuddles.y, _InTerra_GlobalPuddles.x, heightSum) * horizontalWeight;
        mixedNormal.xy = wettnessAround > 0.4 ? mixedNormal.xy * (1 - min(0.7f, (wettnessAround * 2.25f - 1))) : mixedNormal.xy;
        mixedDiffuse.a = lerp(mixedDiffuse.a, 1.0f, wettnessAround);
        mixedDiffuse.a = lerp(mixedDiffuse.a, 1.0f, puddlesHeight.r * horizontalWeight);
        mixedNormal = lerp(mixedNormal, ripNormal, puddlesHeight.r * horizontalWeight);
        ao = lerp(ao, lerp(ao, 0.9f, 0.75f), puddlesHeight.r * horizontalWeight);
    }
    #endif
    //=======================================================================

    #if defined(DIFFUSE) && defined(_NORMALMAPS) 
        mixedNormal = mixedNormal + (splat[0].r * 0.000001f); //this is just a workaround for some Unity bug to prevent missing _Splat0 sampler
    #endif
            

    #if defined(INSTANCING_ON) && defined(SHADER_TARGET_SURFACE_ANALYSIS) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
        mixedNormal = float3(0, 0, 1); // make sure that surface shader compiler realizes we write to normal, as UNITY_INSTANCING_ENABLED is not defined for SHADER_TARGET_SURFACE_ANALYSIS.
    #endif

    #if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
        float3 geomNormal = normalize(tex2D(_TerrainNormalmapTexture, IN.tc.zw).xyz * 2 - 1);
        #if defined(_NORMALMAPS) || defined(_TERRAIN_NORMAL_IN_MASK)
            float3 geomTangent = normalize(cross(geomNormal, float3(0, 0, 1)));
            float3 geomBitangent = normalize(cross(geomTangent, geomNormal));
            mixedNormal = mixedNormal.x * geomTangent
                          + mixedNormal.y * geomBitangent
                          + mixedNormal.z * geomNormal;
        #else
            mixedNormal = geomNormal;
        #endif
        mixedNormal = mixedNormal.xzy;
    #endif
    mixedNormal.z += +1e-5f;
}

#ifndef TERRAIN_SURFACE_OUTPUT
    #define TERRAIN_SURFACE_OUTPUT SurfaceOutput
#endif

void SplatmapFinalColor(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 color)
{
    color *= o.Alpha;
    #ifdef TERRAIN_SPLAT_ADDPASS
        UNITY_APPLY_FOG_COLOR(IN.fogCoord, color, fixed4(0,0,0,0));
    #else
        UNITY_APPLY_FOG(IN.fogCoord, color);
    #endif
}

void SplatmapFinalPrepass(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 normalSpec)
{
    normalSpec *= o.Alpha;
}

void SplatmapFinalGBuffer(Input IN, TERRAIN_SURFACE_OUTPUT o, inout half4 outGBuffer0, inout half4 outGBuffer1, inout half4 outGBuffer2, inout half4 emission)
{
    UnityStandardDataApplyWeightToGbuffer(outGBuffer0, outGBuffer1, outGBuffer2, o.Alpha);
    emission *= o.Alpha;
}
