Shader "Hidden/RT2D/OpIntegratePointCloud"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainTexLOD("Texture LOD", Float) = 0
        _MaxMipLevel("Max Mip Level", Float) = 8
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend One One

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert_common
            #pragma fragment frag_op
            #include "RayTracing2DCommon.cginc"

            float _MaxMipLevel;

            float4 frag_op(v2f_common input) : SV_Target
            {
                float currentLOD = _MaxMipLevel / 2;
                float stepSize = _MaxMipLevel / 4;
                float4 currentSample = SAMPLE_LOD(_MainTex, input.uv, currentLOD);
                for(int i = 0;i < 10;i++)
                {
                    float eps = currentSample.a - 512;
                    if (abs(eps) < 1)
                        break;
                    else if (eps > 0)
                        currentLOD -= stepSize;
                    else
                        currentLOD += stepSize; 

                    stepSize /= 2;
                    currentSample = SAMPLE_LOD(_MainTex, input.uv, currentLOD);
                }

                float4 final = currentSample;
                //float4 final = SAMPLE_LOD(_MainTex, input.uv, currentLOD +2);
                final.a = 1;
                return final;
            }

            ENDHLSL
        }
    }
}
