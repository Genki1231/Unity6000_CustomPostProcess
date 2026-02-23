using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Mask generation pass. Processes sources in order, allocates/clears/draws slot RTs, and publishes them globally.
    /// </summary>
    public class MaskGeneratorRenderPass : ScriptableRenderPass
    {
        private const string k_PublishPassName = "MaskGenerator_PublishGlobal";

        private List<MaskSourceBase> m_Sources;
        private bool m_WarnOnDuplicateGlobalProperty;
        private bool m_WarnOnMissingDepthStencil;
        private HashSet<int> m_SetPropertyIdsThisCamera = new HashSet<int>();
        private static readonly HashSet<int> s_WarnedDuplicatePropertyIds = new HashSet<int>();

        public void Setup(List<MaskSourceBase> sources, bool warnOnDuplicateGlobalProperty, bool warnOnMissingDepthStencil)
        {
            m_Sources = sources;
            m_WarnOnDuplicateGlobalProperty = warnOnDuplicateGlobalProperty;
            m_WarnOnMissingDepthStencil = warnOnMissingDepthStencil;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_Sources == null || m_Sources.Count == 0)
                return;

            var cameraData = frameData.Get<UniversalCameraData>();
            var resourceData = frameData.Get<UniversalResourceData>();
            Camera camera = cameraData.camera;

            RenderTextureDescriptor baseDesc = cameraData.cameraTargetDescriptor;
            baseDesc.depthBufferBits = 0;
            baseDesc.depthStencilFormat = Experimental.Rendering.GraphicsFormat.None;

            m_SetPropertyIdsThisCamera.Clear();

            for (int i = 0; i < m_Sources.Count; i++)
            {
                MaskSourceBase source = m_Sources[i];
                if (source == null || !source.Enabled || source.Slot == null)
                    continue;

                MaskSlot slot = source.Slot;
                slot.EnsurePropertyId();
                if (string.IsNullOrEmpty(slot.globalPropertyName))
                    continue;

                EnsureSlotRT(slot, baseDesc);
                if (slot.rtHandle == null)
                    continue;

                TextureHandle slotRT = renderGraph.ImportTexture(slot.rtHandle);

                var ctx = new MaskSourceContext
                {
                    cameraId = camera.GetInstanceID(),
                    cameraDesc = baseDesc,
                    activeDepth = resourceData.activeDepthTexture,
                    activeColor = resourceData.activeColorTexture,
                    slotRT = slotRT
                };

                source.RecordMask(renderGraph, frameData, ctx);

                if (m_WarnOnDuplicateGlobalProperty && m_SetPropertyIdsThisCamera.Contains(slot.globalPropertyId))
                {
                    if (!s_WarnedDuplicatePropertyIds.Contains(slot.globalPropertyId))
                    {
                        s_WarnedDuplicatePropertyIds.Add(slot.globalPropertyId);
                        Debug.LogWarning($"[MaskGenerator] Duplicate global property name: '{slot.globalPropertyName}'. Later source wins.");
                    }
                }
                m_SetPropertyIdsThisCamera.Add(slot.globalPropertyId);

                PublishGlobal(renderGraph, slot, slotRT);
            }
        }

        private void EnsureSlotRT(MaskSlot slot, in RenderTextureDescriptor baseDesc)
        {
            var desc = new RenderTextureDescriptor(baseDesc.width, baseDesc.height, slot.graphicsFormat, 0)
                {
                    depthBufferBits = baseDesc.depthBufferBits,
                    depthStencilFormat = baseDesc.depthStencilFormat,
                    msaaSamples = baseDesc.msaaSamples,
                    useMipMap = false
                };

            RenderingUtils.ReAllocateHandleIfNeeded(ref slot.rtHandle, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: slot.globalPropertyName);
        }

        private void PublishGlobal(RenderGraph renderGraph, MaskSlot slot, TextureHandle slotRT)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PublishPassData>(k_PublishPassName, out var passData))
            {
                passData.globalPropertyId = slot.globalPropertyId;
                passData.slotTexture = slotRT;

                builder.UseTexture(slotRT, AccessFlags.Read);
                builder.AllowGlobalStateModification(true);
                builder.SetRenderFunc(static (PublishPassData data, RasterGraphContext context) =>
                {
                    if (data.slotTexture.IsValid())
                        context.cmd.SetGlobalTexture(data.globalPropertyId, data.slotTexture);
                });
            }
        }

        private class PublishPassData
        {
            internal int globalPropertyId;
            internal TextureHandle slotTexture;
        }
    }
}
