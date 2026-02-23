using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Simple fullscreen blit effect. Processes source with Material and outputs.
    /// Reference mask by global texture name in the shader when using masks.
    /// </summary>
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
