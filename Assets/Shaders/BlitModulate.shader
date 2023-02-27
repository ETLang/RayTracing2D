Shader "Hidden/RT2D/BlitModulate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainTexLOD("Texture LOD", Float) = 0
        _ColorMod("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend Off
        //Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert_common
            #pragma fragment frag_blit_and_modulate
            #include "RayTracing2DCommon.cginc"
            ENDHLSL
        }
    }
}
