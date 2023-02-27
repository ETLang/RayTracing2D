using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using TestSimpleRNG;
#if UNITY_EDITOR
using UnityEditor;
#endif
using random = Unity.Mathematics.Random;


namespace RayTracing2D
{
    [ExecuteInEditMode]
    public class PointLightRT2D : MonoBehaviour, ILightEmitter
    {
        public Color color = Color.white;
        public float intensity = 1.0f;
        public float radius = 0.1f;
        public float innerAngle = 360;
        public float outerAngle = 360;

        void Start()
        {
            RT2DRenderer.RegisterLight(this);

            EditorStart();
        }

        void OnDestroy()
        {
            Cleanup();
            EditorDestroy();
        }

        void OnEnable()
        {
            RT2DRenderer.RegisterLight(this);
        }

        void Update()
        {
            RT2DRenderer.RegisterLight(this);
        }

        void Cleanup()
        {
        }

        #region IRayTracing2DLightEmitter

        bool ILightEmitter.IsStale => !this || !gameObject.activeSelf;
        bool ILightEmitter.IsLit => isActiveAndEnabled;

        public int Segments => 10000;
        public int TrainingPhotons => 1000000;

        int ILightEmitter.Emit(NativeArray<EmittedRay> rays, int startIndex, int requestedCount)
        {
            var colorEnergy = color * (float)(radius * radius * intensity * 1000.0 / Segments);

            var job = new EmitterJob
            {
                Rays = rays,
                StartIndex = startIndex,
                EndIndex = startIndex + requestedCount,
                PhotonEnergy = new float4(colorEnergy.r, colorEnergy.g, colorEnergy.b, 1),
                Center = (Vector2)transform.position,
                Radius = radius,
                Time = Time.time,
                Seed = (uint)System.DateTimeOffset.Now.Ticks,
            };

            job.Schedule(32, 1).Complete();

            return requestedCount;
        }

        #endregion

        [BurstCompile]
        private struct EmitterJob : IJobParallelFor
        {
            [NativeDisableParallelForRestriction]
            public NativeArray<EmittedRay> Rays;

            public int StartIndex;
            public int EndIndex;
            public float4 PhotonEnergy;
            public float2 Center;
            public float Radius;
            public float Time;
            public uint Seed;

            public void Execute(int i)
            {
                var rand = new SimpleRNG(Seed + (uint)i);

                var batchSize = (EndIndex - StartIndex - 1) / 32 + 1;

                var batchStart = StartIndex + batchSize * i;
                var batchEnd = math.min(EndIndex, batchStart + batchSize);

                for (int u = batchStart; u < batchEnd; u++)
                {
                    var angle = rand.NextFloat() * 2 * math.PI;
                    var offset = math.sqrt(rand.NextFloat());

                    math.sincos(angle, out var s, out var c);

                    var p = new float2(c, s) * offset;

                    Rays[u] = new EmittedRay
                    {
                        Energy = PhotonEnergy,
                        Position = Center + p * Radius,
                        Direction = float2.zero
                    };
                }
            }
        }

        #region Editor Stuff
#if UNITY_EDITOR
        static readonly string _GizmoPath = "Assets/Scripts/RayTracing2D/Editor/Gizmos/Lights/PointLight.png";

        [MenuItem("GameObject/Ray Tracing 2D/Create Point Light")]
        private static void CreatePointLight()
        {
            var go = new GameObject("Point Light 2D");
            go.AddComponent<PointLightRT2D>();
        }

        private void EditorStart()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private void EditorDestroy()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private void OnPlayModeChanged(PlayModeStateChange obj)
        {
            switch(obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    Cleanup();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    Cleanup();
                    break;
            }
        }

        private void OnValidate()
        {
            innerAngle = Mathf.Min(innerAngle, 360);
            outerAngle = Mathf.Min(outerAngle, 360);
            innerAngle = Mathf.Max(innerAngle, 0);
            outerAngle = Mathf.Max(outerAngle, innerAngle);
            radius = Mathf.Max(radius, 0);
        }

        private void OnDrawGizmos()
        {
            var pos = transform.position;

            Gizmos.DrawIcon(pos, _GizmoPath, true, color);

            RT2DRenderer.RegisterLight(this);
        }

        private void OnDrawGizmosSelected()
        {
            var m = Camera.current.projectionMatrix.inverse;

            var pos = transform.position;
            var up = transform.up;

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(pos, Rotate2D(up, innerAngle / 2) * radius * 5);
            Gizmos.DrawRay(pos, Rotate2D(up, -innerAngle / 2) * radius * 5);
            Gizmos.DrawRay(pos, Rotate2D(up, outerAngle / 2) * radius * 5);
            Gizmos.DrawRay(pos, Rotate2D(up, -outerAngle / 2) * radius * 5);
            //Gizmos.DrawWireSphere(pos, radius);

            var arcGap = m.MultiplyVector(new Vector3(20.0f / Camera.current.pixelWidth, 0, 0)).magnitude;
            Handles.DrawWireArc(pos, Vector3.back, Vector3.right, 360, radius);
            Handles.DrawWireArc(pos, Vector3.back, Rotate2D(up, -outerAngle / 2) * (radius + 2), outerAngle, radius + arcGap);
            //Handles.ArrowHandleCap(44444, pos + Rotate2D(up, outerAngle / 2), Quaternion.identity, 1, EventType.MouseDown);

            Handles.color = Color.red;
            Handles.CubeHandleCap(1, pos, Quaternion.identity, 1, EventType.MouseDown);
            //Handles.ConeHandleCap
        }

        private static Vector3 Rotate2D(Vector3 v, float angle)
        {
            angle = Mathf.Deg2Rad * angle;
            var s = Mathf.Sin(angle);
            var c = Mathf.Cos(angle);

            return new Vector3(v.x * c + v.y * s, v.x * -s + v.y * c);
        }
#else
        private void EditorStart() {}
        private void EditorDestroy() {}
#endif
        #endregion
    }
}