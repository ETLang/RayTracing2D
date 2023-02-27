using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RayTracing2D
{
    public static class RT2DMaterialShaderSidecar
    {
        static readonly int PID_ObjectIndex = Shader.PropertyToID("_ObjectIndex");

        static readonly int PID_MainTex = Shader.PropertyToID("_MainTex");
        //static readonly int PID_Color = Shader.PropertyToID("_Color");
        static readonly int PID_RenderColor = Shader.PropertyToID("_RenderColor");
        static readonly int PID_MainTex_ST = Shader.PropertyToID("_MainTex_ST");
        static readonly int PID_Mask = Shader.PropertyToID("_MaskTex");
        static readonly int PID_InteriorNormalMap = Shader.PropertyToID("_InteriorNormalMap");

        static readonly int PID_OutscatteredEmissiveMap = Shader.PropertyToID("_OutscatteredEmissiveMap");
        static readonly int PID_OutscatteredEmissiveColor = Shader.PropertyToID("_OutscatteredEmissiveColor");
        static readonly int PID_OutscatteredEmissiveIntensity = Shader.PropertyToID("_OutscatteredEmissiveIntensity");
        static readonly int PID_OutscatteredEmissiveMap_ST = Shader.PropertyToID("_OutscatteredEmissiveMap_ST");

        static readonly int PID_SmoothnessMap = Shader.PropertyToID("_SmoothnessMap");
        static readonly int PID_Smoothness = Shader.PropertyToID("_Smoothness");

        static readonly int PID_DensityMap = Shader.PropertyToID("_DensityMap");
        static readonly int PID_Density = Shader.PropertyToID("_Density");

        static readonly int PID_Dielectric = Shader.PropertyToID("_Dielectric");
        static readonly int PID_RefractionIndex = Shader.PropertyToID("_RefractinIndex");

        public static Texture2D NullTexture => _NullTexture ?? (_NullTexture = new Texture2D(1, 1));
        private static Texture2D _NullTexture;

        private static bool HasTexture(Material mat, MaterialPropertyBlock block, int id)
        {
            var blockTex = block?.GetTexture(id);

            return block != null && block.GetTexture(id) != null || mat.HasProperty(id) && mat.GetTexture(id) != null;
        }

        public static void FixKeywords(Material mat, MaterialPropertyBlock block = null)
        {
            mat.EnableKeyword("ENABLE_MAINTEX");

            if (HasTexture(mat, block, PID_Mask))
                mat.EnableKeyword("ENABLE_MASK");
            else
                mat.DisableKeyword("ENABLE_MASK");

            if (HasTexture(mat, block, PID_OutscatteredEmissiveMap))
                mat.EnableKeyword("ENABLE_OUTSCATTER_EMISSIVE_MAP");
            else
                mat.DisableKeyword("ENABLE_OUTSCATTER_EMISSIVE_MAP");

            if (HasTexture(mat, block, PID_SmoothnessMap))
                mat.EnableKeyword("ENABLE_SMOOTHNESS_MAP");
            else
                mat.DisableKeyword("ENABLE_SMOOTHNESS_MAP");

            if (HasTexture(mat, block, PID_DensityMap))
                mat.EnableKeyword("ENABLE_DENSITY_MAP");
            else
                mat.DisableKeyword("ENABLE_DENSITY_MAP");

            if (HasTexture(mat, block, PID_InteriorNormalMap))
                mat.EnableKeyword("ENABLE_INTERIOR_NORMAL_MAP");
            else
                mat.DisableKeyword("ENABLE_INTERIOR_NORMAL_MAP");
        }

        public static void ComputeOptimizedProperties(Material mat)
        {
        }
    }
}
