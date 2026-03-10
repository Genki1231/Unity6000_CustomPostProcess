using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

namespace PostProcessEffects
{
    /// <summary>
    /// Refelence FullScreenEffect.
    /// Use mask texture in MaskGeneratorRenderFeature.
    /// </summary>
    [CreateAssetMenu(menuName = "PostProcessEffects/FullscreenBlitEffect")]
    public class FullscreenBlitEffect : PostEffectAssetBase
    {
        public override void Record(RenderGraph rg, ContextContainer frameData, in PostEffectContext ctx)
        {
            if (!ctx.sourceColor.IsValid() || m_Material == null)
                return;

            var para = new RenderGraphUtils.BlitMaterialParameters(ctx.sourceColor, ctx.sourceColor, m_Material, 0);
            rg.AddBlitPass(para, passName: "FullscreenBlitEffect");
        }
    }
}
