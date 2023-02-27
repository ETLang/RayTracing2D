Shader "Hidden/RT2D/BlitAdd"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainTexLOD("Texture LOD", Float) = 0
    }
    
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend One One

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert_common
            #pragma fragment frag_blit
            #include "RayTracing2DCommon.cginc"
            ENDHLSL
        }
    }
}
