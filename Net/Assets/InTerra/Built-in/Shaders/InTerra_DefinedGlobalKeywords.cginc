//NOTE: This file should not be rewritten manually, you can change the defined keywords via MASK MAP MODE setting and GLOBAL SHADER RESTRICTIONS setting!
#define _TERRAIN_MASK_MAPS
#undef _TERRAIN_NORMAL_IN_MASK
#undef _TERRAIN_MASK_HEIGHTMAP_ONLY
 
#define _NORMALMAPS
#define _TERRAIN_BLEND_HEIGHT
#define _TERRAIN_PARALLAX
#define _TRACKS
#ifdef INTERRA_OBJECT
#define _OBJECT_PARALLAX
#endif
#define _PUDDLES
#undef _LAYERS_EIGHT

#if !defined(DIFFUSE)
    #if defined(_TERRAIN_MASK_MAPS) || defined(_TERRAIN_NORMAL_IN_MASK) || defined(_TERRAIN_MASK_HEIGHTMAP_ONLY)
        #define TERRAIN_MASK
    #else
        #undef _TERRAIN_BLEND_HEIGHT
        #undef _TERRAIN_PARALLAX
        #undef _OBJECT_PARALLAX
    #endif
#else
    #undef _TERRAIN_MASK_MAPS
    #undef _TERRAIN_NORMAL_IN_MASK
    #undef _TERRAIN_MASK_HEIGHTMAP_ONLY
    #undef _OBJECT_PARALLAX
    #undef _TERRAIN_PARALLAX
#endif

#if (defined(_TERRAIN_TRIPLANAR) || defined(_OBJECT_TRIPLANAR) || defined(_TERRAIN_TRIPLANAR_ONE)) && !defined(_TERRAIN_BASEMAP_GEN)
    #define TRIPLANAR
#endif

#if (defined(_TERRAIN_PARALLAX) || defined(_OBJECT_PARALLAX)) && (defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))) && !defined(DIFFUSE)
    #define PARALLAX
#endif

