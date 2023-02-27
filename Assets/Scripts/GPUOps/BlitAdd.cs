using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace RayTracing2D.Ops
{
    public class BlitAdd : GPUOpBase
    {
        public static readonly string BlitAdd_ShaderName = "Hidden/RT2D/BlitAdd";

        public BlitAdd(CommandBuffer commander) : base(BlitAdd_ShaderName, commander) { }
    }
}
