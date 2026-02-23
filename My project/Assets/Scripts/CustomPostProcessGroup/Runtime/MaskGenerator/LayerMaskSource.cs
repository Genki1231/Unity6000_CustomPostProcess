using System;
using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Renders the specified layer and generates a mask texture.
    /// </summary>
    [Serializable]
    public class LayerMaskSource : MaskSourceBase
    {
        [SerializeField] private LayerMask layerMask = -1;
        [FormerlySerializedAs("defaultOverrideMaterial")]
        [Tooltip("Default override material used when the slot's override material is null.")]
        [SerializeField] private Material m_material;

        private static readonly HashSet<int> s_WarnedDepthMissing = new HashSet<int>();
        private static readonly HashSet<int> s_WarnedMaterialMissing = new HashSet<int>();
        private static readonly List<ShaderTagId> s_ShaderTagIds = new List<ShaderTagId>();

        public LayerMask LayerMask => layerMask;
        public Material Material => m_material;

        public override void RecordMask(RenderGraph rg, ContextContainer frameData, in MaskSourceContext ctx)
        {
            if (!ctx.slotRT.IsValid())
                return;


            ClearSlotToColor(rg, ctx.slotRT);
            
            if (m_material == null)
            {
                if (!s_WarnedMaterialMissing.Contains(ctx.cameraId))
                {
                    s_WarnedMaterialMissing.Add(ctx.cameraId);
                    Debug.LogWarning("[LayerMaskSource] Override material is null. Mask slot cleared.");
                }
                return;
            }

            if (!ctx.activeDepth.IsValid())
            {
                if (!s_WarnedDepthMissing.Contains(ctx.cameraId))
                {
                    s_WarnedDepthMissing.Add(ctx.cameraId);
                    Debug.LogWarning("[LayerMaskSource] Depth not available. Mask slot cleared.");
                }
                return;
            }

            var universalRenderingData = frameData.Get<UniversalRenderingData>();
            var cameraData = frameData.Get<UniversalCameraData>();
            var lightData = frameData.Get<UniversalLightData>();

            s_ShaderTagIds.Clear();
            s_ShaderTagIds.Add(new ShaderTagId("UniversalForwardOnly"));
            s_ShaderTagIds.Add(new ShaderTagId("UniversalForward"));
            s_ShaderTagIds.Add(new ShaderTagId("SRPDefaultUnlit"));
            s_ShaderTagIds.Add(new ShaderTagId("LightweightForward"));

            var sortFlags = cameraData.defaultOpaqueSortFlags;
            var filterSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
            var drawSettings = RenderingUtils.CreateDrawingSettings(s_ShaderTagIds, universalRenderingData, cameraData, lightData, sortFlags);
            drawSettings.overrideMaterial = m_material;
            drawSettings.overrideMaterialPassIndex = 0;

            var rendererListParams = new RendererListParams(universalRenderingData.cullResults, drawSettings, filterSettings);
            RendererListHandle listHandle = rg.CreateRendererList(rendererListParams);

            const string passName = "LayerMaskSource";
            using (var builder = rg.AddRasterRenderPass<LayerMaskPassData>(passName, out var passData))
            {
                passData.rendererListHandle = listHandle;

                builder.UseRendererList(listHandle);
                builder.SetRenderAttachment(ctx.slotRT, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(ctx.activeDepth, AccessFlags.Read);

                builder.SetRenderFunc(static (LayerMaskPassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(RTClearFlags.Color, data.clearColor, 1f, 0);
                    context.cmd.DrawRendererList(data.rendererListHandle);
                });
            }
        }

        private static void ClearSlotToColor(RenderGraph rg, TextureHandle slotRT)
        {
            const string clearPassName = "LayerMaskSource_Clear";
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

        private class LayerMaskPassData
        {
            internal RendererListHandle rendererListHandle;
            internal Color clearColor;
        }

        private class ClearPassData
        {
            internal TextureHandle target;
        }
    }
}
