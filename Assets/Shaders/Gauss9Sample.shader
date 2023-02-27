Shader "Hidden/Gauss9Sample"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainTexLOD ("Texture LOD", Float) = 0
        _AlphaFactor("Alpha Factor", Float) = 1
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

            float2 _SampleScale;
            float _AlphaFactor;

            float4 frag(v2f_common i) : SV_Target
            {
                float2 sampleDelta = (1 - _ScreenParams.zw);
                float4 output = float4(
                    SAMPLE_LOD(_MainTex, i.uv + float2(-sampleDelta.x, -sampleDelta.y), _MainTexLOD) +
                    SAMPLE_LOD(_MainTex, i.uv + float2(-sampleDelta.x, 0), _MainTexLOD) * 2 +
                    SAMPLE_LOD(_MainTex, i.uv + float2(-sampleDelta.x, sampleDelta.y), _MainTexLOD) +
                    SAMPLE_LOD(_MainTex, i.uv + float2(0, -sampleDelta.y), _MainTexLOD) * 2 +
                    SAMPLE_LOD(_MainTex, i.uv + float2(0, 0), _MainTexLOD) * 4 +
                    SAMPLE_LOD(_MainTex, i.uv + float2(0, sampleDelta.y), _MainTexLOD) * 2 +
                    SAMPLE_LOD(_MainTex, i.uv + float2(sampleDelta.x, -sampleDelta.y), _MainTexLOD) +
                    SAMPLE_LOD(_MainTex, i.uv + float2(sampleDelta.x, 0), _MainTexLOD) * 2 +
                    SAMPLE_LOD(_MainTex, i.uv + float2(sampleDelta.x, sampleDelta.y), _MainTexLOD)
                    ) / 16;

                output.a *= _AlphaFactor;

                return output;
            }
                
            ENDHLSL
        }
    }
}
