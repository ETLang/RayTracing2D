Shader "Hidden/RT2D/OpLogIntensity"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainTexLOD("Texture LOD", Float) = 0
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert_common
            #pragma fragment frag
            #include "RayTracing2DCommon.cginc"

            float4 frag(v2f_common i) : SV_Target
            {
                return float4(log(Intensity(SAMPLE_MAIN(i.uv).rgb)), 0, 0, 1);
            }

            ENDHLSL
        }
    }
}
