Shader "Hidden/RT2D/BlitDiagnostic"
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
        Blend Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert_common
            #pragma fragment frag_diagnostic
            #include "RayTracing2DCommon.cginc"

            float _MaxMipLevel;

        float4 frag_diagnostic(v2f_common input) : SV_Target0
        {
            // return float4(1,0,0,1) ;
            // Try: Find sample LOD wh ere alpha == 16. Binary search!

            float currentLOD = 0;// _MaxMipLevel;
                float stepSize = _MaxMipLevel / 4;
                float4 currentSample = SAMPLE_LOD(_MainTex, input.uv, currentLOD);
                return float4(currentSample.xyz, 1);
                //float2 pixelSize = 1 / TextureSize(_MainTex, (int)currentLOD);

                //float L = SAMPLE_LOD(_MainTex, input.uv - float2(pixelSize.x, 0), currentLOD).z;
                //float R = SAMPLE_LOD(_MainTex, input.uv + float2(pixelSize.x, 0), currentLOD).z;
                //float U = SAMPLE_LOD(_MainTex, input.uv - float2(0, pixelSize.y), currentLOD).z;
                //float D = SAMPLE_LOD(_MainTex, input.uv + float2(0, pixelSize.y), currentLOD).z;

                //float dx = R - L;
                //float dy = U - D;

                //return float4(0, currentSample.y / 64, 0, 1);

                //float dVariance = (dx * dx + dy * dy) /1024 ;

                //dVariance = currentSample.w;
                int bestI = _MaxMipLevel;
                float4 bestSample = 0; // SAMPLE_LOD(_MainTex, input.uv, bestI);
                //float4 bestDVSample = bestSample;
                float bestDV = 1e64;// bestSample.w / (1 << (2 * bestI));

                //return float4(0, dVariance, 0, 1);

                for (int i = _MaxMipLevel; i > 0; i--)
                {
                    float2 pixelSize = 1 / TextureSize(_MainTex, i);

                    float4 c = SAMPLE_LOD(_MainTex, input.uv, i);
                    float4 L = SAMPLE_LOD(_MainTex, input.uv - float2(pixelSize.x, 0), i);
                    float4 R = SAMPLE_LOD(_MainTex, input.uv + float2(pixelSize.x, 0), i);
                    float4 U = SAMPLE_LOD(_MainTex, input.uv - float2(0, pixelSize.y), i);
                    float4 D = SAMPLE_LOD(_MainTex, input.uv + float2(0, pixelSize.y), i);

                    float dx = (R.z - L.z);
                    float dy = (U.z - D.z);

                    float dVariance = dx * dx + dy * dy;
                    float samples = c.y;

                    if (samples < 1024) continue;

                    if (dVariance < bestDV)
                    {
                        bestI = i;
                        bestSample = c;
                        bestDV = dVariance;
                    }

                    //currentSample = SAMPLE_LOD(_MainTex, input.uv, i);

                    //float nextDV = currentSample.w / (1 << (2*i));

                    //bestSample = currentSample;
                    //bestI = i;

                    //if (nextDV > bestDV) break;
                    //bestDV = nextDV;
                    //if (currentSample.y < 32) continue;
                }

                float4 final = SAMPLE_LOD(_MainTex, input.uv, bestI);
                return float4(0, final.x, 0, 1);
                //return float4(0, bestI / _MaxMipLevel, 0, 1);
                //return float4(3*bestSample.z / ((1 << (2*bestI))), 0, 0, 1);

                //for(i = 0;i < 5;i++)
                //{
                //    float eps = currentSample.y - 16;
                //    if (abs(eps) < 1)
                //        break;
                //    else if (eps > 0)
                //        currentLOD -= stepSize;
                //    else
                //        currentLOD += stepSize; 

                //    stepSize /= 2;
                //    currentSample = SAMPLE_LOD(_MainTex, input.uv, currentLOD);
                //}

                //float4 minDVSample = currentSample;

                //for (i = ceil(currentLOD); i <= _MaxMipLevel; i++)
                //{

                //    if (abs(eps) < 1)
                //        break;
                //    else if (eps > 0)
                //        currentLOD -= stepSize;
                //    else
                //        currentLOD += stepSize;

                //    stepSize /= 2;
                //    currentSample = SAMPLE_LOD(_MainTex, input.uv, currentLOD);
                //}

                //currentSample.a = 1;
                return float4(currentLOD / 20, 0, 0, 1);
                //return float4(0, dVariance, 0, 1);
            }

            ENDHLSL
        }
    }
}
