using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace RayTracing2D.Ops
{
    public class LogOfIntensity : GPUOpBase
    {
        public static readonly string LogOfIntensity_ShaderName = "Hidden/RT2D/OpLogIntensity";

        public LogOfIntensity(CommandBuffer commander) : base(LogOfIntensity_ShaderName, commander) { }
    }
}
