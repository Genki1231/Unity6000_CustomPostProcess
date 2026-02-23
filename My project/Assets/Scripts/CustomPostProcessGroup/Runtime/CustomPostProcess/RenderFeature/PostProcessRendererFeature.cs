using System.Collections.Generic;
using UnityEngine.Rendering;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// ポスト処理 Feature。エフェクトリストを保持する（仕様 5.1）
    /// Material は各 PostEffectAssetBase が保持し、Dispose で解放する。
    /// </summary>
    public class PostProcessRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] private RenderPassEvent passEvent = RenderPassEvent.AfterRenderingPostProcessing - 1;
        [SerializeField] private List<PostEffectAssetBase> effects = new List<PostEffectAssetBase>();

        private PostProcessRenderPass m_Pass;

        public override void Create()
        {
            m_Pass = new PostProcessRenderPass();
            m_Pass.renderPassEvent = passEvent;
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
                        effect.ReleaseMaterial();
                }
            }
        }
    }
}
