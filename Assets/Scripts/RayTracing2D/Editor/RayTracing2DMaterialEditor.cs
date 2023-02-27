using RayTracing2D;
using System;
using UnityEditor;
using UnityEngine;

public class RayTracing2DMaterialEditor : MaterialEditor
{
    MaterialProperty _propMainTex;
    MaterialProperty _propColor;
    MaterialProperty _propMainTex_ST;
    MaterialProperty _propMask;
    MaterialProperty _propInteriorNormalMap;

    MaterialProperty _propOutscatteredEmissiveMap;
    MaterialProperty _propOutscatteredEmissiveColor;
    MaterialProperty _propOutscatteredEmissiveIntensity;
    MaterialProperty _propOutscatteredEmissiveMap_ST;

    MaterialProperty _propSmoothnessMap;
    MaterialProperty _propSmoothness;

    MaterialProperty _propDensityMap;
    MaterialProperty _propDensity;

    MaterialProperty _propDielectric;
    MaterialProperty _propRefractionIndex;


    public override void OnEnable()
    {
        base.OnEnable();

        var mats = new UnityEngine.Object[] { target };

        _propMainTex = GetMaterialProperty(mats, "_MainTex");
        //_propColor = GetMaterialProperty(mats, "_Color");
        _propMainTex_ST = GetMaterialProperty(mats, "_MainTex_ST");
        _propMask = GetMaterialProperty(mats, "_MaskTex");
        _propInteriorNormalMap = GetMaterialProperty(mats, "_InteriorNormalMap");

        _propOutscatteredEmissiveMap = GetMaterialProperty(mats, "_OutscatteredEmissiveMap");
        _propOutscatteredEmissiveColor = GetMaterialProperty(mats, "_OutscatteredEmissiveColor");
        _propOutscatteredEmissiveIntensity = GetMaterialProperty(mats, "_OutscatteredEmissiveIntensity");
        _propOutscatteredEmissiveMap_ST = GetMaterialProperty(mats, "_OutscatteredEmissiveMap_ST");

        _propSmoothness = GetMaterialProperty(mats, "_Smoothness");
        _propSmoothnessMap = GetMaterialProperty(mats, "_SmoothnessMap");

        _propDensity = GetMaterialProperty(mats, "_Density");
        _propDensityMap = GetMaterialProperty(mats, "_DensityMap");

        _propDielectric = GetMaterialProperty(mats, "_Dielectric");
        _propRefractionIndex = GetMaterialProperty(mats, "_RefractionIndex");
    }

    public override void OnInspectorGUI()
    {
        if (!isVisible)
            return;

        var targetMat = (Material)target;

        EditorGUI.BeginChangeCheck();
        {
            TexturePropertySingleLine(new GUIContent("Albedo"), _propMainTex);
            TexturePropertySingleLine(new GUIContent("Mask"), _propMask);
            TexturePropertySingleLine(new GUIContent("Interior Normal Map"), _propInteriorNormalMap);
            TexturePropertySingleLine(new GUIContent("Outscattered Light"), _propOutscatteredEmissiveMap, _propOutscatteredEmissiveColor);

            TexturePropertySingleLine(new GUIContent("Smoothness"), _propSmoothnessMap, _propSmoothness);
            TexturePropertySingleLine(new GUIContent("Density"), _propDensityMap, _propDensity);
            RangeProperty(_propDielectric, "Dielectric");
            FloatProperty(_propRefractionIndex, "Refraction Index");

            GUILayout.Space(10);
            TextureScaleOffsetProperty(_propMainTex_ST);

            GUILayout.Space(25);
            FloatProperty(GetMaterialProperty(new UnityEngine.Object[] { target }, "_ObjectIndex"), "Object Index");
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);

            RT2DMaterialShaderSidecar.FixKeywords(targetMat);
            RT2DMaterialShaderSidecar.ComputeOptimizedProperties(targetMat);
        }
    }

    public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
    {
        // TODO

        GUI.Label(r, "HAHAHA");

        //base.OnInteractivePreviewGUI(r, background);
    }
}