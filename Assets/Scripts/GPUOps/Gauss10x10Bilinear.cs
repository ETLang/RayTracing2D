using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace RayTracing2D.Ops
{
    public class Gauss10x10Bilinear : GPUOpBase
    {
        public static readonly string Gauss10x10Bilinear_ShaderName = "Hidden/Gauss10x10Bilinear";

        public Gauss10x10Bilinear(CommandBuffer commander) : base(Gauss10x10Bilinear_ShaderName, commander) { }
    }
}
