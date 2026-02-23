using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Argument struct passed from Pass to Effect.
    /// Masks are read from Shader Globals, so they are not included in ctx.
    /// </summary>
    public struct PostEffectContext
    {
        public TextureHandle sourceColor;
        public TextureHandle activeDepth;
        public RenderTextureDescriptor cameraDesc;
        public int cameraId;
    }
}
