using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Pass→Effect の引数構造体（仕様 5.3）
    /// マスクは Shader Global から取得する設計のため、ctx には含めない。
    /// </summary>
    public struct PostEffectContext
    {
        public TextureHandle sourceColor;
        public TextureHandle activeDepth;
        public RenderTextureDescriptor cameraDesc;
        public int cameraId;
    }
}
