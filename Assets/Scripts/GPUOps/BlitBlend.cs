using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace RayTracing2D.Ops
{
    public class BlitBlend : GPUOpBase
    {
        public static readonly string BlitBlend_ShaderName = "Hidden/RT2D/BlitBlend";

        public BlitBlend(CommandBuffer commander) : base(BlitBlend_ShaderName, commander) { }
    }
}
