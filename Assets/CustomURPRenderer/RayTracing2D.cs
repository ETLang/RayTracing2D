using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RayTracing2D.Feature
{
    public class RayTracing2D : ScriptableRendererFeature
    {
        public ComputeShader rayTracer;

        [Header("Diagnostics Settings")]
        public bool enableDiagnostics;

        public bool pauseTracing;

        [Range(0,10)]
        public float selectMip;

        [Range(1, 10)]
        public float varianceMipMax = 7;

        class RayTracing2DRenderPass : ScriptableRenderPass, IDisposable
        {
            RT2DRenderer _renderer = new RT2DRenderer();
            RayTracing2D _feature;
            bool _disposed;

            public RayTracing2DRenderPass(RayTracing2D feature)
            {
                _feature = feature;
            }

            ~RayTracing2DRenderPass()
            {
                Dispose(false);
            }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                _renderer.Execute(context, ref renderingData);
            }

            #region Supporting Crap

            // This method is called before executing the render pass.
            // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
            // When empty this render pass will render to the active camera render target.
            // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
            // The render pipeline will ensure target setup and clearing happens in a performant manner.
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                _renderer.RayTracer = _feature.rayTracer;
                _renderer.D_Enable = _feature.enableDiagnostics;
                _renderer.D_PauseTracing = _feature.pauseTracing;
                _renderer.D_SelectMip = _feature.selectMip;
                _renderer.D_VarianceMipMax = _feature.varianceMipMax;
                _renderer.OnCameraSetup(cmd, ref renderingData);
            }

            // Cleanup any allocated resources that were created during the execution of this render pass.
            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                _renderer.OnCameraCleanup(cmd);
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                GC.SuppressFinalize(this);
                Dispose(true);
            }

            protected virtual void Dispose(bool disposing)
            {
                _renderer.Dispose();
            }

            #endregion
        }

        RayTracing2DRenderPass m_ScriptablePass;

        /// <inheritdoc/>
        public override void Create()
        {
            if (m_ScriptablePass == null)
            {
                m_ScriptablePass = new RayTracing2DRenderPass(this);

                // Configures where the render pass should be injected.
                m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            }
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_ScriptablePass);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (m_ScriptablePass != null)
            {
                m_ScriptablePass.Dispose();
                m_ScriptablePass = null;
            }
        }

        void Reset()
        {
        }
    }
}