using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace RayTracing2D.Ops
{
    public class BlitCopy : GPUOpBase
    {
        public static readonly string BlitCopy_ShaderName = "Hidden/RT2D/BlitCopy";

        public BlitCopy(CommandBuffer commander) : base(BlitCopy_ShaderName, commander) { }
    }
}
