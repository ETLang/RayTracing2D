using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RayTracing2D
{
    public partial class RT2DRenderer
    {
        partial void DrawUnsupportedShaders();
        partial void PrepareForSceneWindow();

#if UNITY_EDITOR

        static ShaderTagId[] LegacyShaderTagIds =
            {
            new ShaderTagId("FORWARD"),
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM")
        };

        Material _errorMaterial;

        partial void DrawUnsupportedShaders()
        {
            _errorMaterial = _errorMaterial ?? new Material(Shader.Find("Hidden/InternalErrorShader"));

            var drawingSettings = new DrawingSettings(LegacyShaderTagIds[0], new SortingSettings(_camera))
            {
                overrideMaterial = _errorMaterial
            };

            for (int i = 1; i < LegacyShaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, LegacyShaderTagIds[i]);
            }

            var filteringSettings = FilteringSettings.defaultValue;
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        }

        partial void PrepareForSceneWindow()
        {
            if (_camera.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
            }
        }

#endif
    }
}