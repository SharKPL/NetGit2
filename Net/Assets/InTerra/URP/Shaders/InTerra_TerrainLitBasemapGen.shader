//NOTE! This file is based on Unity file "TerrainLitBasemapGen.shader" which was used as a template for adding all the InTerra features.
Shader "Hidden/InTerra/Lit (Basemap Gen)"
{
    Properties
    {
        // Layer count is passed down to guide height-blend enable/disable, due
        // to the fact that heigh-based blend will be broken with multipass.
        [HideInInspector] [PerRendererData] _NumLayersCount ("Total Layer Count", Float) = 1.0
        [HideInInspector] _Control("AlphaMap", 2D) = "" {}

        [HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
        [HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
        [HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
        [HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
        [HideInInspector] _Mask3("Mask 3 (A)", 2D) = "grey" {}
        [HideInInspector] _Mask2("Mask 2 (B)", 2D) = "grey" {}
        [HideInInspector] _Mask1("Mask 1 (G)", 2D) = "grey" {}
        [HideInInspector] _Mask0("Mask 0 (R)", 2D) = "grey" {}
        [HideInInspector] [Gamma] _Metallic0 ("Metallic 0", Range(0.0, 1.0)) = 0.0
        [HideInInspector] [Gamma] _Metallic1 ("Metallic 1", Range(0.0, 1.0)) = 0.0
        [HideInInspector] [Gamma] _Metallic2 ("Metallic 2", Range(0.0, 1.0)) = 0.0
        [HideInInspector] [Gamma] _Metallic3 ("Metallic 3", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _Smoothness0 ("Smoothness 0", Range(0.0, 1.0)) = 1.0
        [HideInInspector] _Smoothness1 ("Smoothness 1", Range(0.0, 1.0)) = 1.0
        [HideInInspector] _Smoothness2 ("Smoothness 2", Range(0.0, 1.0)) = 1.0
        [HideInInspector] _Smoothness3 ("Smoothness 3", Range(0.0, 1.0)) = 1.0

        [HideInInspector] _DstBlend("DstBlend", Float) = 0.0
    }

    Subshader
    {

        HLSLINCLUDE
        #pragma target 3.0

        #define _METALLICSPECGLOSSMAP 1
        #define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A 1
        #define _TERRAIN_BASEMAP_GEN

        #pragma shader_feature_local _MASKMAP

        #pragma shader_feature_local __ _TERRAIN_TRIPLANAR_ONE _TERRAIN_TRIPLANAR
        #pragma shader_feature_local _TERRAIN_DISTANCEBLEND
        #pragma shader_feature_local __ _LAYERS_TWO _LAYERS_EIGHT

        #include "InTerra_URP_DefinedGlobalKeywords.hlsl"

        #define INTERRA_TERRAIN 

        #if defined(_TERRAIN_TRIPLANAR) || defined(_TERRAIN_TRIPLANAR_ONE)
            #ifdef _TERRAIN_TRIPLANAR_ONE
                #define TRIPLANAR_TINT
            #endif
            #undef _TERRAIN_TRIPLANAR
            #undef _TERRAIN_TRIPLANAR_ONE
        #endif

        #include "InTerra_TerrainLitInput.hlsl"
        #include "InTerra_TerrainLitPasses.hlsl"

        ENDHLSL

        PackageRequirements { "com.unity.render-pipelines.universal":"[12.1,19.0]" }
        //Tags {"RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            Tags
            {
                "Name" = "_MainTex"
                "Format" = "ARGB32"
                "Size" = "1"
            }

            ZTest Always Cull Off ZWrite Off
            Blend One [_DstBlend]
            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            Varyings Vert(Attributes IN)
            {
                Varyings output = (Varyings) 0;

                output.clipPos = TransformWorldToHClip(IN.positionOS.xyz);

                // NOTE : This is basically coming from the vertex shader in TerrainLitPasses
                // There are other plenty of other values that the original version computes, but for this
                // pass, we are only interested in a few, so I'm just skipping the rest.
                output.uvMainAndLM.xy = IN.texcoord;
                output.uvSplat01.xy = TRANSFORM_TEX(IN.texcoord, _Splat0);
                output.uvSplat01.zw = TRANSFORM_TEX(IN.texcoord, _Splat1);
                #ifndef _LAYERS_TWO
                    output.uvSplat23.xy = TRANSFORM_TEX(IN.texcoord, _Splat2);
                    output.uvSplat23.zw = TRANSFORM_TEX(IN.texcoord, _Splat3);

                    #ifdef _LAYERS_EIGHT
                        output.uvSplat45.xy = TRANSFORM_TEX(IN.texcoord, _Splat4);
                        output.uvSplat45.zw = TRANSFORM_TEX(IN.texcoord, _Splat5);
                        output.uvSplat67.xy = TRANSFORM_TEX(IN.texcoord, _Splat6);
                        output.uvSplat67.zw = TRANSFORM_TEX(IN.texcoord, _Splat7);
                    #endif
                #endif

                return output;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                half3 normalTS = half3(0.0h, 0.0h, 1.0h);
                half4 splatControl;
                half weight;
                half4 mixedDiffuse = 0.0h;
                half metallic;
                half occlusion;
                half smoothness;
                float2 uv[_LAYER_COUNT];
                float4 inUV[_LAYER_COUNT/2];
                inUV[0] = IN.uvSplat01;
                #ifndef _LAYERS_TWO
                    inUV[1] = IN.uvSplat23;
                    #ifdef _LAYERS_EIGHT
                        inUV[2] = IN.uvSplat45;
                        inUV[3] = IN.uvSplat67;
                    #endif
                #endif
                uvArray(uv, inUV);

                SplatmapMix(IN.uvMainAndLM, uv, float3(0, 0, 0), float3(0, 0, 0), IN.positionWS, weight, mixedDiffuse, smoothness, metallic, occlusion, normalTS);

                return half4(mixedDiffuse.rgb, smoothness);
            }

            ENDHLSL
        }

        Pass
        {
            Tags
            {
                "Name" = "_MetallicTex"
                "Format" = "RGBA32"
                "Size" = "1/4"
                "EmptyColor" = "FF000000"
            }

            ZTest Always Cull Off ZWrite Off
            Blend One [_DstBlend]

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            Varyings Vert(Attributes IN)
            {
                Varyings output = (Varyings)0;

                output.clipPos = TransformWorldToHClip(IN.positionOS.xyz);

                // This is just like the other in that it is from TerrainLitPasses
                output.uvMainAndLM.xy = IN.texcoord;
                output.uvSplat01.xy = TRANSFORM_TEX(IN.texcoord, _Splat0);
                output.uvSplat01.zw = TRANSFORM_TEX(IN.texcoord, _Splat1);
                #ifndef _LAYERS_TWO
                    output.uvSplat23.xy = TRANSFORM_TEX(IN.texcoord, _Splat2);
                    output.uvSplat23.zw = TRANSFORM_TEX(IN.texcoord, _Splat3);

                    #ifdef _LAYERS_EIGHT
                        output.uvSplat45.xy = TRANSFORM_TEX(IN.texcoord, _Splat4);
                        output.uvSplat45.zw = TRANSFORM_TEX(IN.texcoord, _Splat5);
                        output.uvSplat67.xy = TRANSFORM_TEX(IN.texcoord, _Splat6);
                        output.uvSplat67.zw = TRANSFORM_TEX(IN.texcoord, _Splat7);
                    #endif
                #endif

                return output;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                half3 normalTS = half3(0.0h, 0.0h, 1.0h);
                half4 splatControl;
                half weight;
                half4 mixedDiffuse;
                half4 defaultSmoothness;

                half4 masks[4];

                half metallic;
                half occlusion;
                half smoothness;
                float2 uv[_LAYER_COUNT];
                float4 inUV[_LAYER_COUNT/2];

                inUV[0] = IN.uvSplat01;
                #ifndef _LAYERS_TWO
                    inUV[1] = IN.uvSplat23;
                    #ifdef _LAYERS_EIGHT
                        inUV[2] = IN.uvSplat45;
                        inUV[3] = IN.uvSplat67;
                    #endif
                #endif

                uvArray(uv, inUV);

                SplatmapMix(IN.uvMainAndLM, uv, float3(0, 0, 0), float3(0, 0, 0), IN.positionWS, weight, mixedDiffuse, smoothness, metallic, occlusion, normalTS);

                return float4(metallic, occlusion, splatControl.r, SAMPLE_TEXTURE2D(_Splat0, sampler_Splat0, uv[0]).r * 1e-5f);
            }
            ENDHLSL
        }

        Pass
        {
            Tags
            {
                "Name" = "_TriplanarTex"
                "Format" = "ARGB32"
                "Size" = "1"
            }

            ZTest Always Cull Off ZWrite Off
            Blend One [_DstBlend]

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #define _TERRAIN_BASEMAP_GEN_TRIPLANAR

            Varyings Vert(Attributes IN)
            {
                Varyings output = (Varyings)0;

                output.clipPos = TransformWorldToHClip(IN.positionOS.xyz);

                // This is just like the other in that it is from TerrainLitPasses
                output.uvMainAndLM.xy = IN.texcoord;
                output.uvSplat01.xy = TRANSFORM_TEX(IN.texcoord, _Splat0);  

                return output;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                half3 normalTS = half3(0.0h, 0.0h, 1.0h);
                half4 splatControl = float4(1, 0, 0, 0);
                half weight;
                half4 mixedDiffuse;
                half4 defaultSmoothness;
                half metallic;
                half occlusion;
                half smoothness;
                float2 uv[_LAYER_COUNT];
                half4 albedo[_LAYER_COUNT];
                float4 mask[_LAYER_COUNT];
                float4 inUV[_LAYER_COUNT/2];
                inUV[0] = IN.uvSplat01;
                #ifndef _LAYERS_TWO
                    inUV[1] = IN.uvSplat23;
                    #ifdef _LAYERS_EIGHT
                        inUV[2] = IN.uvSplat45;
                        inUV[3] = IN.uvSplat67;
                    #endif
                #endif
                uvArray(uv, inUV);

                SplatmapMix(IN.uvMainAndLM, uv, float3(0, 0, 0), float3(0, 0, 0), IN.positionWS, weight, mixedDiffuse, smoothness, metallic, occlusion, normalTS);

                #ifdef _TERRAIN_DISTANCEBLEND
                    uv[0] *= (_DiffuseRemapOffset0.r + 1) * _HT_distance_scale;
                #endif
                mask[0] = Mask(0);
                mask[0] = RemapMask(0);

                mask[0] *= weight;

                albedo[0] = SAMPLE_TEXTURE2D(_Splat0, sampler_Splat0, uv[0]) * weight;

                albedo[0].rgb *= DiffuseRemap(0).xyz;
                albedo[0].a = Smoothness(0).x;

                return half4(albedo[0].rgb, albedo[0].a);
            }
            ENDHLSL
        }

        Pass
        {
            Tags
            {
                "Name" = "_Triplanar_MetallicAO"
                "Format" = "RGBA32"
                "Size" = "1/4"
            }
                 
            ZTest Always Cull Off ZWrite Off
            Blend One [_DstBlend]

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #define _TERRAIN_BASEMAP_GEN_TRIPLANAR

            Varyings Vert(Attributes IN)
            {
                Varyings output = (Varyings)0;

                output.clipPos = TransformWorldToHClip(IN.positionOS.xyz);

                // This is just like the other in that it is from TerrainLitPasses
                output.uvMainAndLM.xy = IN.texcoord;
                output.uvSplat01.xy = TRANSFORM_TEX(IN.texcoord, _Splat0);

                return output;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                half3 normalTS = half3(0.0h, 0.0h, 1.0h);
                half4 splatControl = float4(1, 0, 0, 0);
                half weight;
                half4 mixedDiffuse;
                half4 defaultSmoothness;
                half metallic;
                half occlusion;
                half smoothness;
                float2 uv[_LAYER_COUNT];
                float4 mask[_LAYER_COUNT];
                float4 inUV[_LAYER_COUNT/2];

                inUV[0] = IN.uvSplat01;
                #ifndef _LAYERS_TWO
                    inUV[1] = IN.uvSplat23;
                    #ifdef _LAYERS_EIGHT
                        inUV[2] = IN.uvSplat45;
                        inUV[3] = IN.uvSplat67;
                    #endif
                #endif
                uvArray(uv, inUV);

                SplatmapMix(IN.uvMainAndLM, uv, float3(0, 0, 0), float3(0, 0, 0), IN.positionWS, weight, mixedDiffuse, smoothness, metallic, occlusion, normalTS);

                mask[0] = Mask(0);
                mask[0] = RemapMask(0); 
                 
                mask[0] *= weight;

                #ifdef _TERRAIN_NORMAL_IN_MASK
                    occlusion = mask[0].r;
                #else
                    occlusion = mask[0].g;
                #endif  
                    
                #ifndef _TERRAIN_MASK_MAPS
                    metallic = _Metallic0;
                #else
                    metallic = mask[0].r;
                #endif

                return float4(metallic, occlusion, 0, SAMPLE_TEXTURE2D(_Splat0, sampler_Splat0, uv[0]).r * 1e-5f);
            }

            ENDHLSL
        }
    }
}
