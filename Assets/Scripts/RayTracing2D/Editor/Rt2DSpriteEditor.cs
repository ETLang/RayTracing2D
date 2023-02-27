using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RayTracing2D
{
    [CustomEditor(typeof(RT2DSprite))]
    public class RT2DSpriteEditor : Editor
    {
        SerializedProperty _propOverrideMask;
        SerializedProperty _propOverrideInteriorNormals;
        SerializedProperty _propOverrideOutscatteredEmission;
        SerializedProperty _propOverrideSmoothness;
        SerializedProperty _propOverrideDensity;
        SerializedProperty _propOverrideDielectric;
        SerializedProperty _propOverrideRefractionIndex;
        SerializedProperty _propOverrideTiling;

        SerializedProperty _propMask;
        SerializedProperty _propInteriorNormals;
        SerializedProperty _propOutscatteredEmissionMap;
        SerializedProperty _propOutscatteredEmissionEnergy;
        SerializedProperty _propSmoothnessMap;
        SerializedProperty _propSmoothness;
        SerializedProperty _propDensityMap;
        SerializedProperty _propDensity;
        SerializedProperty _propDielectric;
        SerializedProperty _propRefractionIndex;
        SerializedProperty _propTiling;

        private void OnEnable()
        {
            _propOverrideMask = serializedObject.FindProperty("overrideMask");
            _propOverrideInteriorNormals = serializedObject.FindProperty("overrideInteriorNormals");
            _propOverrideOutscatteredEmission = serializedObject.FindProperty("overrideOutscatteredLightEmissive");
            _propOverrideSmoothness = serializedObject.FindProperty("overrideSmoothness");
            _propOverrideDensity = serializedObject.FindProperty("overrideDensity");
            _propOverrideDielectric = serializedObject.FindProperty("overrideDielectric");
            _propOverrideRefractionIndex = serializedObject.FindProperty("overrideRefractionIndex");
            _propOverrideTiling = serializedObject.FindProperty("overrideTiling");

            _propMask = serializedObject.FindProperty("mask");
            _propInteriorNormals = serializedObject.FindProperty("interiorNormals");
            _propOutscatteredEmissionMap = serializedObject.FindProperty("outscatteredEmissionMap");
            _propOutscatteredEmissionEnergy = serializedObject.FindProperty("outscatteredEmissionEnergy");
            _propSmoothnessMap = serializedObject.FindProperty("smoothnessMap");
            _propSmoothness = serializedObject.FindProperty("smoothness");
            _propDensityMap = serializedObject.FindProperty("densityMap");
            _propDensity = serializedObject.FindProperty("density");
            _propDielectric = serializedObject.FindProperty("dielectric");
            _propRefractionIndex = serializedObject.FindProperty("refractionIndex");
            _propTiling = serializedObject.FindProperty("tiling");
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            _propOverrideMask.boolValue = EditorGUILayout.Toggle(new GUIContent("", "Override Mask"), _propOverrideMask.boolValue, GUILayout.Width(27));
            EditorGUILayout.PropertyField(_propMask);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _propOverrideInteriorNormals.boolValue = EditorGUILayout.Toggle(new GUIContent("", "Override Interior Normals"), _propOverrideInteriorNormals.boolValue, GUILayout.Width(27));
            EditorGUILayout.PropertyField(_propInteriorNormals);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _propOverrideOutscatteredEmission.boolValue = EditorGUILayout.Toggle(new GUIContent("", "Override Outscattered Emission"), _propOverrideOutscatteredEmission.boolValue, GUILayout.Width(27));
            EditorGUILayout.PropertyField(_propOutscatteredEmissionMap);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            _propOutscatteredEmissionEnergy.colorValue = EditorGUILayout.ColorField(new GUIContent("Outscattered Emission Energy"), _propOutscatteredEmissionEnergy.colorValue, true, false, true);
            //EditorGUILayout.PropertyField(_propOutscatteredEmissionEnergy);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _propOverrideSmoothness.boolValue = EditorGUILayout.Toggle(new GUIContent("", "Override Smoothness"), _propOverrideSmoothness.boolValue, GUILayout.Width(27));
            EditorGUILayout.PropertyField(_propSmoothnessMap);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            EditorGUILayout.PropertyField(_propSmoothness);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _propOverrideDensity.boolValue = EditorGUILayout.Toggle(new GUIContent("", "Override Density"), _propOverrideDensity.boolValue, GUILayout.Width(27));
            EditorGUILayout.PropertyField(_propDensityMap);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            EditorGUILayout.PropertyField(_propDensity);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _propOverrideDielectric.boolValue = EditorGUILayout.Toggle(new GUIContent("", "Override Dielectric"), _propOverrideDielectric.boolValue, GUILayout.Width(27));
            EditorGUILayout.PropertyField(_propDielectric);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _propOverrideRefractionIndex.boolValue = EditorGUILayout.Toggle(new GUIContent("", "Override Refraction Index"), _propOverrideRefractionIndex.boolValue, GUILayout.Width(27));
            EditorGUILayout.PropertyField(_propRefractionIndex);
            GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            //_propOverrideTiling.boolValue = EditorGUILayout.Toggle(_propOverrideTiling.boolValue, GUILayout.Width(27));
            //EditorGUILayout.PropertyField(_propTiling);
            //GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}