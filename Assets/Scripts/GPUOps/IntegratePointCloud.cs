using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracing2D.Ops
{
    public class IntegratePointCloud : GPUOpBase
    {
        public static readonly string IntegratePointCloud_ShaderName = "Hidden/RT2D/OpIntegratePointCloud";

        public IntegratePointCloud(CommandBuffer commander) : base(IntegratePointCloud_ShaderName, commander) { }

        public float MaxMipLevel
        {
            get => Material.GetFloat(PID_MaxMipLevel);
            set => Material.SetFloat(PID_MaxMipLevel, value);
        }

        public override void Paint(Texture source, float sourceMip, RenderTargetIdentifier target, int targetMip)
        {
            Initialize();
            MaxMipLevel = source.mipmapCount - 1;
            base.Paint(source, sourceMip, target, targetMip);
        }
    }
}
