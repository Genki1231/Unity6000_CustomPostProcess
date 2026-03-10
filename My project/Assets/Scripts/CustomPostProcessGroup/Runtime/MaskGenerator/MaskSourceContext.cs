using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace MaskGenerator
{
    /// <summary>
    /// Argument struct passed from Pass to Source.
    /// </summary>
    public struct MaskSourceContext
    {
        public int cameraId;
        public RenderTextureDescriptor cameraDesc;
        public TextureHandle activeDepth;
        public TextureHandle activeColor;
        public TextureHandle slotRT;
    }
}
