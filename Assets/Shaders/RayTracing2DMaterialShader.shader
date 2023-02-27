Shader "Ray Tracing 2D/Sprite"
{
    Properties
    {
        _MainTex("Color Map", 2D) = "white" {}

        _Color("Color", Color) = (0.5, 0.5, 0.5, 1)
        _RendererColor("Renderer Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Flip("Flip", Vector) = (1, 1, 1, 1)
        //_EmissiveColor("Emissive Color", Color) = "clear"
        //_EmissiveMap("Emissive Map", 2D) = {}
        //_EmissiveIntensity("Emissive Intensity", Float) = 0

        [HDR] _OutscatteredEmissiveColor("Outscattered Emissive Color", Color) = (0, 0, 0, 0)
        [MainTexture] [HDR] _OutscatteredEmissiveMap("Outscattered Emissive Map", 2D) = "clear" {}
        _OutscatteredEmissiveIntensity("Outscattered Emissive Intensity", Range(0.0, 10.0)) = 0

        _Smoothness("Smoothness", Range(0.0, 1.0)) = 1
        _SmoothnessMap("Smoothness Map", 2D) = "clear" {}

        _Density("Density", Range(0.0, 1.0)) = 0.2
        _DensityMap("DensityMap", 2D) = "clear" {}

        [HideInInspector] _ObjectIndex("Object Index", Int) = 0
        //_MaterialIndex("Material Index", Int) = 0
        _Dielectric("Dielectric", Range(0.0, 1.0)) = 0
        _RefractionIndex("Refraction Index", Float) = 1
            
        // Shape properties
        _MaskTex("Mask", 2D) = "white" {}
        [Normal] _InteriorNormalMap("Interior Normal Map", 2D) = "bump" {}
    }

    HLSLINCLUDE
    //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "RayTracing2DCommon.cginc"
    ENDHLSL

    SubShader
    {
        //Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            //Blend One OneMinusSrcAlpha
            //Cull Off ZWrite Off ZTest Always
            Cull Off Blend Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct appdata
            {
                float3 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half4  color       : COLOR;
            };

            struct v2f
            {
                float2 uv_Primary : TEXCOORD0;
                float2 uv_InteriorNormalMap : TEXCOORD2;
                float2 uv_DensityMap : TEXCOORD5;
                float4 vertex : SV_POSITION;
                half4  color       : COLOR;
            };

            struct FragmentOutput
            {
                float4 OutScatter : SV_Target0;
                half4 ColorObject : SV_Target1;
                half4 NormalDensityBoundary : SV_Target2;
            };

#pragma multi_compile __ ENABLE_MAINTEX
            //TEXTURE2D(_MainTex);
            //SAMPLER(sampler_MainTex);
            //half4 _MainTex_ST;

#pragma multi_compile __ ENABLE_MASK
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

#pragma shader_feature ENABLE_SMOOTHNESS_MAP
            TEXTURE2D(_SmoothnessMap);
            SAMPLER(sampler_SmoothnessMap);
            float _Smoothness;

#pragma multi_compile __ ENABLE_OUTSCATTER_EMISSIVE_MAP
            TEXTURE2D(_OutscatteredEmissiveMap);
            SAMPLER(sampler_OutscatteredEmissiveMap);
            float4 _OutscatteredEmissiveColor;
            float _OutscatteredEmissiveIntensity;

#pragma shader_feature ENABLE_DENSITY_MAP
            TEXTURE2D(_DensityMap);
            SAMPLER(sampler_DensityMap);
            //half4 _DensityMap_ST;
            float _Density;

#pragma shader_feature ENABLE_INTERIOR_NORMAL_MAP
            TEXTURE2D(_InteriorNormalMap);
            SAMPLER(sampler_InteriorNormalMap);
            //half4 _InteriorNormalMap_ST;

            float4 _Color;
            float4 _RendererColor;
            float2 _Flip;
            int _ObjectIndex;
            float _Dielectric;
            float _RefractionIndex;

            v2f vert (appdata v)
            {
                v2f o;
                o.color = v.color;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv_Primary = TRANSFORM_TEX(v.uv, _MainTex);

#ifdef ENABLE_INTERIOR_NORMAL_MAP
                o.uv_InteriorNormalMap = 0;

                //o.uv_InteriorNormalMap = TRANSFORM_TEX(v.uv, _InteriorNormalMap);
#else
                o.uv_InteriorNormalMap = 0;
#endif

#ifdef ENABLE_DENSITY_MAP
                o.uv_DensityMap = 0;
                //o.uv_DensityMap = TRANSFORM_TEX(v.uv, _DensityMap);
#else
                o.uv_DensityMap = 0;
#endif

                return o;
            }

            FragmentOutput frag(v2f i)
            {
                FragmentOutput o;

                half4 color = _RendererColor * float4(i.color.rgb, 1);

#ifdef ENABLE_MAINTEX
                color *= SAMPLE(_MainTex, i.uv_Primary);
                clip(color.a - 0.5);
#endif

#ifdef ENABLE_MASK
                half mask = SAMPLE(_MaskTex, i.uv_Primary).x;
                clip(mask - 0.5);
#endif

#ifdef ENABLE_OUTSCATTER_EMISSIVE_MAP
                o.OutScatter = SAMPLE(_OutscatteredEmissiveMap, i.uv_Primary) * _OutscatteredEmissiveColor;
#else
                o.OutScatter = _OutscatteredEmissiveColor;
#endif
                o.ColorObject = half4(color.rgb * color.a, _ObjectIndex);
                o.NormalDensityBoundary = half4(1, 0, 0, 1);

                return o;
            }
            ENDHLSL
        }
    }

    CustomEditor "RayTracing2DMaterialEditor"
}
