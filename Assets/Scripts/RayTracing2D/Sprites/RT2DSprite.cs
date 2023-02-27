using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace RayTracing2D
{
    [RequireComponent(typeof(SpriteRenderer))]
    [ExecuteInEditMode]
    public class RT2DSprite : MonoBehaviour, ISprite
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
        static readonly int PID_RefractionIndex = Shader.PropertyToID("_RefractionIndex");

        //public bool overrideAlbedo;
        public bool overrideMask;
        public bool overrideInteriorNormals;
        public bool overrideOutscatteredLightEmissive;
        public bool overrideSmoothness;
        public bool overrideDensity;
        public bool overrideDielectric;
        public bool overrideRefractionIndex;
        public bool overrideTiling;

        //public Texture2D albedoMap;
        //public Color albedo;

        public Texture2D mask;

        public Texture2D interiorNormals;

        public Texture2D outscatteredEmissionMap;
        public Color outscatteredEmissionEnergy;

        public Texture2D smoothnessMap;

        [Range(0, 1)]
        public float smoothness;

        public Texture2D densityMap;
        [Range(0, 1)]
        public float density;

        [Range(0, 1)]
        public float dielectric;

        public float refractionIndex = 1;

        public Vector4 tiling = new Vector4(1, 1, 0, 0);

        Material _originalMaterial;
        Material _materialInstance;
        SpriteRenderer _renderer;

        // Need way to:

        /*
         - Set sprite without accidentally making a leaked asset.
         - Set material to a standard sprite material
         - Optionally override all material properties
         - Programmatically report Object ID to renderer
         - Autogenerate edge normal map
        */

        public int RenderId { get; set; }

        // Start is called before the first frame update
        void Start()
        {
            _renderer = GetComponent<SpriteRenderer>();
            var sprite = _renderer.sprite;

            _originalMaterial = _renderer.sharedMaterial;

#if UNITY_EDITOR
            _materialInstance = new Material(_renderer.sharedMaterial);
            _renderer.sharedMaterial = _materialInstance;
#else
            _materialInstance = _renderer.material;
#endif
        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            if(_renderer.sharedMaterial != _originalMaterial && _renderer.sharedMaterial != _materialInstance)
            {
                _originalMaterial = _renderer.sharedMaterial;
                _materialInstance = new Material(_originalMaterial);
                _renderer.sharedMaterial = _materialInstance;
            }
#endif

            if (_materialInstance == null) return;

            if (_originalMaterial == null)
                _originalMaterial = new Material(_materialInstance);

            smoothness = Mathf.Clamp(smoothness, 0, 1);
            density = Mathf.Clamp(density, 0, 1);
            dielectric = Mathf.Clamp(dielectric, 0, 1);
            refractionIndex = Mathf.Max(refractionIndex, 0.001f);

            _materialInstance.SetColor(PID_RenderColor, Color.white);

            //if (overrideAlbedo)
            //{
            //    _materialInstance.SetTexture(PID_MainTex, albedoMap);
            //    _materialInstance.SetColor(PID_RenderColor, albedo);
            //}
            //else
            //{
            //    _materialInstance.SetColor(PID_RenderColor, Color.white);
            //}

            _materialInstance.SetTexture(PID_Mask, overrideMask ? mask : _originalMaterial.GetTexture(PID_Mask));
            _materialInstance.SetTexture(PID_InteriorNormalMap, overrideInteriorNormals ? interiorNormals : _originalMaterial.GetTexture(PID_InteriorNormalMap));
            _materialInstance.SetTexture(PID_OutscatteredEmissiveMap, overrideOutscatteredLightEmissive ? outscatteredEmissionMap : _originalMaterial.GetTexture(PID_OutscatteredEmissiveMap));
            _materialInstance.SetColor(PID_OutscatteredEmissiveColor, overrideOutscatteredLightEmissive ? outscatteredEmissionEnergy : _originalMaterial.GetColor(PID_OutscatteredEmissiveColor));
            _materialInstance.SetTexture(PID_SmoothnessMap, overrideSmoothness ? smoothnessMap : _originalMaterial.GetTexture(PID_SmoothnessMap));
            _materialInstance.SetFloat(PID_Smoothness, overrideSmoothness ? smoothness : _originalMaterial.GetFloat(PID_Smoothness));
            _materialInstance.SetTexture(PID_DensityMap, overrideDensity ? densityMap : _originalMaterial.GetTexture(PID_DensityMap));
            _materialInstance.SetFloat(PID_Density, overrideDensity ? density : _originalMaterial.GetFloat(PID_Density));
            _materialInstance.SetFloat(PID_Dielectric, overrideDielectric ? dielectric : _originalMaterial.GetFloat(PID_Dielectric));
            _materialInstance.SetFloat(PID_RefractionIndex, overrideRefractionIndex ? refractionIndex : _originalMaterial.GetFloat(PID_RefractionIndex));
            _materialInstance.SetVector(PID_MainTex_ST, overrideTiling ? tiling : _originalMaterial.GetVector(PID_MainTex_ST));

            RT2DMaterialShaderSidecar.FixKeywords(_materialInstance);
            RT2DMaterialShaderSidecar.ComputeOptimizedProperties(_materialInstance);
        }

        #region Editor Stuff
#if UNITY_EDITOR
        private static readonly string _StandardSpriteMaterialPath = "Assets/Materials/RT2D_Sprite_Standard.mat";

        [MenuItem("GameObject/Ray Tracing 2D/Sprite")]
        private static void CreateSquareSprite()
        {
            var go = new GameObject("Sprite");
            go.AddComponent<RT2DSprite>();

            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(_StandardSpriteMaterialPath);

            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/test_sprite.png");
        }

        //void OnValidate()
        //{
        //    if (_materialInstance == null) return;

        //    smoothness = Mathf.Clamp(smoothness, 0, 1);
        //    density = Mathf.Clamp(density, 0, 1);
        //    dielectric = Mathf.Clamp(dielectric, 0, 1);
        //    refractionIndex = Mathf.Max(refractionIndex, 0.001f);

        //    //if(_renderer != null && _materialInstance != _renderer.sharedMaterial)


        //    if (_matProperties == null)
        //        _matProperties = new MaterialPropertyBlock();


        //    _matProperties.SetColor(PID_RenderColor, Color.white);

        //    //if (overrideAlbedo)
        //    //{
        //    //    _materialInstance.SetTexture(PID_MainTex, albedoMap);
        //    //    _materialInstance.SetColor(PID_RenderColor, albedo);
        //    //}
        //    //else
        //    //{
        //    //    _materialInstance.SetColor(PID_RenderColor, Color.white);
        //    //}

        //    if (overrideMask)
        //        _matProperties.SetTexture(PID_Mask, mask);

        //    if (overrideInteriorNormals)
        //        _matProperties.SetTexture(PID_InteriorNormalMap, interiorNormals);

        //    if (overrideOutscatteredLightEmissive)
        //    {
        //        _matProperties.SetTexture(PID_OutscatteredEmissiveMap, outscatteredEmissionMap);
        //        _matProperties.SetColor(PID_OutscatteredEmissiveColor, outscatteredEmissionEnergy);
        //    }

        //    if (overrideSmoothness)
        //    {
        //        _matProperties.SetTexture(PID_SmoothnessMap, smoothnessMap);
        //        _matProperties.SetFloat(PID_Smoothness, smoothness);
        //    }

        //    if(overrideDensity)
        //    {
        //        _matProperties.SetTexture(PID_DensityMap, densityMap);
        //        _matProperties.SetFloat(PID_Density, density);
        //    }

        //    if(overrideDielectric)
        //    {
        //        _matProperties.SetFloat(PID_Dielectric, dielectric);
        //    }

        //    if(overrideRefractionIndex)
        //    {
        //        _matProperties.SetFloat(PID_RefractionIndex, refractionIndex);
        //    }

        //    if(overrideTiling)
        //    {
        //        _matProperties.SetVector(PID_MainTex_ST, tiling);
        //    }


        //    _renderer.SetPropertyBlock(_matProperties);
        //}
#endif
#endregion
    }
}