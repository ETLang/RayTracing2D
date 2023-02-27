using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace RayTracing2D.Ops
{
    public class Gauss4Sample : GPUOpBase
    {
        public static readonly string Gauss4Sample_ShaderName = "Hidden/Gauss4Sample";

        public Gauss4Sample(CommandBuffer commander) : base(Gauss4Sample_ShaderName, commander) { }
    }
}
