using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace RayTracing2D.Ops
{
    public class Gauss9Sample : GPUOpBase
    {
        public static readonly string Gauss9Sample_ShaderName = "Hidden/Gauss9Sample";

        public Gauss9Sample(CommandBuffer commander) : base(Gauss9Sample_ShaderName, commander) { }
    }
}
