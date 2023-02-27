Shader "Hidden/RT2D/OpGradient"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainTexLOD("Texture LOD", Float) = 0
        _PixelSize("Pixel Size", Vector) = (0, 0, 0, 0)
        _Spread("Spread", Float) = 1
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

            float2 _PixelSize;
            float _Spread;

            float4 frag(v2f_common i) : SV_Target
            {
                // Try: Maximize gradient, minimize divergence... might work?

                float4 pixel = SAMPLE_LOD(_MainTex, i.uv, _MainTexLOD);

                float4 l = SAMPLE_LOD(_MainTex, i.uv + float2(-1, 0) * _PixelSize, _MainTexLOD);
                float4 r = SAMPLE_LOD(_MainTex, i.uv + float2( 1, 0) * _PixelSize, _MainTexLOD);
                float4 u = SAMPLE_LOD(_MainTex, i.uv - float2(0,  1) * _PixelSize, _MainTexLOD);
                float4 d = SAMPLE_LOD(_MainTex, i.uv - float2(0, -1) * _PixelSize, _MainTexLOD);

                float4 nw = SAMPLE_LOD(_MainTex, i.uv + float2(-0.5, -0.5) * _PixelSize, _MainTexLOD);
                float4 ne = SAMPLE_LOD(_MainTex, i.uv + float2( 0.5, -0.5) * _PixelSize, _MainTexLOD);
                float4 se = SAMPLE_LOD(_MainTex, i.uv + float2( 0.5,  0.5) * _PixelSize, _MainTexLOD);
                float4 sw = SAMPLE_LOD(_MainTex, i.uv + float2(-0.5,  0.5) * _PixelSize, _MainTexLOD);

                l = (pixel - l) / _PixelSize.x;
                r = (r - pixel) / _PixelSize.x;
                u = (pixel - u) / _PixelSize.y;
                d = (d - pixel) / _PixelSize.y;
                /*
                
                Gx = (ne + se) / 2 - (nw + sw) / 2 = ( ne - nw + se - sw) / 2
                Gy = (sw + se) / 2 - (nw + ne) / 2 = (-ne - nw + se + sw) / 2
                
                */

                float2 gradient2 = float2((ne - nw + se - sw).w, (-ne - nw + se + sw).w) / _PixelSize;

                //return float4(0, pixel.a, 0, 1);

                float2 densityGradient = float2((l + r).w / 2, (u + d).w / 2);
                float2 densityDivergence = ((r - l) + (d - u)).xy;

                float gradient = length(densityGradient);
                float divergence = length(densityDivergence);

                float score = length(gradient2) / (0.1 + 3 * pixel.a);
                
                return float4(0, pow(score / 100, 2), pixel.a, 1);
            }

            ENDHLSL
        }
    }
}
