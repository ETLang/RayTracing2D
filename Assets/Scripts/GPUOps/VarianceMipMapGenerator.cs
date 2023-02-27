using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace RayTracing2D.Ops
{
    public enum VarianceSource
    {
        X,
        Y,
        Z,
        W,
        Grayscale,
        Accumulate
    }

    public class VarianceMipMapGenerator : GPUOpBase
    {
        public static readonly string VarianceMipMapGenerator_ShaderName = "Hidden/RT2D/VarianceMipMapGenerator";

        static readonly Dictionary<VarianceSource, string> _VarianceSources = new Dictionary<VarianceSource, string>
        {
            { VarianceSource.X, "SAMPLE_X" },
            { VarianceSource.Y, "SAMPLE_Y" },
            { VarianceSource.Z, "SAMPLE_Z" },
            { VarianceSource.W, "SAMPLE_W" },
            { VarianceSource.Grayscale, "SAMPLE_GRAYSCALE" },
            { VarianceSource.Accumulate, "ACCUMULATE" }
        };

        public VarianceMipMapGenerator(CommandBuffer commander) : base(VarianceMipMapGenerator_ShaderName, commander) { }

        public VarianceSource VarianceSource
        {
            get => _varianceSource;
            set
            {
                if (_varianceSource == value) return;

                _varianceSource = value;

                foreach (var keyword in _VarianceSources.Values)
                    Material.DisableKeyword(keyword);
                Material.EnableKeyword(_VarianceSources[value]);
            }
        }
        private VarianceSource _varianceSource = VarianceSource.Accumulate;
    }
}
