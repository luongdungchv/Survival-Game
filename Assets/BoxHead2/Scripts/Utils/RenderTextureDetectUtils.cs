using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace BoxHead2.Utils
{
    public static class RenderTextureDetectUtils
    {
        public static RenderTexture CreateSafeRenderTexture(int width, int height, bool enableSRGB = true)
        {
            GraphicsFormat colorFormat;
            if (enableSRGB && SystemInfo.IsFormatSupported(GraphicsFormat.R8G8B8A8_SRGB, FormatUsage.Render))
            {
                colorFormat = GraphicsFormat.R8G8B8A8_SRGB;
            }
            else if (SystemInfo.IsFormatSupported(GraphicsFormat.R8G8B8A8_UNorm, FormatUsage.Render))
            {
                colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
                enableSRGB = false;
            }
            else
            {
                colorFormat = GraphicsFormat.B8G8R8A8_UNorm;
                enableSRGB = false;
            }

            // Fallback Depth Format
            GraphicsFormat depthStencilFormat;
            if (SystemInfo.IsFormatSupported(GraphicsFormat.D24_UNorm, FormatUsage.Render))
                depthStencilFormat = GraphicsFormat.D24_UNorm;
            else if (SystemInfo.IsFormatSupported(GraphicsFormat.D16_UNorm, FormatUsage.Render))
                depthStencilFormat = GraphicsFormat.D16_UNorm;
            else if (SystemInfo.IsFormatSupported(GraphicsFormat.D32_SFloat, FormatUsage.Render))
                depthStencilFormat = GraphicsFormat.D32_SFloat;
            else
            {
                depthStencilFormat = GraphicsFormat.None;
            }
            
            // Build RenderTextureDescriptor
            var descriptor = new RenderTextureDescriptor(width, height)
            {
                graphicsFormat = colorFormat,
                depthStencilFormat = depthStencilFormat,
                sRGB = enableSRGB,
                msaaSamples = 1,
                useMipMap = false,
                autoGenerateMips = false,
            };

            var rt = new RenderTexture(descriptor);
            rt.Create();

            return rt;
        }

        public static bool IsSafeRenderTextureFormat(RenderTextureFormat format)
        {
            if (!SystemInfo.SupportsRenderTextureFormat(format)) return false;
            if (!SystemInfo.SupportsRandomWriteOnRenderTextureFormat(format)) return false;
            var rt = new RenderTexture(32, 32, 0, format)
            {
                enableRandomWrite = true
            };

            bool created = rt.Create();
            if (!created)
            {
                rt.Release();
                return false;
            }

            if (!rt.IsCreated())
            {
                rt.Release();
                return false;
            }

            rt.Release();
            return true;
        }

        public static RenderTextureFormat GetSafeRTFormatForRandomWrite(out bool found)
        {
            var result = RenderTextureFormat.RFloat;
            found = true;
            if (!IsSafeRenderTextureFormat(result)) result = RenderTextureFormat.Default;
            if (!IsSafeRenderTextureFormat(result)) result = RenderTextureFormat.RGB565;
            if (!IsSafeRenderTextureFormat(result)) found = false;
            return result;
        }
    }
}