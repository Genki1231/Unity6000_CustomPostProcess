using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Mask generation feature. Manages the pass and sources.
    /// </summary>
    public class MaskGeneratorRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] private RenderPassEvent passEvent = RenderPassEvent.AfterRenderingTransparents;
        [SerializeReference] [SerializeField] private List<MaskSourceBase> sources = new List<MaskSourceBase>();
        [SerializeField] private bool warnOnDuplicateGlobalProperty = true;
        [SerializeField] private bool warnOnMissingDepthStencil = true;

        private MaskGeneratorRenderPass m_Pass;

        public override void Create()
        {
            m_Pass = new MaskGeneratorRenderPass();
            m_Pass.renderPassEvent = passEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (sources == null || sources.Count == 0)
                return;
            if (renderingData.cameraData.cameraType != CameraType.Game)
                return;

            m_Pass.Setup(sources, warnOnDuplicateGlobalProperty, warnOnMissingDepthStencil);
            renderer.EnqueuePass(m_Pass);
        }

        protected override void Dispose(bool disposing)
        {
            if (sources != null)
            {
                foreach (var source in sources)
                {
                    if (source?.Slot != null)
                        source.Slot.ReleaseRTHandle();
                }
            }
        }
    }
}
