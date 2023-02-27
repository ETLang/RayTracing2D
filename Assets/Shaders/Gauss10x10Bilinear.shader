Shader "Hidden/Gauss10x10Bilinear"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainTexLOD("Texture LOD", Float) = 0
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

                float3 SampleData[] = 
                {
                    float3(-1.142857143, -1.142857143, 0.060616284),
                    float3(1.142857143, -1.142857143, 0.060616284),
                    float3(-1.142857143, 1.142857143, 0.060616284),
                    float3(1.142857143, 1.142857143, 0.060616284),
                    float3(-1.142857143, -2.285714286, 0.011021143),
                    float3(1.142857143, -2.285714286, 0.011021143),
                    float3(-2.285714286, -1.142857143, 0.011021143),
                    float3(2.285714286, -1.142857143, 0.011021143),
                    float3(-2.285714286, 1.142857143, 0.011021143),
                    float3(2.285714286, 1.142857143, 0.011021143),
                    float3(-1.142857143, 2.285714286, 0.011021143),
                    float3(1.142857143, 2.285714286, 0.011021143),
                    float3(0, -2.285714286, 0.018893387),
                    float3(-2.285714286, 0, 0.018893387),
                    float3(2.285714286, 0, 0.018893387),
                    float3(0, 2.285714286, 0.018893387),
                    float3(0, -1.142857143, 0.10391363),
                    float3(-1.142857143, 0, 0.10391363),
                    float3(1.142857143, 0, 0.10391363),
                    float3(0, 1.142857143, 0.10391363),
                    float3(0, 0, 0.178137652)
                };


            float4 frag(v2f_common input) : SV_Target
            {
                float2 sampleDelta = 1 / TextureSize(_MainTex, _MainTexLOD);

                float4 output = 0;

                //for (int i = 0; i < 21; i++)
                //    output += SAMPLE_LOD(_MainTex, input.uv + SampleData[i].xy, _MainTexLOD) * SampleData[i].z;

                int i;
                for (i = 0; i < 4; i++)
                    output += SAMPLE_LOD(_MainTex, input.uv + SampleData[i].xy, _MainTexLOD) * 0.060616284;

                for (; i < 12; i++)
                    output += SAMPLE_LOD(_MainTex, input.uv + SampleData[i].xy, _MainTexLOD) * 0.011021143;

                for (; i < 16; i++)
                    output += SAMPLE_LOD(_MainTex, input.uv + SampleData[i].xy, _MainTexLOD) * 0.018893387;

                for (; i < 20; i++)
                    output += SAMPLE_LOD(_MainTex, input.uv + SampleData[i].xy, _MainTexLOD) * 0.10391363;

                for (; i < 21; i++)
                    output += SAMPLE_LOD(_MainTex, input.uv + SampleData[i].xy, _MainTexLOD) * 0.178137652;

                output.a = 1;
                return output;
            }

            ENDHLSL
        }
    }
}
