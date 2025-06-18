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

#if defined(_TERRAIN_MASK_MAPS) || defined(_TERRAIN_NORMAL_IN_MASK) || defined(_TERRAIN_MASK_HEIGHTMAP_ONLY)
    #define TERRAIN_MASK
#else
    #undef _TERRAIN_BLEND_HEIGHT
    #undef _TERRAIN_PARALLAX
    #undef _OBJECT_PARALLAX
#endif

#if (defined(_TERRAIN_TRIPLANAR) || defined(_OBJECT_TRIPLANAR) || defined(_TERRAIN_TRIPLANAR_ONE) || defined(_TRIPLANAR_ONE) || defined(_TRIPLANAR_ALL) ) && !defined(_TERRAIN_BASEMAP_GEN)
    #define TRIPLANAR
#endif

#if (defined(_TERRAIN_PARALLAX) || defined(_OBJECT_PARALLAX)) && !defined(TESSELLATION_ON) && !((defined(INTERRA_OBJECT) || defined(INTERRA_MESH_TERRAIN)) && ((SHADERPASS == SHADERPASS_RAYTRACING_INDIRECT) || (SHADERPASS == SHADERPASS_RAYTRACING_FORWARD) || (SHADERPASS == SHADERPASS_PATH_TRACING) || SHADERPASS == SHADERPASS_RAYTRACING_GBUFFER))
    #define PARALLAX
#endif
