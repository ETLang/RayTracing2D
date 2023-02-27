using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RayTracing2D
{
    public interface ILightEmitter
    {
        bool IsLit { get; }
        bool IsStale { get; }
        int Segments { get; }
        int Emit(NativeArray<EmittedRay> rays, int startIndex, int requestedCount);
    }

    public interface ISprite
    {
        int RenderId { get; set; }
    }

    public interface ITrainingSample
    {
        string Name { get; }
        int Index { get; set; }
    }
    
    public interface ITrainer
    {
        IEnumerable<ITrainingSample> Samples { get; }
        void OnTrainingSampleStarting(RT2DRenderer renderer);
        void OnTrainingInputRendered();
        void OnTrainingOutputRendered();
    }

    public partial class RT2DRenderer : Disposable
    {
        #region Diagnostic Properties

        public bool D_Enable { get; set; }
        public bool D_PauseTracing { get; set; }
        public float D_SelectMip { get; set; }
        public float D_VarianceMipMax { get; set; }

        #endregion

        #region Training

        public static ITrainer Trainer { get; set; }
        public static bool TrainingMode => Trainer != null;

        public int PassCount { get; set; } = 1;
        public double TrainingDetail { get; set; } = 100;
        public double FrameDeltaBias { get; set; } = 0.1;
        public double FrameDelta { get; private set; } = 1;
        public double ConvergenceThreshold { get; set; } = 0.001;
        public int TrainingWidth { get; set; } = 1024;
        public int TrainingHeight { get; set; } = 1024;

        public RenderTexture TrainingTarget
        {
            get
            {
                if (TrainingMode)
                {
                    _context.Submit();
                    return _trainingTarget ?? 
                        SetupTarget(
                            ref _trainingTarget, 
                            _outScatter.descriptor.WithEnableRandomWrite(false), 
                            "TrainingTarget");
                }
                else
                    return null;
            }
        }

        #endregion

        static readonly string _profileName = "Ray Tracing 2D";

        static ShaderTagId ProperShaderTagId = new ShaderTagId("Universal2D");
        static readonly int PID_OutScatter = Shader.PropertyToID("OutScatter");
        static readonly int PID_OutScatterBuffer = Shader.PropertyToID("OutScatterBuffer");
        static readonly int PID_OutScatterBufferWidth = Shader.PropertyToID("OutScatterBufferWidth");
        static readonly int PID_GBuffer0 = Shader.PropertyToID("GBuffer0");
        static readonly int PID_GBuffer1 = Shader.PropertyToID("GBuffer1");
        static readonly int PID_RayBuffer = Shader.PropertyToID("RayBuffer");
        static readonly int PID_WorldToOutScatterScreen = Shader.PropertyToID("WorldToOutScatterScreen");
        static readonly int PID_OutScatterScreenToPixel = Shader.PropertyToID("OutScatterScreenToPixel");

        static readonly string KernelName_Trace = "Kernel_Trace";
        static readonly string KernelName_ConvertOutscatter = "Kernel_ConvertOutscatter";

        static HashSet<ILightEmitter> _lightsInNextFrame = new HashSet<ILightEmitter>();
        public static void RegisterLight(ILightEmitter light) { if (!_lightsInNextFrame.Contains(light)) _lightsInNextFrame.Add(light); }

        public ComputeShader RayTracer { get; set; }

        int _kernelIdTrace = -1;
        int _kernelIdConvertOutscatter = -1;

        long _totalRays;
        long _passRays;

        ComputeBuffer _rayBuffer;
        //ComputeBuffer _outScatterBuffer;
        RenderTexture _outScatterBuffer2;
        Camera _camera;
        CommandBuffer _cmd;
        ScriptableRenderContext _context;
        ScriptableCullingParameters _cullingData;
        CullingResults _cullingResults;

        RenderTexture _gBufferLight;
        RenderTexture _gBufferMaterial;
        RenderTexture _gBufferStructure;
        RenderTargetIdentifier[] _gBufferTarget;

        RenderTexture _outScatter;
        RenderTexture _outScatterVariance;
        RenderTexture _trainingTarget;

        Mesh _blitQuad;

        Ops.BlitCopy _opCopy;
        Ops.BlitAdd _opAdd;
        Ops.BlitBlend _opBlend;
        Ops.BlitModulate _opModulate;
        Ops.BlitDiagnostic _opDiagnostic;
        Ops.Gauss4Sample _opGauss4Sample;
        Ops.Gauss9Sample _opGauss9Sample;
        Ops.Gauss10x10Bilinear _opGauss10x10Bilinear;
        Ops.IntegratePointCloud _opIntegratePointCloud;
        Ops.LogOfIntensity _opLogOfIntensity;
        Ops.VarianceMipMapGenerator _opVarianceMipMapGenerator;

        #region Lifetime Management

        public RT2DRenderer()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += OnUpdate;
            UnityEditor.EditorApplication.pauseStateChanged += OnPauseStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        protected override void OnDispose(bool disposing)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= OnUpdate;
#endif
            SafeDispose(ref _rayBuffer);
            //SafeDispose(ref _outScatterBuffer);
            SafeDispose(ref _cmd);

            SafeDispose(ref _gBufferMaterial);
            SafeDispose(ref _gBufferStructure);
            SafeDispose(ref _outScatter);
            SafeDispose(ref _outScatterBuffer2);
            SafeDispose(ref _outScatterVariance);

            SafeDispose(ref _trainingTarget);
        }

#if UNITY_EDITOR
        void OnUpdate()
        {
        }

        void OnPauseStateChanged(UnityEditor.PauseState pauseState)
        {
            switch (pauseState)
            {
                case UnityEditor.PauseState.Paused:

                    break;
                case UnityEditor.PauseState.Unpaused:
                    break;
            }
        }

        void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange obj)
        {
            switch(obj)
            {
                case UnityEditor.PlayModeStateChange.EnteredEditMode:
                    break;
                case UnityEditor.PlayModeStateChange.EnteredPlayMode:
                    break;
                case UnityEditor.PlayModeStateChange.ExitingEditMode:
                    break;
                case UnityEditor.PlayModeStateChange.ExitingPlayMode:
                    Trainer = null;
                    break;
            }
        }
