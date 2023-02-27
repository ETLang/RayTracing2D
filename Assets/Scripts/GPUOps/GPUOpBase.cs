using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracing2D.Ops
{
    public class GPUOpBase
    {
        public static readonly int PID_BlitMainTex = Shader.PropertyToID("_MainTex");
        public static readonly int PID_BlitMainTexLOD = Shader.PropertyToID("_MainTexLOD");
        public static readonly int PID_Color = Shader.PropertyToID("_ColorMod");
        public static readonly int PID_MaxMipLevel = Shader.PropertyToID("_MaxMipLevel");

        public string ShaderName { get; private set; }
        public bool IsInitialized { get; private set; }
        public Material Material { get; protected set; }
        public CommandBuffer Commander { get; private set; }
        public static Mesh BlitQuad { get; private set; }

        protected GPUOpBase(string shaderName, CommandBuffer commander)
        {
            ShaderName = shaderName;
            Commander = commander;
        }

        public void Initialize()
        {
            if (IsInitialized) return;

            IsInitialized = true;
            Material = new Material(Shader.Find(ShaderName));

            if (!BlitQuad)
            {
                BlitQuad = new Mesh();
                BlitQuad.SetVertices(new Vector3[]
                {
                    new Vector3(-1,1,0),
                    new Vector3(1,1,0),
                    new Vector3(1,-1,0),
                    new Vector3(-1,-1,0)
                });

                BlitQuad.SetUVs(0, new Vector2[]
                {
                    new Vector2(0,1),
                    new Vector2(1,1),
                    new Vector2(1,0),
                    new Vector2(0,0)
                });

                BlitQuad.SetIndices(new int[] { 0, 1, 2, 0, 2, 3 }, MeshTopology.Triangles, 0);
            }
        }

        protected void Blit(Texture source, float sourceMip, RenderTargetIdentifier target, int targetMip, Material mat, int shaderPass = -1)
        {
            if (mat == null) return;

            Commander.SetRenderTarget(target, targetMip);
            var m = new Material(mat);
            m.SetTexture(PID_BlitMainTex, source);
            m.SetFloat(PID_BlitMainTexLOD, sourceMip);

            if (shaderPass != -1)
                m.SetPass(shaderPass);

            Commander.DrawMesh(BlitQuad, Matrix4x4.identity, m);
        }

        protected void Blit(Texture source, RenderTargetIdentifier target, Material mat, int shaderPass = -1)
        {
            Blit(source, 0, target, 0, mat, shaderPass);
        }

        public void Paint(Texture source, RenderTargetIdentifier target)
        {
            Paint(source, 0, target, 0);
        }

        public virtual void Paint(Texture source, float sourceMip, RenderTargetIdentifier target, int targetMip)
        {
            Initialize();
            Blit(source, sourceMip, target, targetMip, Material);
        }
    }
}
