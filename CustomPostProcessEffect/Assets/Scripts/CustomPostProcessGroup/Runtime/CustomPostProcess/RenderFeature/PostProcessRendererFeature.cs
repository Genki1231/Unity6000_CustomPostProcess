using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering.Universal;
using UnityEngine;

namespace PostProcessEffects
{
    /// <summary>
    /// Post-process feature. Holds the effect list.
    /// Material is held by each PostEffectAssetBase and released in Dispose.
    /// </summary>
    public class PostProcessRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] private RenderPassEvent passEvent = RenderPassEvent.AfterRenderingPostProcessing - 1;
        [SerializeField] private List<PostEffectAssetBase> effects = new List<PostEffectAssetBase>();

        private PostProcessRenderPass m_Pass;

        public override void Create()
        {
            CreateMaterials();
            m_Pass = new PostProcessRenderPass
            {
                renderPassEvent = passEvent
            };
        }
        
        private void CreateMaterials()
        {
            if (effects == null || effects.Count == 0)
                return;

            foreach (var effect in effects.Where(effect => effect != null))
            {
                effect.Initialize();
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (effects == null || effects.Count == 0)
                return;
            if (renderingData.cameraData.cameraType != CameraType.Game)
                return;

            m_Pass.Setup(effects);
            renderer.EnqueuePass(m_Pass);
        }

        protected override void Dispose(bool disposing)
        {
            if (effects != null)
            {
                foreach (var effect in effects)
                {
                    if (effect != null)
                        effect.Dispose();
                }
            }
        }
    }
}
