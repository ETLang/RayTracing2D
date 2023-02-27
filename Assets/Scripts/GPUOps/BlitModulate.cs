using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracing2D.Ops
{
    public class BlitModulate : GPUOpBase
    {
        public static readonly string BlitModulate_ShaderName = "Hidden/RT2D/BlitModulate";

        public BlitModulate(CommandBuffer commander) : base(BlitModulate_ShaderName, commander) { }

        public Color Color
        {
            get => Material.GetColor(PID_Color);
            set => Material.SetColor(PID_Color, value);
        }
    }
}
