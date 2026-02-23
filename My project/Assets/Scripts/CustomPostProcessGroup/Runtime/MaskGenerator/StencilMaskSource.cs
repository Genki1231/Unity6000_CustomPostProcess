using System;
using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Writes the existing stencil result as a mask texture.
    /// </summary>
    [Serializable]
    public class StencilMaskSource : MaskSourceBase
    {
        [SerializeField] private int stencilRef = 1;
        [SerializeField] private CompareFunction stencilCompare = CompareFunction.Equal;
        [Tooltip("Fullscreen material that outputs color where stencil test passes. Stencil Ref/Comp use this component's values.")]
        [SerializeField] private Material fullscreenStencilMaskMaterial;

        private static readonly HashSet<int> s_WarnedDepthMissing = new HashSet<int>();

        public int StencilRef => stencilRef;
        public CompareFunction StencilCompare => stencilCompare;
        public Material FullscreenStencilMaskMaterial => fullscreenStencilMaskMaterial;

        public override void RecordMask(RenderGraph rg, ContextContainer frameData, in MaskSourceContext ctx)
        {
            if (!ctx.slotRT.IsValid())
                return;

            ClearSlotToColor(rg, ctx.slotRT);

            if (!ctx.activeDepth.IsValid())
            {
                if (!s_WarnedDepthMissing.Contains(ctx.cameraId))
                {
                    s_WarnedDepthMissing.Add(ctx.cameraId);
                    Debug.LogWarning("[StencilMaskSource] Depth/Stencil not available. Mask slot cleared to zero.");
                }
                return;
            }

            if (fullscreenStencilMaskMaterial == null)
            {
                return;
            }

            const string passName = "StencilMaskSource";
            using (var builder = rg.AddRasterRenderPass<StencilMaskPassData>(passName, out var passData))
            {
                passData.material = fullscreenStencilMaskMaterial;
                passData.stencilRef = stencilRef;
                passData.stencilCompare = (int)stencilCompare;
                passData.depthTexture = ctx.activeDepth;
                passData.slotRT = ctx.slotRT;

                builder.SetRenderAttachment(ctx.slotRT, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(ctx.activeDepth, AccessFlags.Read);

                builder.SetRenderFunc(static (StencilMaskPassData data, RasterGraphContext context) =>
                {
                    if (data.material == null) return;
                    data.material.SetInt("_StencilRef", data.stencilRef);
                    data.material.SetInt("_StencilComp", data.stencilCompare);
                    Blitter.BlitTexture(context.cmd, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }
        }

        internal static void ClearSlotToColor(RenderGraph rg, TextureHandle slotRT)
        {
            const string clearPassName = "StencilMaskSource_Clear";
            using (var builder = rg.AddRasterRenderPass<ClearPassData>(clearPassName, out var passData))
            {
                passData.target = slotRT;
                builder.SetRenderAttachment(slotRT, 0, AccessFlags.Write);
                builder.SetRenderFunc(static (ClearPassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(RTClearFlags.Color, Color.black, 1f, 0);
                });
            }
        }

        private class StencilMaskPassData
        {
            internal Material material;
            internal int stencilRef;
            internal int stencilCompare;
            internal TextureHandle depthTexture;
            internal TextureHandle slotRT;
        }

        private class ClearPassData
        {
            internal TextureHandle target;
        }
    }
}
