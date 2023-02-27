using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracing2D.Ops
{
    public class BlitDiagnostic : GPUOpBase
    {
        public static readonly string BlitDiagnostic_ShaderName = "Hidden/RT2D/BlitDiagnostic";

        public BlitDiagnostic(CommandBuffer commander) : base(BlitDiagnostic_ShaderName, commander) { }

        public float VarianceMipMax { get; set; }

        public override void Paint(Texture source, float sourceMip, RenderTargetIdentifier target, int targetMip)
        {
            Material.SetFloat(PID_MaxMipLevel, Mathf.Min(source.mipmapCount - 1, VarianceMipMax));
            base.Paint(source, sourceMip, target, targetMip);
        }
    }
}
