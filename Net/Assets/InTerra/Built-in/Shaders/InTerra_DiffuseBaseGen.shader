Shader "Hidden/InTerra/InTerra-Diffuse-BaseGen"
{   
    Properties
    {
        [HideInInspector] _DstBlend("DstBlend", Float) = 0.0
        [HideInInspector] _HT_distance_scale("Scale",   Range(0,0.55)) = 0.2
        [HideInInspector] _HT_cover("Cover strenght",   Range(0,1)) = 0.6
    }

    SubShader
    {
        CGINCLUDE

        #include "UnityCG.cginc"

        #pragma multi_compile_local __ _TERRAIN_TRIPLANAR_ONE _TERRAIN_TRIPLANAR
        #pragma multi_compile_local __ _TERRAIN_DISTANCEBLEND  

        #define _TERRAIN_TINT_TEXTURE
        #define INTERRA_TERRAIN
        #define DIFFUSE
        #define TERRAIN_BASEGEN

        #include "InTerra_DefinedGlobalKeywords.cginc"
        #include "InTerra_InputsAndFunctions.cginc"

        struct appdata_t
        {
            float4 vertex : POSITION;
            float2 texcoord : TEXCOORD0;
        };

        float2 ComputeControlUV(float2 uv)
        {
            // adjust splatUVs so the edges of the terrain tile lie on pixel centers
            return (uv * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
        }

        ENDCG

        Pass
        {
            Tags
            {
                "Name" = "_MainTex"
                "Format" = "RGBA32"
                "Size" = "1"
            }

            ZTest Always Cull Off ZWrite Off
            Blend One[_DstBlend]
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
                #ifndef _LAYERS_TWO
                    float2 texcoord3 : TEXCOORD3;
                    float2 texcoord4 : TEXCOORD4;
                #endif
                float2 texcoord5 : TEXCOORD5;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float2 uv = TRANSFORM_TEX(v.texcoord, _Control);
                o.texcoord0 = ComputeControlUV(uv);
                o.texcoord1 = TRANSFORM_TEX(uv, _Splat0);
                o.texcoord2 = TRANSFORM_TEX(uv, _Splat1);
                #ifndef _LAYERS_TWO
                    o.texcoord3 = TRANSFORM_TEX(uv, _Splat2);
                    o.texcoord4 = TRANSFORM_TEX(uv, _Splat3);
                #endif
                o.texcoord5 = uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                half4 mixedDiffuse = 1;
                float3 mixedNormal = float3(0, 0, 1);
                float2 uvSplat[_LAYER_COUNT];
                half4 splat[_LAYER_COUNT], mask[_LAYER_COUNT];

                half4 blendMask[2];
                blendMask[0] = UNITY_SAMPLE_TEX2D(_Control, i.texcoord0);
                blendMask[1] = UNITY_SAMPLE_TEX2D_SAMPLER(_Control1, _Control, i.texcoord0);

                UvSplat(uvSplat, i.texcoord0);

                #ifdef _TERRAIN_DISTANCEBLEND
                    float4 distantDiffuse = 1;
                    float3 distantNormal = float3(0, 0, 1);

                    half4 dBlendMask[2];
                    dBlendMask[0] = blendMask[0];
                    dBlendMask[1] = blendMask[1];

                    float2 distantUV[_LAYER_COUNT];
                    half4 dSplat[_LAYER_COUNT], dMask[_LAYER_COUNT];

                    DistantUV(distantUV, uvSplat);
                    SampleMask(dMask, distantUV, dBlendMask, 1.0f);
                #endif

                SampleMask(mask, uvSplat, blendMask, 1.0f);
                //---------- HEIGHT MAP SPLAT BLENDINGS ---------
               #if defined(_TERRAIN_BLEND_HEIGHT)
                if (_NumLayersCount <= 4)
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

                SampleSplat(uvSplat, blendMask, 1.0f, mask, mixedDiffuse, mixedNormal, splat);
                #ifdef _TERRAIN_DISTANCEBLEND                     
                    SampleSplat(distantUV, dBlendMask, 1.0f, dMask, distantDiffuse, distantNormal, dSplat);
                    mixedDiffuse = lerp(mixedDiffuse, distantDiffuse, _HT_cover);
                #endif 
                #ifndef _TERRAIN_TRIPLANAR_ONE
                    half3 tint = tex2D(_TerrainColorTintTexture, i.texcoord5 * _TerrainColorTintTexture_ST.xy + _TerrainColorTintTexture_ST.zw);
                    mixedDiffuse.rgb = lerp(mixedDiffuse.rgb, ((mixedDiffuse.rgb) * (tint)), _TerrainColorTintStrenght).rgb;
                #endif
                return  mixedDiffuse;
            }
            ENDCG
        }

        Pass
        {
            Tags
            {
                "Name" = "_TriplanarTex"
                "Format" = "RGBA32"
                "Size" = "1"
            }

            ZTest Always Cull Off ZWrite Off
            Blend One[_DstBlend]
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
                
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord0 : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float2 uv = TRANSFORM_TEX(v.texcoord, _Control);
                o.texcoord0 = TRANSFORM_TEX(uv, _Splat0);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord0;                                
                                
               float4 splat0 = UNITY_SAMPLE_TEX2D(_Splat0, uv);

                #ifdef _TERRAIN_DISTANCEBLEND
                    float4 dSplat0 = UNITY_SAMPLE_TEX2D(_Splat0, uv * (_DiffuseRemapOffset0.r + 1) * _HT_distance_scale);
                    splat0 = lerp(splat0, dSplat0, _HT_cover);
                #endif

                return splat0;
            }
            ENDCG
        }          
    }
    Fallback Off
}
