#if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN) 
    #if defined (_OBJECT_TRIPLANAR) || defined (_TERRAIN_TRIPLANAR_ONE) || defined (_TERRAIN_TRIPLANAR_ALL)
        #define TRIPLANAR
    #endif
#endif

#ifdef _LAYERS_ONE
    #define _LAYER_COUNT 1
#else
    #ifdef _LAYERS_TWO
        #define _LAYER_COUNT 2
    #else
        #if defined(_LAYERS_EIGHT) 
            #define _TERRAIN_8_LAYERS
            #define _LAYER_COUNT 8
        #else
            #define _LAYER_COUNT 4
        #endif
    #endif
#endif

#if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN) 
    #include "InTerra_LayersProperties.hlsl"
#endif

//----- Global Properties -----
float _InTerra_TrackArea;
float3 _InTerra_TrackPosition;
TEXTURE2D(_InTerra_TrackTexture);
float4 _InTerra_TrackTexture_TexelSize;
float _InTerra_TracksLayer;
float _InTerra_TracksFading;
float _InTerra_TracksFadingTime;
float _InTerra_TrackTextureSize;
float _InTerra_TrackLayer;

float _InTerra_GlobalWetness;
float3 _InTerra_GlobalPuddles;
float3 _InTerra_GlobalRaindropRipples;

//==========================================================================================
//======================================   FUNCTIONS   =====================================
//==========================================================================================
float2 ObjectFrontUV(float posOffset, half4 splatUV, float offsetZ)
{
    return  float2((posOffset + splatUV.z) / splatUV.x, (offsetZ + splatUV.w) / splatUV.y);
}

float2 ObjectSideUV(float posOffset, half4 splatUV, float offsetX)
{
    return  float2((offsetX + splatUV.z) / splatUV.x, (posOffset + splatUV.w) / splatUV.y);
}

half3 WorldTangent(float3 wTangent, float3 wBTangent, half3 mixedNormal)
{
    mixedNormal.xy = mul(float2x2(wTangent.xz, wBTangent.xz), mixedNormal.xy);
    return  half3(mixedNormal);
}

half2 HeightBlendTwoTextures(float2 splat, float2 heights, half sharpness)
{
    splat *= (1 / (1 * pow(2, heights * (-(sharpness)))) + 1) * 0.5;
    splat /= (splat.r + splat.g);

    return  splat;
}

half3 UnpackNormalGAWithScale(half4 packednormal, float scale)
{
    half3 normal;
    normal.xy = (packednormal.wy * 2 - 1) * scale;
    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));

    return normal;
}

#ifndef INTERRA_OBJECT 
    void uvArray(in out float2 uv[_LAYER_COUNT], float4 inUv[_LAYER_COUNT/2])
    {
        uv[0] = inUv[0].xy;
        uv[1] = inUv[0].zw;
        #ifndef _LAYERS_TWO
            uv[2] = inUv[1].xy;
            uv[3] = inUv[1].zw;
            #ifdef _LAYERS_EIGHT
                uv[4] = inUv[2].xy;
                uv[5] = inUv[2].zw;
                uv[6] = inUv[3].xy;
                uv[7] = inUv[3].zw;
            #endif
        #endif
    }
#endif

#if defined(INTERRA_MESH_TERRAIN)
    float2 TerrainFrontUV(float3 wPos, half4 splatUV, float2 tc, float3 flip)
    {
        return  float2(tc.x * -flip.z, (wPos.y - _TerrainSizeXZPosY.z) * (splatUV.y / _TerrainSizeXZPosY.y) + splatUV.w);
    }

    float2 TerrainSideUV(float3 wPos, half4 splatUV, float2 tc, float3 flip)
    {
        return  float2(tc.y * flip.x, (wPos.y - _TerrainSizeXZPosY.z) * (splatUV.x / _TerrainSizeXZPosY.x) + splatUV.z);
    }
#endif

void TriplanarOneToAllSteep(in out half4 blendMask[2], float weightY, in out half splatWeight)
{
    if (_TriplanarOneToAllSteep == 1)
    {
        #if !defined(TERRAIN_SPLAT_ADDPASS) 
            blendMask[0] = float4(saturate(blendMask[0].r + weightY), saturate((blendMask[0].gba) - weightY));
            blendMask[1] = float4(saturate((blendMask[1].rgba) - weightY));
            splatWeight = saturate(splatWeight + weightY);

            blendMask[0] = float4(saturate(blendMask[0].r + weightY), saturate(blendMask[0].gba - weightY));
            splatWeight = saturate(splatWeight + weightY);
        #else
            blendMask[0] = float4(saturate(blendMask[0].rgba - weightY));
            splatWeight = saturate(splatWeight - weightY);
        #endif
    }
}

