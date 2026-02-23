using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// ポスト処理 Pass。effects を順次実行し、Swap で cameraColor を更新する（仕様 5.2）
    /// </summary>
    public class PostProcessRenderPass : ScriptableRenderPass
    {
        private List<PostEffectAssetBase> m_Effects;

        public PostProcessRenderPass()
        {
            requiresIntermediateTexture = true;
        }

        public void Setup(List<PostEffectAssetBase> effects)
        {
            m_Effects = effects;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (m_Effects == null || m_Effects.Count == 0)
                return;

            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            if (resourceData.isActiveTargetBackBuffer)
                return;

            TextureHandle source = resourceData.activeColorTexture;
            
            var ctx = new PostEffectContext
            {
                sourceColor = source,
                activeDepth = resourceData.activeDepthTexture,
                cameraDesc = cameraData.cameraTargetDescriptor,
                cameraId = cameraData.camera != null ? cameraData.camera.GetInstanceID() : 0
            };

            for (int i = 0; i < m_Effects.Count; i++)
            {
                PostEffectAssetBase effect = m_Effects[i];
                if (effect == null || !effect.Enabled)
                    continue;

                effect.GetMaterial();
                effect.Record(renderGraph, frameData, ctx);
            }
        }
    }
}
