Shader "Hidden/RT2D/VarianceMipMapGenerator"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainTexLOD ("Texture LOD", Float) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Blend Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert_common
            #pragma fragment frag

            #include "RayTracing2DCommon.cginc"

            #pragma multi_compile ACCUMULATE SAMPLE_X SAMPLE_Y SAMPLE_Z SAMPLE_W


#if defined(SAMPLE_X)
#define SAMPLE_COMPONENT 0
#elif defined(SAMPLE_Y)
#define SAMPLE_COMPONENT 1
#elif defined(SAMPLE_Z)
#define SAMPLE_COMPONENT 2
#elif defined(SAMPLE_W)
#define SAMPLE_COMPONENT 3
#endif

#ifdef SAMPLE_COMPONENT
#define SAMPLE_COMPONENT_LOD(tex, uv, lod) SAMPLE_LOD(tex, uv, lod)[SAMPLE_COMPONENT]
#endif

            float4 frag(v2f_common i) : SV_Target
            {
                float2 sampleDelta = (1 - _ScreenParams.zw) / 2;

#ifdef ACCUMULATE
                float4 center = SAMPLE_LOD(_MainTex, i.uv, _MainTexLOD);

                float4 a = SAMPLE_LOD(_MainTex, i.uv + float2( 1,  1) * sampleDelta, _MainTexLOD);
                float4 b = SAMPLE_LOD(_MainTex, i.uv + float2(-1,  1) * sampleDelta, _MainTexLOD);
                float4 c = SAMPLE_LOD(_MainTex, i.uv + float2( 1, -1) * sampleDelta, _MainTexLOD);
                float4 d = SAMPLE_LOD(_MainTex, i.uv + float2(-1, -1) * sampleDelta, _MainTexLOD);

                float mean = center.x;
                float sum = a.y + b.y + c.y + d.y;
                float4 cornerMeanDiff = mean.xxxx - float4(a.x, b.x, c.x, d.x);
                float variance = center.y + dot(cornerMeanDiff, cornerMeanDiff) / 4;

                float dA = d.z - a.z;
                float dB = c.z - b.z;
#else
                float a = SAMPLE_COMPONENT_LOD(_MainTex, i.uv + float2( 1,  1) * sampleDelta, _MainTexLOD);
                float b = SAMPLE_COMPONENT_LOD(_MainTex, i.uv + float2(-1,  1) * sampleDelta, _MainTexLOD);
                float c = SAMPLE_COMPONENT_LOD(_MainTex, i.uv + float2( 1, -1) * sampleDelta, _MainTexLOD);
                float d = SAMPLE_COMPONENT_LOD(_MainTex, i.uv + float2(-1, -1) * sampleDelta, _MainTexLOD);

                float mean = (a + b + c + d) / 4;
                float sum = a + b + c + d;
                float ssum = a * a + b * b + c * c + d * d;
                float variance = -sum * sum / 4 + ssum;


                float dA = d - a;
                float dB = c - b;
#endif

                float dVariance = dA * dA + dB * dB;

                return float4(mean, sum, variance, dVariance);
            }

            ENDHLSL
        }
    }
}
