using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomRenderer : ScriptableRenderer
{
    public CustomRenderer(ScriptableRendererData data) : base(data) { }

    public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // Calls AddRenderPasses for each renderer feature added to this renderer
        AddRenderPasses(ref renderingData);

        // Tell the pipeline the default render pipeline texture to use. When a scriptable render pass doesn't
        // set a render target using ConfigureTarget these render textures will be bound as color and depth by default.
        ConfigureCameraTarget(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget);

        context.DrawSkybox(renderingData.cameraData.camera);
    }

    public override void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        base.SetupLights(context, ref renderingData);
    }

    public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData)
    {
        base.SetupCullingParameters(ref cullingParameters, ref cameraData);
    }

    public override void FinishRendering(CommandBuffer cmd)
    {
        base.FinishRendering(cmd);
    }
}