half3 TriplanarNormal(half3 normal, half3 tangent, half3 bTangent, half3 normal_front, half3 normal_side, float3 weights, half3 flipUV)
{
    #ifdef INTERRA_OBJECT
        normal_front.y *= -flipUV.z;
        normal_front.xy = mul(float2x2(tangent.xy, bTangent.xy), normal_front.xy);

        normal_side.x *= -flipUV.x;
        normal_side.xy = mul(float2x2(tangent.yz, bTangent.yz), normal_side.xy);
    #else
        normal_side.xy = normal_side.yx; //this is needed because the uv was rotated
        normal_front.xy *= -flipUV.z;       
        normal_side.x *= -flipUV.x;
        normal_side.y *= flipUV.x;
    #endif

    return half3 (normal+ normal_front + normal_side);
}

#if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN) 
    #define DiffuseRemap(i) float4(_DiffuseRemapScale##i.xyzw)
#else
    #define DiffuseRemap(i) float4(_DiffuseRemapScale##i.xyzw + _DiffuseRemapOffset##i.xyzw)
#endif

#if defined (PARALLAX)

    #define MipMapLod(i, lod) float(_MipMapLevel + (lod * log2(max(_Mask##i##_TexelSize.z, _Mask##i##_TexelSize.w)) + 1))

    float GetParallaxHeight(texture2D maskT, sampler maskS, float2 uv, float lod, float2 offset, int invert)
    {
        return abs(SAMPLE_TEXTURE2D_LOD(maskT, maskS, float2(uv + offset), lod).b -invert);
    }

    //this function is based on Parallax Occlusion Mapping from Shader Graph URP
    float2 ParallaxOffset(texture2D maskT, sampler maskS, int numSteps, float amplitude, float2 uv, float3 tangentViewDir, float affineSteps, float lod, int invert)
    {    
        float2 offset = 0;

        if (numSteps > 0)
        {
            float3 viewDir = float3(tangentViewDir.xy * amplitude * -0.01, tangentViewDir.z);
            float stepSize = (1.0 / numSteps);

            float2 texOffsetPerStep = stepSize * viewDir.xy;
                

            // Do a first step before the loop to init all value correctly
            float2 texOffsetCurrent = float2(0.0, 0.0); 
            float prevHeight = GetParallaxHeight(maskT, maskS, uv, lod, texOffsetCurrent, invert);
            texOffsetCurrent += texOffsetPerStep;
            float currHeight = GetParallaxHeight(maskT, maskS, uv, lod, texOffsetCurrent, invert);
            float rayHeight = 1.0 - stepSize; // Start at top less one sample

            for (int stepIndex = 0; stepIndex < numSteps; ++stepIndex)
            {
                // Have we found a height below our ray height ? then we have an intersection
                if (currHeight > rayHeight)
                    break; // end the loop

                prevHeight = currHeight;
                rayHeight -= stepSize;
                texOffsetCurrent += texOffsetPerStep;

                currHeight = GetParallaxHeight(maskT, maskS, uv, lod, texOffsetCurrent, invert);
            }

            if (affineSteps <= 1)
            {
                float delta0 = currHeight - rayHeight;
                float delta1 = (rayHeight + stepSize) - prevHeight;
                float ratio = delta0 / (delta0 + delta1);
                offset = texOffsetCurrent - ratio * texOffsetPerStep;

                currHeight = GetParallaxHeight(maskT, maskS, uv, lod, texOffsetCurrent, invert);
            }
            else
            {
                float pt0 = rayHeight + stepSize;
                float pt1 = rayHeight;
                float delta0 = pt0 - prevHeight;
                float delta1 = pt1 - currHeight;

                float delta;

                // Secant method to affine the search
                // Ref: Faster Relief Mapping Using the Secant Method - Eric Risser
                for (int i = 0; i < affineSteps; ++i)
                {
                    // intersectionHeight is the height [0..1] for the intersection between view ray and heightfield line
                    float intersectionHeight = (pt0 * delta1 - pt1 * delta0) / (delta1 - delta0);
                    // Retrieve offset require to find this intersectionHeight
                    offset = (1 - intersectionHeight) * texOffsetPerStep * numSteps;

                    currHeight = GetParallaxHeight(maskT, maskS, uv, lod, offset, invert);

                    delta = intersectionHeight - currHeight;

                    if (abs(delta) <= 0.01)
                        break;

                    // intersectionHeight < currHeight => new lower bounds
                    if (delta < 0.0)
                    {
                        delta1 = delta;
                        pt1 = intersectionHeight;
                    }
                    else
                    {
                        delta0 = delta;
                        pt0 = intersectionHeight;
                    }
                }
            }
        }  
        return offset;
    }

 
    void ParallaxUV(inout float2 uv[_LAYER_COUNT], float3 tangentViewDir, half4 blendMask[2],float weight, float lod)
    {
        #define uvParallax(i, blendMask)                                \
        UNITY_BRANCH if (blendMask * weight  > 0.01f)                   \
        {                                                               \
            uv[i] += ParallaxOffset(_Mask##i, SamplerState_Linear_Repeat, _DiffuseRemapOffset##i.w,  DiffuseRemap(i).w, uv[i], tangentViewDir, _ParallaxAffineStepsTerrain,  MipMapLod(i, lod), 0);\
        }                                                               \


        UNITY_BRANCH if (_Terrain_Parallax == 1)
        {
            uvParallax(0, blendMask[0].r);
            #ifndef _LAYERS_ONE
                uvParallax(1, blendMask[0].g);
                #ifndef _LAYERS_TWO
                    uvParallax(2, blendMask[0].b);
                    uvParallax(3, blendMask[0].a);
                    #ifdef _LAYERS_EIGHT
                        uvParallax(4, blendMask[1].r);
                        uvParallax(5, blendMask[1].g);
                        uvParallax(6, blendMask[1].b);
                        uvParallax(7, blendMask[1].a);
                    #endif
                #endif
            #endif
        }
    }
#endif


float3 UnpackNormals(float4 packednormal, float normalScale)
    {    
        #ifdef SURFACE_GRADIENT
            #ifdef UNITY_NO_DXT5nm
                return float3(UnpackDerivativeNormalRGB(packednormal, normalScale), 0);
            #else
                return float3(UnpackDerivativeNormalRGorAG(packednormal, normalScale), 0);
            #endif
        #else
            #ifdef UNITY_NO_DXT5nm
                return UnpackNormalRGB(packednormal, normalScale);
            #else
                return UnpackNormalmapRGorAG(packednormal, normalScale);
            #endif
        #endif
    }

    float3 UnpackNormalGAWithScale(float4 packednormal, float scale, half hasMask)
    {
        UNITY_BRANCH if (hasMask > 0)
        {
            #ifdef SURFACE_GRADIENT
                return float3(UnpackDerivativeNormalAG(packednormal, scale), 0);         
            #else
                return UnpackNormalAG(packednormal, scale);
            #endif
        }
        else
        {
            return float3(0, 0, 1);
        }
    }
 
    float3 BlendNormals(float3 n1, float3 n2)
    {
        #ifdef INTERRA_OBJECT
            float3 t = n1.xyz + float3(0.0, 0.0, 1.0);
            float3 u = n2.xyz * float3(-1.0, -1.0, 1.0);
            float3 r = (t / t.z) * dot(t, u) - u;
            return r;
        #else
            return (float3(n1.xy + n2.xy, n1.z));
        #endif
    }



    #if defined(_NORMALMAPS) && !defined(_TERRAIN_NORMAL_IN_MASK) 
        #define SampleNormals(i) (UnpackNormals(SAMPLE_TEXTURE2D(_Normal##i, SamplerState_Linear_Repeat, uv[i]), _NormalScale##i).xyz)
    #elif defined(_TERRAIN_NORMAL_IN_MASK) 
        #define SampleNormals(i) float3(UnpackNormalGAWithScale(mask[i], _NormalScale##i, _LayerHasMask##i).xyz)
    #else
        #define SampleNormals(i) float3(0, 0, 1)
    #endif

    float3 SmoothMaskOrAlbedo(half mask, half albedo, float hasMask, float smoothness)
    {
        UNITY_BRANCH if (hasMask > 0)
        {                                                                               
            albedo = mask;
        }
        else                                                                      
        {                                                                           
            albedo *= smoothness;
        }
        return albedo;
    }

    #ifdef _TERRAIN_MASK_MAPS
        #define Smoothness(i) SmoothMaskOrAlbedo(mask[i].a, albedo[i].a, _LayerHasMask##i, _Smoothness##i)
    #else
        #define Smoothness(i) albedo[i].a *= _Smoothness##i
    #endif

    #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN) 
        #ifdef INTERRA_OBJECT
            #define UV(i) (posOffset.xz + _SplatUV##i.zw  + _SteepDistortion) / _SplatUV##i.xy;
        #else
            #define UV(i) (posOffset.xz + _SplatUV##i.zw) / _SplatUV##i.xy;
        #endif
        #ifdef PARALLAX                                                      
            #define fUV(i) ObjectFrontUV(posOffset.x, _SplatUV##i, offsetZ + (_DiffuseRemapScale##i.w * 0.004 * _SplatUV##i.x) * -flip.z);
            #define sUV(i) ObjectSideUV(posOffset.z, _SplatUV##i, offsetX + (_DiffuseRemapScale##i.w * 0.004 * _SplatUV##i.y) * -flip.x);
        #else   
            #if defined(TESSELLATION_ON)
                #define fUV(i) ObjectFrontUV(posOffset.x, _SplatUV##i, offsetZ + (-_DiffuseRemapOffset##i.y * 0.005 - _TerrainTessOffset) * -flip.z);
                #define sUV(i) ObjectSideUV(posOffset.z, _SplatUV##i, offsetX + (-_DiffuseRemapOffset##i.y * 0.005 - _TerrainTessOffset) * -flip.x);
            #else
                #define fUV(i) ObjectFrontUV(posOffset.x, _SplatUV##i, offsetZ);
                #define sUV(i) ObjectSideUV(posOffset.z, _SplatUV##i, offsetX);    
            #endif
        #endif
    #else
        #define UV(i) uv[i];
        #define fUV(i) TerrainFrontUV(worldPos, _Splat##i##_ST, uvSplat[i], flip);  
        #define sUV(i) TerrainSideUV(worldPos, _Splat##i##_ST, uvSplat[i], flip);

    #endif

    float4 RemapMasks(half4 mask, float4 remapScale, float4 remapOffset)
    {
        #ifdef _TERRAIN_NORMAL_IN_MASK
            mask.rb * remapScale.gb + remapOffset.gb;
            return mask;
        #else
            return mask * remapScale + remapOffset;
        #endif

    }

    #ifdef TERRAIN_MASK
        #define Mask(i) SAMPLE_TEXTURE2D(_Mask##i, SamplerState_Linear_Repeat, uv[i]);

        #ifdef _TERRAIN_NORMAL_IN_MASK
            #define RemapMask(i) mask[i] * float4(_MaskMapRemapScale##i.g, 1, _MaskMapRemapScale##i.b, 1)  \
                                        + float4(_MaskMapRemapOffset##i.g, 0, _MaskMapRemapOffset##i.b, 0);
        #else
            #define RemapMask(i) mask[i] * _MaskMapRemapScale##i + _MaskMapRemapOffset##i;
        #endif
    #else
        #define Mask(i) float4(_Metallic##i, 1, 0.5, 0);
        #define RemapMask(i) mask[i];
    #endif

    void SampleMask(out half4 mask[_LAYER_COUNT], float2 uv[_LAYER_COUNT], half4 blendMask[2], float weight)
    {
    #define SampleMasks(i, blendMask)                                       \
        UNITY_BRANCH if (blendMask * weight > 1e-5f )                       \
        {                                                                   \
            mask[i] = Mask(i);                                              \
            mask[i] = RemapMask(i);                                         \
        }                                                                   \
        else                                                                \
        {                                                                   \
            mask[i] = float4(_Metallic##i, 1, 0.5, 0);                      \
        }                                                                   \

        SampleMasks(0, blendMask[0].r);
        #ifndef _LAYERS_ONE
            SampleMasks(1, blendMask[0].g);
            #ifndef _LAYERS_TWO
                SampleMasks(2, blendMask[0].b);
                SampleMasks(3, blendMask[0].a);
                #ifdef _TERRAIN_8_LAYERS
                    SampleMasks(4, blendMask[1].r);
                    SampleMasks(5, blendMask[1].g);
                    SampleMasks(6, blendMask[1].b);
                    SampleMasks(7, blendMask[1].a);
                #endif
            #endif
        #endif
        #undef SampleMasks
    }

    void SampleSplat(float2 uv[_LAYER_COUNT], half4 blendMask[2], float weight, inout half4 mask[_LAYER_COUNT], out float4 mixAlbedo, out float3 mixNormal)
    {
        float4 albedo[_LAYER_COUNT];
        float3 normal[_LAYER_COUNT];

        mixAlbedo = 0;
        mixNormal = 0;

        #define Samples(i,  blendMask)  blendMask *= weight;                            \
        UNITY_BRANCH if (blendMask > 1e-5f)                                             \
        {                                                                               \
            albedo[i] = SAMPLE_TEXTURE2D(_Splat##i, sampler_Splat0, uv[i]);             \
            albedo[i].rgb *= DiffuseRemap(i).xyz;                                       \
            albedo[i].a = Smoothness(i).x;                                              \
            normal[i] = SampleNormals(i).xyz;                                           \
            mixAlbedo += albedo[i].xyzw * blendMask.x;                                  \
            mixNormal += normal[i].xyz * blendMask.x;                                   \
        }                                                                               \
        else                                                                            \
        {                                                                               \
            albedo[i] = float4(0, 0, 0, 0);                                             \
            normal[i] = float3(0, 0, 1);                                                \
        }                                                                               \


        Samples(0, blendMask[0].r);
        #ifndef _LAYERS_ONE
            Samples(1, blendMask[0].g);
            #ifndef _LAYERS_TWO
                Samples(2, blendMask[0].b);
                Samples(3, blendMask[0].a);
                #ifdef _TERRAIN_8_LAYERS
                    Samples(4, blendMask[1].r);
                    Samples(5, blendMask[1].g);
                    Samples(6, blendMask[1].b);
                    Samples(7, blendMask[1].a);
                #endif
            #endif
        #endif
        #undef Samples       
    }

    #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN)  
        #ifndef TRIPLANAR
            void UvSplat(out float2 uvSplat[_LAYER_COUNT], float3 posOffset)
        #else
            void UvSplat(out float2 uvSplat[_LAYER_COUNT], out float2 uvFront[_LAYER_COUNT], out float2 uvSide[_LAYER_COUNT], float3 posOffset, float offsetZ, float offsetX, float3 flip)
        #endif
    #else
        #ifndef TRIPLANAR
            void UvSplat(out float2 uvSplat[_LAYER_COUNT], float2 uv[_LAYER_COUNT], float2 splatBaseUV)
        #else
            void UvSplat(out float2 uvSplat[_LAYER_COUNT], out float2 uvFront[_LAYER_COUNT], out float2 uvSide[_LAYER_COUNT], float3 worldPos, float2 uv[_LAYER_COUNT], float2 splatBaseUV, float3 flip)
        #endif
    #endif
    {
        #ifndef TRIPLANAR
            #define SplatUV(i);             \
            uvSplat[i] = UV(i);       
        #else
            #define SplatUV(i)              \
            uvSplat[i] = UV(i);             \
            uvFront[i] = fUV(i);            \
            uvSide[i] = sUV(i);             \

        #endif

        SplatUV(0);
        #ifndef _LAYERS_ONE
            SplatUV(1);
            #ifndef _LAYERS_TWO
                SplatUV(2);
                SplatUV(3);
                #ifdef _TERRAIN_8_LAYERS
                    SplatUV(4);
                    SplatUV(5);
                    SplatUV(6);
                    SplatUV(7);
                #endif
            #endif    
        #endif
    }

    void DistantUV(out float2 distantUV[_LAYER_COUNT], float2 uvSplat[_LAYER_COUNT])
    {
        #define uvDistant(i)                                                                \
        distantUV[i] = uvSplat[i] * (_DiffuseRemapOffset##i.r + 1) * _HT_distance_scale;    \
        
        uvDistant(0);
        #ifndef _LAYERS_ONE
            uvDistant(1);
            #ifndef _LAYERS_TWO
                uvDistant(2);
                uvDistant(3);
                #ifdef _TERRAIN_8_LAYERS
                    uvDistant(4);
                    uvDistant(5);
                    uvDistant(6);
                    uvDistant(7);
                #endif
            #endif    
        #endif
    }


    void MaskWeight(inout half4 mask[_LAYER_COUNT], half4 mask_front[_LAYER_COUNT], half4 mask_side[_LAYER_COUNT], half4 blendMask[2], inout float3 triplanarWeights, float heightBlendingSharpness)
    {
        float splatWeight[_LAYER_COUNT];
        float3 heights = 0;

        splatWeight[0] = blendMask[0].x;
        #ifndef _LAYERS_ONE
            splatWeight[1] = blendMask[0].y;
            #ifndef _LAYERS_TWO
                splatWeight[2] = blendMask[0].z;
                splatWeight[3] = blendMask[0].w;
                #ifdef _TERRAIN_8_LAYERS
                    splatWeight[4] = blendMask[1].x;
                    splatWeight[5] = blendMask[1].y;
                    splatWeight[6] = blendMask[1].z;
                    splatWeight[7] = blendMask[1].w;
                #endif
            #endif
        #endif

        for (int i = 0; i < _LAYER_COUNT; ++i)
        {
            mask[i] = (mask[i] * triplanarWeights.y) + (mask_front[i] * triplanarWeights.z) + (mask_side[i] * triplanarWeights.x);
        }
    }

    half4 MaskSplatWeight(half4 mask[_LAYER_COUNT],  half4 blendMask[2], out half4 mixedMask)
    {
        float splatWeight[_LAYER_COUNT];
        mixedMask = 0;

        #ifdef _TERRAIN_NORMAL_IN_MASK
            #define MixdMask(i,  blendMask) float4(_Metallic##i, mask[i].r, mask[i].b, 0.0f) * blendMask
        #elif defined(_TERRAIN_MASK_HEIGHTMAP_ONLY)
            #define MixdMask(i,  blendMask) float4(_Metallic##i, 1.0f, mask[i].b, 0.0f) * blendMask
        #else
            #define MixdMask(i,  blendMask) mask[i] * blendMask
        #endif


        #define MixdMasks(i,  blendMask)                        \
        UNITY_BRANCH if (blendMask > 0)                         \
        {                                                       \
            mixedMask += MixdMask(i, blendMask);                \
        }

        MixdMasks(0, blendMask[0].r);
        #ifndef _LAYERS_ONE
            MixdMasks(1, blendMask[0].g);
            #ifndef _LAYERS_TWO
                MixdMasks(2, blendMask[0].b);
                MixdMasks(3, blendMask[0].a);
                #ifdef _TERRAIN_8_LAYERS
                    MixdMasks(4, blendMask[1].r);
                    MixdMasks(5, blendMask[1].g);
                    MixdMasks(6, blendMask[1].b);
                    MixdMasks(7, blendMask[1].a);
                #endif
            #endif
        #endif

        return mixedMask;
    }

    #ifdef TERRAIN_MASK
        float AmbientOcclusion(half4 mask[_LAYER_COUNT], half4 blendMask[2])
        {
            float occlusion[_LAYER_COUNT];
            #ifdef _TERRAIN_NORMAL_IN_MASK
                UNITY_UNROLL for (int i = 0; i < _LAYER_COUNT; ++i)
                {
                    occlusion[i] = mask[i].r;
                }
            #else
                UNITY_UNROLL for (int i = 0; i < _LAYER_COUNT; ++i)
                {
                    occlusion[i] = mask[i].g;
                }
            #endif  

            float ao = 0;

            ao = occlusion[0] * blendMask[0].r;
            #ifndef _LAYERS_ONE
                ao += occlusion[1] * blendMask[0].g;
                #ifndef _LAYERS_TWO
                    ao += occlusion[2] * blendMask[0].b
                        + occlusion[3] * blendMask[0].a;
                    #ifdef _TERRAIN_8_LAYERS
                        ao += occlusion[4] * blendMask[1].r
                            + occlusion[5] * blendMask[1].g
                            + occlusion[6] * blendMask[1].b
                            + occlusion[7] * blendMask[1].a;
                    #endif
                #endif
            #endif
            return  ao;
        }
    #endif

    #ifndef _TERRAIN_MASK_MAPS
        #define Metallic(i, blendMask) _Metallic##i * blendMask;
    #else
        #define Metallic(i, blendMask) mask[i].r * blendMask;
    #endif

    float MetallicMask(half4 mask[_LAYER_COUNT], half4 blendMask[2])
    {    
        float metallic = 0;

        #define Metallics(i,  blendMask)                        \
        UNITY_BRANCH if (blendMask > 0)                         \
        {                                                       \
            metallic += Metallic(i, blendMask);                 \
        }

        Metallics(0, blendMask[0].r);
        #ifndef _LAYERS_ONE
            Metallics(1, blendMask[0].g);
            #ifndef _LAYERS_TWO
                Metallics(2, blendMask[0].b);
                Metallics(3, blendMask[0].a);
                #ifdef _TERRAIN_8_LAYERS
                    Metallics(4, blendMask[1].r);
                    Metallics(5, blendMask[1].g);
                    Metallics(6, blendMask[1].b);
                    Metallics(7, blendMask[1].a);
                #endif
            #endif
        #endif

        return metallic;
    } 

    float HeightSum(half4 mask[_LAYER_COUNT], half4 blendMask[2])
    {   
        #ifdef _LAYERS_ONE
            return float(mask[0].b);
        #else
            float heightSum = dot(blendMask[0].rg, float2(mask[0].b, mask[1].b));
            #ifndef _LAYERS_TWO
                heightSum += dot(blendMask[0].ba, float2(mask[2].b, mask[3].b));
                #ifdef _TERRAIN_8_LAYERS
                    heightSum += dot(blendMask[1], float4(mask[4].b, mask[5].b, mask[6].b, mask[7].b));
                #endif
            #endif
            return heightSum;
        #endif
    }

    float4 TrackSplatValues(half4 blendMask[2], float4 trackSplats[_LAYER_COUNT])
    {   
       #ifdef _LAYERS_ONE
            return trackSplats[0];
        #else
            float4 color = (blendMask[0].r * trackSplats[0])
                         + (blendMask[0].g * trackSplats[1]);
            #ifndef _LAYERS_TWO
                    color += (blendMask[0].b * trackSplats[2])
                           + (blendMask[0].a * trackSplats[3]);
                #ifdef _TERRAIN_8_LAYERS
                    color += (blendMask[1].r * trackSplats[4])
                           + (blendMask[1].g * trackSplats[5])
                           + (blendMask[1].b * trackSplats[6])
                           + (blendMask[1].a * trackSplats[7]);
                #endif
            #endif
            return color;
        #endif
    }


    #if defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN) 
        #define SpecularValueR(i) _Specular##i.r;
        #define SpecularValueG(i) _Specular##i.g;
        #define SpecularValueB(i) _Specular##i.b;
    #else
        #define SpecularValueR(i) _Gamma ? _Specular##i.r : pow(abs(_Specular##i.r),1/2.2f);
        #define SpecularValueG(i) _Gamma ? _Specular##i.g : pow(abs(_Specular##i.g),1/2.2f);
        #define SpecularValueB(i) _Gamma ? _Specular##i.b : pow(abs(_Specular##i.b),1/2.2f);
    #endif


    void UnpackTrackSplatValues(out float4 trackSplats[_LAYER_COUNT])
    {
        float value;
        int precision = 1024;
       
        #define trackSplat(i)  value =  SpecularValueR(i);                  \
        trackSplats[i].z  = value % precision;                              \
                            value = floor(value / precision);               \
        trackSplats[i].x  = value;                                          \
        trackSplats[i] /= (precision - 1);                                  \
                                                                            \
        trackSplats[i].y = (_DiffuseRemapOffset##i.w * 10.0f) % 1 ;         \
        trackSplats[i].w = floor((_DiffuseRemapOffset##i.w % 1 ) * 10.0f);  \
        
        trackSplat(0);
        #ifndef _LAYERS_ONE
            trackSplat(1);
            #ifndef _LAYERS_TWO
                trackSplat(2);
                trackSplat(3); 
                #ifdef _TERRAIN_8_LAYERS
                    trackSplat(4);
                    trackSplat(5);
                    trackSplat(6);
                    trackSplat(7);
                #endif
            #endif    
        #endif           
    }

    void UnpackTrackSplatColor(out float4 trackSplatsColor[_LAYER_COUNT])
    {
        float color;
        float value;
        int precision = 1024;

        #define trackSplatColor(i)  color = SpecularValueG(i)       \
                                                                    \
        trackSplatsColor[i].y = color % precision;                  \
        color = floor(color / precision);                           \
        trackSplatsColor[i].x = color;                              \
        value = SpecularValueB(i);                                  \
        trackSplatsColor[i].w  =   value % precision;               \
        value = floor(value / precision);                           \
        trackSplatsColor[i].z = value % precision;                  \
        trackSplatsColor[i] /= (precision - 1);                     \
        
        trackSplatColor(0);
        #ifndef _LAYERS_ONE
            trackSplatColor(1);
            #ifndef _LAYERS_TWO
                trackSplatColor(2);
                trackSplatColor(3);
                #ifdef _TERRAIN_8_LAYERS
                    trackSplatColor(4);
                    trackSplatColor(5);
                    trackSplatColor(6);
                    trackSplatColor(7);
                #endif
            #endif    
        #endif
    }

    #ifdef _TERRAIN_BLEND_HEIGHT
        void HeightBlend(half4 mask[_LAYER_COUNT], inout half4 blendMask[2], float sharpness)
        {
            #ifdef _LAYERS_TWO
                float2 height = float2(mask[0].b, mask[1].b);

                blendMask[0].rg *= (1 / (pow(2, (height + blendMask[0].rg) * (-(sharpness)))) + 1) * 0.5;
                blendMask[0].rg /= (blendMask[0].r + blendMask[0].g);
            #else
                float4 height = float4 (mask[0].b, mask[1].b, mask[2].b, mask[3].b);
                blendMask[0].rgba *= (1 / (pow(2, (height + blendMask[0].rgba) * (-(sharpness)))) + 1) * 0.5;
                float heightSum = blendMask[0].r + blendMask[0].g + blendMask[0].b + blendMask[0].a;
                #ifdef _TERRAIN_8_LAYERS  
                    float4 height1 = float4 (mask[4].b, mask[5].b, mask[6].b, mask[7].b);
                    blendMask[1].rgba *= (1 / (pow(2, (height1 + blendMask[1].rgba) * (-(sharpness)))) + 1) * 0.5;
                    heightSum += blendMask[1].r + blendMask[1].g + blendMask[1].b + blendMask[1].a;
                    blendMask[1].rgba /= heightSum;               
                #endif
                blendMask[0].rgba /= heightSum;
            #endif
        }
    #endif


    #ifndef _TERRAIN_BASEMAP_GEN
        void SampleSplatTOL(in out half4 mixedAlbedo, in out half3 mixedNormal, float2 uv[_LAYER_COUNT], half4 blendMask[2], float weight, half4 mask[_LAYER_COUNT])
        {
            float4 albedo[1];
            float3 normal[1];

             albedo[0] = float4(0, 0, 0, 0);
             normal[0] = float3(0, 0, 1);

             blendMask[0].r *= weight;

            #ifndef TERRAIN_SPLAT_ADDPASS
                UNITY_BRANCH if (blendMask[0].r > 1e-5f)
                {          
                    albedo[0] = SAMPLE_TEXTURE2D(_Splat0, sampler_Splat0, uv[0]);
                    albedo[0].rgb *= DiffuseRemap(0).rgb;
                    albedo[0].a = Smoothness(0).x;
                    normal[0] = SampleNormals(0);
                }

                mixedAlbedo = (albedo[0] * blendMask[0].r);
                mixedNormal = (normal[0] * blendMask[0].r);
            #else
                 mixedAlbedo = 0.0f;
                 mixedNormal = 0.0f;
            #endif
        }

        void SampleMaskTOL(out half4 mask[_LAYER_COUNT], half4 noTriplanarMask[_LAYER_COUNT], float2 uv[_LAYER_COUNT], float weight)
        {
            #ifndef TERRAIN_SPLAT_ADDPASS
            UNITY_BRANCH if (weight > 1e-5f)
            {
                mask[0] = Mask(0);
                mask[0] = RemapMask(0);
            }
            else
            {
                mask[0] = float4(0, 0, 0.5, 0);
            }
            #else
                mask[0] = noTriplanarMask[0];
            #endif                     
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
           
        #define TAU 6.283185307

        float random(in float2 st) 
        {
            return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
        }

        float2 rotate_2d(float2 p_input, float p_theta)
        {
            float2x2 l_rot_matrix = float2x2(cos(p_theta), -sin(p_theta),
                sin(p_theta), cos(p_theta));
            return mul(l_rot_matrix, p_input);
        }

        float3 RainRipples(float2 uv, float scale, float rotation)
        {
            float3 normal = float3(0, 0, 1);
            uv.xy = rotate_2d(uv.xy, rotation);
            float2 sUV = (uv.xy * scale + scale * 0.1f);
            sUV.x += step(1.0f, (sUV.y % 2.0)) * 0.5f;

            float2 center = float2(0.5f, 0.5f);
            float size = 0.25f;

            float2 tile = floor(sUV);
            float2 fract = frac(sUV);

            float2 offset = (center - fract.xy) * size;
            float2 cUV = (fract - 0.5) / size + offset;

            float2 polarUV = float2(length(cUV), atan2(cUV.y, cUV.x));
            float time = _Time.x * scale * 3.0f;
            float radius = frac(time * 0.9f + (random(floor(sUV) + 5000.0).x)) * 1.25f;

            if (radius < 0.8f)
            {
                float thickness = min((size * 0.5f + 0.25f) * 0.4f, radius);

                float start = radius + thickness;
                float end = max(0., radius - thickness);
                if (radius > 0.15)
                {
                    thickness *= (0.9 - (radius * 0.9f));
                }

                float radius2 = radius - thickness * 1.25f;
                float cAngle = smoothstep(start, end, polarUV.x) * PI;

                float start2 = radius2 + thickness;
                float end2 = max(0., radius2 - thickness);
                float cAngle2 = smoothstep(start2, end2, polarUV.x) * PI;

                float rippleMask = smoothstep(thickness, -0.1f, abs(polarUV.x - radius));
                float rippleMask2 = smoothstep(thickness, -0.1f, abs(polarUV.x - radius2));

                float decayMask = min(1., max(0., 1. - polarUV.x));
                cAngle = lerp(cAngle, PI * 0.5f, (1.0f - rippleMask * decayMask));
                cAngle2 = lerp(cAngle2, PI * 0.5f, (1.0f - rippleMask2 * decayMask));

                float c = lerp(cos(cAngle), cos(cAngle2), 0.5f);
                normal = float3(c * sin(polarUV.y), c * cos(polarUV.y), sin(cAngle));

                float opacity = _InTerra_GlobalRaindropRipples.y;
                normal.xy = rotate_2d(normal.xy, rotation) * (opacity - (radius * opacity));
            }
            return normal;
        }
    #endif
