using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class RT2DExtensions
{
    public static RenderTextureDescriptor WithDepth(this RenderTextureDescriptor @this, int depth)
    {
        @this.depthBufferBits = depth;
        return @this;
    }

    public static RenderTextureDescriptor WithFormat(this RenderTextureDescriptor @this, RenderTextureFormat format)
    {
        @this.colorFormat = format;
        return @this;
    }

    public static RenderTextureDescriptor WithEnableRandomWrite(this RenderTextureDescriptor @this, bool enableRandomWrite)
    {
        @this.enableRandomWrite = enableRandomWrite;
        return @this;
    }

    public static RenderTextureDescriptor WithMipMapUsage(this RenderTextureDescriptor @this, int mipCount, bool autogenMips)
    {
        @this.mipCount = mipCount;
        @this.useMipMap = mipCount != 1;
        @this.autoGenerateMips = autogenMips;
        return @this;
    }
}
