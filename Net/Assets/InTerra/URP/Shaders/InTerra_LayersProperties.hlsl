#if defined(INTERRA_OBJECT)
#define SplatST(n) float4 _SplatUV##n
#else
#define SplatST(n) float4 _Splat##n##_ST
#endif


#define DECLARE_TERRAIN_LAYER_PROPS(n)          \
    float _Metallic##n;                         \
    float _Smoothness##n;                       \
    float _NormalScale##n;                      \
    float4 _DiffuseRemapOffset##n;              \
    float4 _DiffuseRemapScale##n;               \
    float4 _MaskMapRemapOffset##n;              \
    float4 _MaskMapRemapScale##n;               \
    float _LayerHasMask##n;                     \
    float4 _Mask##n##_TexelSize;                \
    float4 _Specular##n;                        \
    TEXTURE2D(_Normal##n);                      \
    TEXTURE2D(_Splat##n);                       \
    TEXTURE2D(_Mask##n);                        \
    SplatST(n);                                 \