#endif

        #endregion

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var paused = D_PauseTracing;

            if (IsDisposed)
                return;

#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isCompiling)
                return;

            paused = paused || UnityEditor.EditorApplication.isPaused;

            if (TrainingMode && !paused && PassCount == 1)
                Trainer.OnTrainingSampleStarting(this);

            if (!TrainingMode)
                ResetTrainingProperties();
#endif

            _context = context;

            PrepareForSceneWindow();

            if (!SetupRayTracing())
                return;

            /* 2D Ray Tracing Problems
             * 
             * 1. Normal Computation
             * 
             * Normal maps should be acceptable, but also normals should be able to be generated from the 2D geometry.
             * 
             * Interpretation of normals is tricky, since the light propagation is 2D.
             * 
             * 2. Material Properties
             * 3. Radiance Function
             * 
             * Incoming light can be:
             * - absorbed
             * - transmitted/refracted
             * - reflected
             * - upscattered
             * - downscattered
             * 
             * Total energy of outgoing and absorbed light must equal incoming light.
             * There are two or three types of interactions: Surface, Substrate, and possibly Terrain
             * 
             * Surface Interaction:
             *  - Ray can be reflected, refracted, or absorbed.
             *  - alpha := absorption factor = (1 - color)
             *  - R := reflectance
             *  - M := Metallicness
             *  - S := smoothness = 1 - roughness
             *  - I := index of refraction
             *  - P := Quantum Density
             *  - N := Normal
             *  Analogous to familiar BRDF
             *  
             *  Substrate Interaction:
             *  - Substrate is a probability distribution function
             *  - Ray can be scattered or absorbed
             *  - alpha := absorption factor = (1 - color)
             *  - Probability of Scatter = integrate_over_space(density => probability)
             *  - Out.Color = Ray.Color * alpha
             *  
             *  Terrain Interaction:
             *  - Terrain is a probability distrubtion function derived from the normal and other material properties
             *  - Same as substrate interaction, with probability of collision over space.
             *  - Probability of collision = integrate_over_space(k * (N dot Ray))
             *   
             *  Probability Density Function:
             *  - Float texture. Stores probability of transmission.
             *  - Full mipchain. Downsample 4 pixels (a,b,c,d) = sqrt(a*b*c*d);
             * 
             * 4. Tracing Algorithm
             * 
             * Trace(Ray r, float uOutScatter, float2 outScatterDirection)
             * 1. uSurface = FindSurfaceInteraction(r, surfaces)
             * 2. uScatter = FindScatterInteraction(r, probabilityDensity)
             * 3. o.Origin = r.Origin + r.Direction * min(uSurface, uScatter)
             * 4. mat = GetMaterialData(o.Origin)
             * 5. if(uSurface < uScatter)
             * 6.   (o.Direction, o.Energy, o.UpScatter, o.DownScatter) = SurfaceScatter(o.Origin, r.Direction, mat)
             * 7. else
             * 8.   (o.Direction, o.Energy, o.UpScatter, o.DownScatter) = SubstrateScatter(o.Origin, r.Direction, mat)
             * 9. WriteAdd(upScatterTarget, o.Origin, o.UpScatter)
             * 10. WriteAdd(downScatterTarget, o.Origin, o.DownScatter)
             * 11. Trace(o)  // until done
             * 
             * FindSurfaceInteraction(r, surfaces)
             * 1.
             * 
             * FincScatterInteraction(r, probabilityDensity)
             * 0. mipLevel = maxMipLevel
             * 0. mipSize = 1 << mipLevel
             * 1. u = rand(0,1)
             * 2. delta = r.Direction * mipSize
             * 3. transmitChance = Read(probabilityDensity, r.Origin + delta / 2, mipLevel)
             * 4. if(u > transmitChance) // Interaction detected somewhere within delta
             * 5.   scatterPoint = r.Origin + delta * u / (1 - transmitChance)
             * 6.   ....  (binary search probability density function)
             * 
             * RecurseFindScatterPoint(
             * 
             * SurfaceScatter(incidentPoint, incidentDir, mat)
             * 1.
             * 
             * SubstrateScatter(incidentPoint, incidentDir, mat)
             * 1. 
             * 
             * 
             * Frame Render Implementation:
             * 
             * 1. Render G buffer: Scatter Color, material index, Normal, kDensity, Boundary presence
             * 2. Downsample normal, kDensity, boundary
             * 2. Write material data to structured buffer
             * 3. Generate rays from light sources. Insert into structured buffer.
             * 4. Run compute shader to propagate rays
             * 5. Tone map outscattered energy
             * 6. Display!
             * 
             * 
             * Standup process:
             * 
             * A ----
             * 1. Render scatter color to G buffer
             * 2. Blit scatter color output
             * 
             * 
             * B ----
             * 1. Render scatter color, material index, kDensity, and boundary presence to G buffer
             * 2. Debug-Render material index, kDensity, and boundary presence to color output
             * 
             * C ----
             * 1. Render full G buffer
             * 2. Debug-Render normals
             * 
             * D ----
             * 1. Render full G buffer
             * 2. Generate rays from light sources. Insert into structured buffer.
             * 3. Run compute shader to paint rays directly to final output
             * 
             * E ----
             * 1. Render full G buffer
             * 2. Downsample N/D/B
             * 2. Write downsamples to debug output
             * 
             * F ----
             * 1. Render G buffer
             * 2. Downsample
             * 3. Generate rays
             * 4. Propagate rays with substrate scattering
             * 5. Blit outscatter directly to output
             * 
             * G --- Add tone mapping
             * 
             * H --- Add boundary collision with full absorption (no reflection)
             * I --- Add reflection to boundary collision
             * J --- Add refraction to boundary collision
             */

            CreateTargets();

            _cmd.BeginSample(_profileName);

            if (PassCount < 2 || !TrainingMode)
            {
                ClearAndSetupTargets();
                RenderGBuffer();
            }

            if (paused)
            {
                ComposeLayers();
                ToneMapToOutputFrame();
            }
            else
            {
                var rayCount = InitializeRays();
                TraceRays(rayCount);

                if (!TrainingMode)
                    IntegrateRays();

                ComposeLayers();
                ToneMapToOutputFrame();

                if (TrainingMode)
                {
                    CompleteRayTracing();

                    if (PassCount == 1)
                    {
                        _context.Submit();
                        Trainer.OnTrainingInputRendered();
                    }
                    else
                    {
                        FrameDelta = FrameDelta * (1 - FrameDeltaBias) + MeasureFrameDelta() * FrameDeltaBias;
                    }

                    if (FrameDelta < ConvergenceThreshold)
                    {
                        _context.Submit();
                        Trainer.OnTrainingOutputRendered();
                        PassCount = 1;
                        FrameDelta = 1;
                    }
                    else
                        PassCount++;
                }
                else
                {
                    PassCount = 1;
                    FrameDelta = 1;
                    DrawUnsupportedShaders();
                    CompleteRayTracing();
                }
            }

            _cmd.EndSample(_profileName);
        }

        bool SetupRayTracing()
        {
            if (!_camera.TryGetCullingParameters(out _cullingData))
                return false;

            _cullingResults = _context.Cull(ref _cullingData);
            ExecuteBuffer();

            _context.SetupCameraProperties(_camera);

            _rayBuffer = _rayBuffer ?? new ComputeBuffer(10000000, 32, ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);

            if (_opCopy == null) _opCopy = new Ops.BlitCopy(_cmd);
            if (_opAdd == null) _opAdd = new Ops.BlitAdd(_cmd);
            if (_opBlend == null) _opBlend = new Ops.BlitBlend(_cmd);
            if (_opModulate == null) _opModulate = new Ops.BlitModulate(_cmd);
            if (_opDiagnostic == null) _opDiagnostic = new Ops.BlitDiagnostic(_cmd);
            if (_opGauss4Sample == null) _opGauss4Sample = new Ops.Gauss4Sample(_cmd);
            if (_opGauss9Sample == null) _opGauss9Sample = new Ops.Gauss9Sample(_cmd);
            if (_opGauss10x10Bilinear == null) _opGauss10x10Bilinear = new Ops.Gauss10x10Bilinear(_cmd);
            if (_opIntegratePointCloud == null) _opIntegratePointCloud = new Ops.IntegratePointCloud(_cmd);
            if (_opLogOfIntensity == null) _opLogOfIntensity = new Ops.LogOfIntensity(_cmd);
            if (_opVarianceMipMapGenerator == null) _opVarianceMipMapGenerator = new Ops.VarianceMipMapGenerator(_cmd);

            if (!_blitQuad)
            {
                _blitQuad = new Mesh();
                _blitQuad.SetVertices(new Vector3[]
                {
                new Vector3(-1,1,0),
                new Vector3(1,1,0),
                new Vector3(1,-1,0),
                new Vector3(-1,-1,0)
                });

                _blitQuad.SetUVs(0, new Vector2[]
                {
                new Vector2(0,1),
                new Vector2(1,1),
                new Vector2(1,0),
                new Vector2(0,0)
                });

                _blitQuad.SetIndices(new int[] { 0, 1, 2, 0, 2, 3 }, MeshTopology.Triangles, 0);
            }

            if (RayTracer.HasKernel(KernelName_Trace))
                _kernelIdTrace = RayTracer.FindKernel(KernelName_Trace);

            if (RayTracer.HasKernel(KernelName_ConvertOutscatter))
                _kernelIdConvertOutscatter = RayTracer.FindKernel(KernelName_ConvertOutscatter);

            return true;
        }

        RenderTexture SetupTarget(ref RenderTexture target, RenderTextureDescriptor desc, string name)
        {
            if (target) target.Release();
            target = new RenderTexture(desc);
            target.name = name;
            return target;
        }

        void CreateTargets()
        {
            var targetWidth = TrainingMode ? TrainingWidth : _camera.pixelWidth;
            var targetHeight = TrainingMode ? TrainingHeight : _camera.pixelHeight;

            if (_gBufferMaterial != null && _gBufferMaterial.width == targetWidth && _gBufferMaterial.height == targetHeight)
                return;

            int potWidth, potHeight;
            for (potWidth = 1; potWidth < targetWidth; potWidth *= 2) ;
            for (potHeight = 1; potHeight < targetHeight; potHeight *= 2) ;

            SafeDispose(ref _trainingTarget);

            // BUGBUG
            //if (_outScatterBuffer != null) _outScatterBuffer.Release();
            //_outScatterBuffer = new ComputeBuffer(potWidth * potHeight, 16, ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);

            var gBufferDescriptor = new RenderTextureDescriptor(targetWidth, targetHeight)
            {
                depthBufferBits = 0,
                mipCount = 1
            };

            var outScatterDescriptor = new RenderTextureDescriptor(potWidth, potHeight)
            {
                depthBufferBits = 0,
                colorFormat = RenderTextureFormat.ARGBFloat,
                mipCount = 0,
                useMipMap = true,
                autoGenerateMips = false,
                enableRandomWrite = true
            };

            var outScatterBufferDesc = new RenderTextureDescriptor(potWidth * 4, potHeight)
            {
                depthBufferBits = 0,
                graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_UInt,
                mipCount = 1,
                enableRandomWrite = true
            };

            SetupTarget(ref _gBufferLight, gBufferDescriptor.WithFormat(RenderTextureFormat.ARGBFloat), "G Buffer Light");
            SetupTarget(ref _gBufferMaterial, gBufferDescriptor.WithFormat(RenderTextureFormat.BGRA32), "G Buffer Material");
            SetupTarget(ref _gBufferStructure, gBufferDescriptor.WithFormat(RenderTextureFormat.ARGBHalf), "G Buffer Structure");
            SetupTarget(ref _outScatterBuffer2, outScatterBufferDesc, "Outscatter RW Buffer");
            SetupTarget(ref _outScatter, outScatterDescriptor, "Outscatter POT");
            SetupTarget(ref _outScatterVariance, outScatterDescriptor.WithEnableRandomWrite(false), "Outscatter Variance POT");

            _gBufferTarget = new RenderTargetIdentifier[] 
            {
                _gBufferLight, 
                _gBufferMaterial, 
                _gBufferStructure 
            };
        }

        void ClearAndSetupTargets()
        {
            _cmd.SetRenderTarget(_outScatter);
            _cmd.ClearRenderTarget(true, true, Color.clear);
            //_cmd.SetRenderTarget(_outScatterScratch);
            //_cmd.ClearRenderTarget(true, true, Color.clear);
            _cmd.SetRenderTarget(_outScatterBuffer2);
            _cmd.ClearRenderTarget(false, true, Color.clear);

            _cmd.SetRenderTarget(_gBufferLight);
            _cmd.ClearRenderTarget(false, true, Color.clear);

            _cmd.SetRenderTarget(_gBufferStructure);
            _cmd.ClearRenderTarget(false, true, Color.clear);

            _cmd.SetRenderTarget(_gBufferMaterial, _gBufferMaterial.depthBuffer);
            _cmd.ClearRenderTarget(true, true, _camera.backgroundColor);

            _cmd.SetRenderTarget(_gBufferTarget, _gBufferMaterial.depthBuffer);

            ExecuteBuffer();
        }

        void RenderGBuffer()
        {
            /* G buffer is three render targets:
             * 
             * A-
             * Float32 [Outscatter light, ??]
             * 
             * B-
             * UNORM8 [Albedo Color, Material Index]
             * 
             * C-
             * FLOAT16 [Normal XY, kDensity, Boundary]
             * 
             * C generates a mipchain.
             */

            var sortingSettings = new SortingSettings(_camera) { criteria = SortingCriteria.SortingLayer };
            var drawingSettings = new DrawingSettings(ProperShaderTagId, sortingSettings);
            var filteringSettings = new FilteringSettings(RenderQueueRange.all);
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
            ExecuteBuffer();
        }

        int InitializeRays()
        {
            int desiredPhotons = 0;
            var toRemove = new List<ILightEmitter>();

            foreach (var light in _lightsInNextFrame)
            {
                if (light.IsStale)
                    toRemove.Add(light);
                else if (light.IsLit)
                    desiredPhotons += PhotonsOf(light);
            }

            foreach (var oldLight in toRemove)
                _lightsInNextFrame.Remove(oldLight);

            var rays = _rayBuffer.BeginWrite<EmittedRay>(0, desiredPhotons);

            int raysWritten = 0;
            foreach (var light in _lightsInNextFrame)
            {
                if (light.IsLit)
                    raysWritten += light.Emit(rays, raysWritten, PhotonsOf(light));
            }

            _rayBuffer.EndWrite<EmittedRay>(raysWritten);

            return raysWritten;
        }

        void TraceRays(int rayCount)
        {
            if (rayCount == 0) return;

            var screenMatrix = _camera.projectionMatrix * _camera.transform.worldToLocalMatrix;
            var pixelMatrix = Matrix4x4.Scale(new Vector3(_outScatter.width / 2, _outScatter.height / 2, 1)) * Matrix4x4.Translate(new Vector3(_outScatter.width / 2, _outScatter.height / 2, 0));

            if (_kernelIdTrace != -1)
            {
                RayTracer.SetTexture(_kernelIdTrace, PID_OutScatter, _outScatter);
                RayTracer.SetTexture(_kernelIdTrace, PID_GBuffer0, _gBufferMaterial);
                RayTracer.SetTexture(_kernelIdTrace, PID_GBuffer1, _gBufferStructure);
                RayTracer.SetBuffer(_kernelIdTrace, PID_RayBuffer, _rayBuffer);
                RayTracer.SetTexture(_kernelIdTrace, PID_OutScatterBuffer, _outScatterBuffer2); // BUGBUG Should be _outScatterBuffer but stupid Unity can't make a UAV for it.

                RayTracer.SetTexture(_kernelIdConvertOutscatter, PID_OutScatter, _outScatter);
                RayTracer.SetTexture(_kernelIdConvertOutscatter, PID_OutScatterBuffer, _outScatterBuffer2); // BUGBUG

                RayTracer.SetInt(PID_OutScatterBufferWidth, _outScatter.width);
                RayTracer.SetMatrix(PID_WorldToOutScatterScreen, screenMatrix);
                RayTracer.SetMatrix(PID_OutScatterScreenToPixel, pixelMatrix);
            }

            _context.Submit();
            _context.SetupCameraProperties(_camera);

            RunKernel_Trace(rayCount);
            RunKernel_ConvertOutscatter();
            _totalRays += rayCount;
        }

        void IntegrateRays()
        {
            ExecuteBuffer();

            GenerateMipsGaussianRayDistribution(_outScatter);
            //GenerateVarianceLODs(_outScatter, _outScatterVariance, VarianceSource.W, Mathf.CeilToInt(D_VarianceMipMax));
        }

        void ComposeLayers()
        {
        }

        void ToneMapToOutputFrame()
        {
            if (D_Enable)
            {
                _opDiagnostic.VarianceMipMax = D_VarianceMipMax;
                _opDiagnostic.Paint(_outScatter, D_SelectMip, BuiltinRenderTextureType.CameraTarget, 0);
            }
            else
            {
                // L0 := Log of pixel intensity
                // L := Downsample(L0, Max)
                // LIris := Readback(L[Max])
                // Exposure ~T~ LIris
                // LChem := Saturate(Blur(L[LBLOD]) - Exposure)
                // ToneOut = _outScatter / (Exp(LIris) * Exp(LChem)) = In / Exp(Exposure + LChem)

                if (TrainingMode)
                {
                    // Blit mip 0 of outscatter to measurable training frame.
                    if (PassCount == 1)
                        _opCopy.Paint(_outScatter, 0, TrainingTarget, 0);
                    else
                    {
                        float mod = (float)(1.0 / (TrainingDetail * (PassCount - 1)));
                        _opModulate.Color = new Color(mod, mod, mod);
                        _opModulate.Paint(_outScatter, 0, TrainingTarget, 0);
                    }

                    _opCopy.Paint(TrainingTarget, _gBufferLight);
                }
                else
                    _opIntegratePointCloud.Paint(_outScatter, _gBufferLight);

                _opCopy.Paint(_gBufferMaterial, BuiltinRenderTextureType.CameraTarget);
                _opAdd.Paint(_gBufferLight, BuiltinRenderTextureType.CameraTarget);
            }
        }

        void CompleteRayTracing()
        {
            ExecuteBuffer();
        }

        #region Operations

        void RunKernel_Trace(int rayCount)
        {
            if (_kernelIdTrace != -1)
                RayTracer.Dispatch(_kernelIdTrace, rayCount / 256, 1, 1);
        }

        void RunKernel_ConvertOutscatter()
        {
            if (_kernelIdConvertOutscatter != -1)
                RayTracer.Dispatch(_kernelIdConvertOutscatter, _outScatter.width / 32, _outScatter.height / 32, 1);
        }

        double MeasureFrameDelta()
        {
            // TODO

            // Measure the difference between outScatter and potScratch
            return 1.0 / PassCount;
        }

        #endregion

        #region Utilities

        void ExecuteBuffer()
        {
            _context.ExecuteCommandBuffer(_cmd);
            _cmd.Clear();
        }

        void SafeDispose(ref RenderTexture target)
        {
            if (target)
            {
                target.Release();
                target = null;
            }
        }

        void SafeDispose<T>(ref T thing) where T : class, IDisposable
        {
            thing?.Dispose();
            thing = null;
        }

        void ResetTrainingProperties()
        {
            FrameDeltaBias = 0.1f;
            ConvergenceThreshold = 1;
            TrainingWidth = 1024;
            TrainingHeight = 1024;
            FrameDelta = 1;
        }

        int PhotonsOf(ILightEmitter light)
        {
            return (int)(light.Segments * (TrainingMode ? (PassCount == 1) ? 1 : TrainingDetail : 1));
        }

        void GenerateMipsGaussian(RenderTexture image, RenderTexture scratch)
        {
            _cmd.BeginSample("Gaussian Mip Gen");
            for (int i = 0; i < image.mipmapCount - 1; i++)
            {
                _opGauss4Sample.Paint(image, i, scratch, i);
                _opGauss9Sample.Paint(scratch, i, image, i + 1);
            }
            _cmd.EndSample("Gaussian Mip Gen");
        }

        void GenerateMipsGaussianRayDistribution(RenderTexture image)
        {
            _cmd.BeginSample("Gaussian Mip Gen - Ray Distribution");

            var scratch = RenderTexture.GetTemporary(image.descriptor);
            for (int i = 0; i < image.mipmapCount - 1; i++)
            {
                //Blit(image, i, scratch, i);
                //BlitGauss10x10Bilinear(scratch, i, image, i + 1);
                _opGauss4Sample.Paint(image, i, scratch, i);
                _opGauss9Sample.Paint(scratch, i, image, i + 1);
            }
            RenderTexture.ReleaseTemporary(scratch);

            _cmd.EndSample("Gaussian Mip Gen - Ray Distribution");
        }

        void GenerateVarianceLODs(RenderTexture source, RenderTexture outVariance, Ops.VarianceSource dataSource, int mipMax = -1)
        {
            _cmd.BeginSample("Variance Mip Gen");

            int i = 0;

            if (mipMax == -1 || mipMax > source.mipmapCount)
                mipMax = source.mipmapCount;

            //for(int i = 0;i < mipMax;i++)
            //    BlitVariance

            if (mipMax >= 1)
            {
                _opVarianceMipMapGenerator.VarianceSource = dataSource;
                _opVarianceMipMapGenerator.Paint(source, 0, outVariance, 1);
            }

            var scratch = RenderTexture.GetTemporary(outVariance.descriptor);
            for (i = 1; i < mipMax - 1; i++)
            {
                _opVarianceMipMapGenerator.VarianceSource = Ops.VarianceSource.Accumulate;
                _opVarianceMipMapGenerator.Paint(outVariance, i, scratch, i + 1);
                _opCopy.Paint(scratch, i + 1, outVariance, i + 1);
            }
            RenderTexture.ReleaseTemporary(scratch);

            _cmd.EndSample("Variance Mip Gen");
        }

        #endregion

        #region Supporting Crap

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            _camera = renderingData.cameraData.camera;
            _cmd = cmd;
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        #endregion
    }

    public struct EmittedRay
    {
        public float2 Position;
        public float2 Direction;
        public float4 Energy;
    }
}