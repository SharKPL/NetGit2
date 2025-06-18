Shader "InTerra/Built-in/Diffuse/Object into Terrain Integration (Diffuse)"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1
        _MainMask("Mask Map", 2D) = "grey" {}

        [HideInInspector] _HasMask("", Float) = 0
        [HideInInspector] _MaskMapRemapOffset("", Vector) = (0,0,0,0)
        [HideInInspector] _MaskMapRemapScale("", Vector) = (1,1,1,1)

        [HideInInspector] _Detail("", Float) = 1
        _DetailAlbedoMap("Detail Albedo", 2D) = "gery" {}
        _DetailNormalMap("Detail Normal Map", 2D) = "bump" {}
        _DetailNormalMapScale("Normal Scale", Float) = 1
        _DetailStrenght("Detail Strenght", Range(0,1)) = 0.5
        _DetailNormalStrenght("Detail Strenght", Range(0,1)) = 0.5

        _Intersection("Intersection Values", Vector) = (-0.5,0.6,-2,2)
        _Sharpness("Sharpness", Range(30,100)) = 80
        _NormIntersect("Normals intersection", Vector) = (0, 0.5, 0, 1)
       
        _Intersection2("Intersection2", Vector) = (0.3,0.6,0,1)
        _Steepness("Steepness", Range(-0.7,0.2)) = -0.3
        _SteepDistortion("Distortion", Range(0,3)) = 0
        _SteepIntersection("Steep intersection", Float) = 0

        [Toggle(_OBJECT_TRIPLANR)] _Triplanar("Triplanar", Float) = 1
        [Toggle(_OBJECT_DISABLE_OFFSET)]_DisableOffsetY("Disable Offset", Float) = 0
        [Toggle(_OBJECT_DISABLE_DISTANCEBLEND)]_DisableDistanceBlending("Disable Hide Tiling", Float) = 0

        [HideInInspector] _HT_distance_scale("Scale",   Range(0,0.55)) = 0.2
        [HideInInspector] _HT_distance("Distance",  vector) = (0,20,0,100)
        [HideInInspector] _HT_cover("Cover strenght",   Range(0,1)) = 0.6
        [HideInInspector] _HeightTransition("Height blending Sharpness",   Range(0,60)) = 40
        [HideInInspector] _Distance_HeightTransition("Distance Height blending Sharpness ", Range(0,60)) = 40

        [HideInInspector] _LayerIndex1("", Float) = 0
        [HideInInspector] _LayerIndex2("", Float) = 1
        [HideInInspector] _ControlNumber("", Float) = 0
        [HideInInspector] _PassNumber("", float) = 0
             
        [HideInInspector] _Splat0("Splat 0", 2D) = "white" {}
        [HideInInspector] _Splat1("Splat 1", 2D) = "white" {}
        [HideInInspector] _Splat2("Splat 2", 2D) = "white" {}
        [HideInInspector] _Splat3("Splat 3", 2D) = "white" {}
        [HideInInspector] _Splat4("Splat 4", 2D) = "white" {}
        [HideInInspector] _Splat5("Splat 5", 2D) = "white" {}
        [HideInInspector] _Splat6("Splat 6", 2D) = "white" {}
        [HideInInspector] _Splat7("Splat 7", 2D) = "white" {}

        [HideInInspector] _Normal0("Normal 0", 2D) = "bump" {}
        [HideInInspector] _Normal1("Normal 1", 2D) = "bump" {}
        [HideInInspector] _Normal2("Normal 2", 2D) = "bump" {}
        [HideInInspector] _Normal3("Normal 3", 2D) = "bump" {}
        [HideInInspector] _Normal4("Normal 4", 2D) = "bump" {}
        [HideInInspector] _Normal5("Normal 5", 2D) = "bump" {}
        [HideInInspector] _Normal6("Normal 6", 2D) = "bump" {}
        [HideInInspector] _Normal7("Normal 7", 2D) = "bump" {}

        [HideInInspector] _Mask0("Mask 0", 2D) = "black" {}
        [HideInInspector] _Mask1("Mask 1", 2D) = "black" {}
        [HideInInspector] _Mask2("Mask 2", 2D) = "black" {}
        [HideInInspector] _Mask3("Mask 3", 2D) = "black" {}
        [HideInInspector] _Mask4("Mask 4", 2D) = "black" {}
        [HideInInspector] _Mask5("Mask 5", 2D) = "black" {}
        [HideInInspector] _Mask6("Mask 6", 2D) = "black" {}
        [HideInInspector] _Mask7("Mask 7", 2D) = "black" {}

        [HideInInspector] _Specular0("specular 0", Color) = (1,0,0,0)
        [HideInInspector] _Specular1("specular 1", Color) = (1,0,0,0)
        [HideInInspector] _Specular2("specular 2", Color) = (1,0,0,0)
        [HideInInspector] _Specular3("specular 3", Color) = (1,0,0,0)
        [HideInInspector] _Specular4("specular 4", Color) = (1,0,0,0)
        [HideInInspector] _Specular5("specular 5", Color) = (1,0,0,0)
        [HideInInspector] _Specular6("specular 6", Color) = (1,0,0,0)
        [HideInInspector] _Specular7("specular 7", Color) = (1,0,0,0)

        [HideInInspector] _SplatUV0("splat UV0", Vector) = (1,1,0,0)
        [HideInInspector] _SplatUV1("splat sUV1", Vector) = (1,1,0,0)
        [HideInInspector] _SplatUV2("splat sUV2", Vector) = (1,1,0,0)
        [HideInInspector] _SplatUV3("splat sUV3", Vector) = (1,1,0,0)
        [HideInInspector] _SplatUV4("splat sUV4", Vector) = (1,1,0,0)
        [HideInInspector] _SplatUV5("splat sUV5", Vector) = (1,1,0,0)
        [HideInInspector] _SplatUV6("splat sUV6", Vector) = (1,1,0,0)
        [HideInInspector] _SplatUV7("splat sUV7", Vector) = (1,1,0,0)

        [HideInInspector] _MaskMapRemapScale0("", Vector) = (0,0,0,0)
        [HideInInspector] _MaskMapRemapScale1("", Vector) = (0,0,0,0)
        [HideInInspector] _MaskMapRemapScale2("", Vector) = (0,0,0,0)
        [HideInInspector] _MaskMapRemapScale3("", Vector) = (0,0,0,0)
        [HideInInspector] _MaskMapRemapScale4("", Vector) = (0,0,0,0)
        [HideInInspector] _MaskMapRemapScale5("", Vector) = (0,0,0,0)
        [HideInInspector] _MaskMapRemapScale6("", Vector) = (0,0,0,0)
        [HideInInspector] _MaskMapRemapScale7("", Vector) = (0,0,0,0)

        [HideInInspector] _MaskMapRemapOffset0("", Vector) = (0,0,0,0)
        [HideInInspector] _MaskMapRemapOffset1("", Vector) = (0,0,0,0)
        [HideInInspector] _MaskMapRemapOffset2("", Vector) = (0,0,0,0)
        [HideInInspector] _MaskMapRemapOffset3("", Vector) = (0,0,0,0)
        [HideInInspector] _MaskMapRemapOffset4("", Vector) = (0,0,0,0)
        [HideInInspector] _MaskMapRemapOffset5("", Vector) = (0,0,0,0)
        [HideInInspector] _MaskMapRemapOffset6("", Vector) = (0,0,0,0)
        [HideInInspector] _MaskMapRemapOffset7("", Vector) = (0,0,0,0)

        [HideInInspector] _DiffuseRemapScale0("", Vector) = (1,1,1,1)
        [HideInInspector] _DiffuseRemapScale1("", Vector) = (1,1,1,1)
        [HideInInspector] _DiffuseRemapScale2("", Vector) = (1,1,1,1)
        [HideInInspector] _DiffuseRemapScale3("", Vector) = (1,1,1,1)
        [HideInInspector] _DiffuseRemapScale4("", Vector) = (1,1,1,1)
        [HideInInspector] _DiffuseRemapScale5("", Vector) = (1,1,1,1)
        [HideInInspector] _DiffuseRemapScale6("", Vector) = (1,1,1,1)
        [HideInInspector] _DiffuseRemapScale7("", Vector) = (1,1,1,1)

        [HideInInspector] _DiffuseRemapOffset0("", Vector) = (0,0,0,0)
        [HideInInspector] _DiffuseRemapOffset1("", Vector) = (0,0,0,0)
        [HideInInspector] _DiffuseRemapOffset2("", Vector) = (0,0,0,0)
        [HideInInspector] _DiffuseRemapOffset3("", Vector) = (0,0,0,0)
        [HideInInspector] _DiffuseRemapOffset4("", Vector) = (0,0,0,0)
        [HideInInspector] _DiffuseRemapOffset5("", Vector) = (0,0,0,0)
        [HideInInspector] _DiffuseRemapOffset6("", Vector) = (0,0,0,0)
        [HideInInspector] _DiffuseRemapOffset7("", Vector) = (0,0,0,0)

        [HideInInspector] _Smoothness0("", Float) = 0
        [HideInInspector] _Smoothness1("", Float) = 0
        [HideInInspector] _Smoothness2("", Float) = 0
        [HideInInspector] _Smoothness3("", Float) = 0
        [HideInInspector] _Smoothness4("", Float) = 0
        [HideInInspector] _Smoothness5("", Float) = 0
        [HideInInspector] _Smoothness6("", Float) = 0
        [HideInInspector] _Smoothness7("", Float) = 0

        [HideInInspector] _Metallic0("", Float) = 0
        [HideInInspector] _Metallic1("", Float) = 0
        [HideInInspector] _Metallic2("", Float) = 0
        [HideInInspector] _Metallic3("", Float) = 0
        [HideInInspector] _Metallic4("", Float) = 0
        [HideInInspector] _Metallic5("", Float) = 0
        [HideInInspector] _Metallic6("", Float) = 0
        [HideInInspector] _Metallic7("", Float) = 0

        [HideInInspector] _NormalScale0("", Float) = 1
        [HideInInspector] _NormalScale1("", Float) = 1
        [HideInInspector] _NormalScale2("", Float) = 1
        [HideInInspector] _NormalScale3("", Float) = 1
        [HideInInspector] _NormalScale4("", Float) = 1
        [HideInInspector] _NormalScale5("", Float) = 1
        [HideInInspector] _NormalScale6("", Float) = 1
        [HideInInspector] _NormalScale7("", Float) = 1

        [HideInInspector] _TerrainNormalmapTexture("", 2D) = "green" {}
        [HideInInspector] _TerrainHeightmapTexture("", 2D) = "black" {}
        [HideInInspector] _Control("cntrl", 2D) = "red" {}
        [HideInInspector] _Control1("cntrl", 2D) = "black" {}

        [HideInInspector] _TerrainPosition("tp", Vector) = (0,0,0)
        [HideInInspector] _TerrainSize("ts", Vector) = (0,0,0)
        [HideInInspector] _TerrainHeightmapScale("hms", Vector) = (0,0,0)

        [HideInInspector] _TriplanarOneToAllSteep("", Float) = 0
        [HideInInspector] _TriplanarSharpness("Triplanar Sharpness", Range(3,10)) = 8

        [HideInInspector] _TerrainColorTintTexture("Color Tint Texture", 2D) = "white" {}
        [HideInInspector] _TerrainColorTintStrenght("Color Tint Strenght", Range(1, 0)) = 0

        [HideInInspector] _TerrainNormalTintTexture("Additional Normal Texture", 2D) = "bump" {}
        [HideInInspector] _TerrainNormalTintStrenght("Additional Normal Strenght", Range(0, 1)) = 0.0
        [HideInInspector] _TerrainNormalTintDistance("Additional Normal Distance", vector) = (3, 10, 0, 25)

        [HideInInspector] _WorldMapping("", Float) = 0
        [HideInInspector] _CustomTerrainSelection("", Float) = 0
        [HideInInspector] _GlobalWetness("", Range(0, 1)) = 0
        [HideInInspector] _MeshTerrain("", Float) = 0
        [HideInInspector] _MipMapFade(" MipMap fade", vector) = (3, 15, 0, 35)
        [HideInInspector] _MipMapLevel(" MipMap level", Float) = 0

        [HideInInspector] _HeightmapBlending("", Float) = 1
        [HideInInspector] _Terrain_Parallax("", Float) = 0
        [HideInInspector] _Object_Parallax("", Float) = 0
        [HideInInspector] _Tracks("", Float) = 0

        [HideInInspector] _TrackAO("", Range(0, 1)) = 0.8
        [HideInInspector] _TrackEdgeNormals("Track Edge index", Range(0.001, 4)) = 1
        [HideInInspector] _TrackDetailTexture("Track Color Detail Texture", 2D) = "white" {}
        [HideInInspector] _TrackDetailNormalTexture("Track Normal Detail Texture", 2D) = "bump" {}
        [HideInInspector] _TrackDetailNormalStrenght("Track Detail Normal Strenght", Float) = 1
        [HideInInspector] _TrackNormalStrenght("Track Normal Strenght", Float) = 1
        [HideInInspector] _TrackHeightOffset("Track Heightmap Offset", Range(-1, 1)) = 0
        [HideInInspector] _TrackMultiplyStrenght("Track Multiply strenght", Float) = 3
        [HideInInspector] _TrackHeightTransition("Track Normal Strenght", Range(0, 60)) = 20
        [HideInInspector] _ParallaxTrackAffineSteps("", Float) = 3
        [HideInInspector] _ParallaxTrackSteps("", Float) = 5

        _GlobalWetnessDisabled("", Float) = 0
    }

    SubShader {
        Tags { "RenderType" = "Opaque"}
		LOD 200

        CGPROGRAM
        #pragma surface surf Lambert vertex:SplatmapVert addshadow fullforwardshadows

        #pragma target 3.0
        #pragma shader_feature_local _TERRAIN_DISTANCEBLEND
        #pragma shader_feature_local _OBJECT_TRIPLANAR
        #pragma shader_feature_local ONE_PASS _LAYERS_ONE _LAYERS_TWO

        #define DIFFUSE
        #define INTERRA_OBJECT
  
        #include "InTerra_DefinedGlobalKeywords.cginc"

        #if defined(_LAYERS_ONE) || defined(_LAYERS_TWO)
            #undef _LAYERS_EIGHT
        #endif

        #include "InTerra_InputsAndFunctions.cginc"
        #include "InTerra_Mixing.cginc"
      
        //===========================================================================
        //--------------------------------- SURFACE ---------------------------------
        //===========================================================================
        void surf (Input IN, inout SurfaceOutput o) {
            half weight;
            fixed4 mixedDiffuse;

            SplatmapMix(IN, weight, mixedDiffuse, o.Normal);
            o.Albedo = mixedDiffuse.rgb;
        }
        ENDCG 
    }
    FallBack "Diffuse"
	CustomEditor "InTerra.InTerra_ObjectShaderGUI"
}