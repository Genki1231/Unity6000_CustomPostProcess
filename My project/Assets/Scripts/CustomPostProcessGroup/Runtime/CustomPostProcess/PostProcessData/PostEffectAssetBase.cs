using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace PostProcessEffects
{
    /// <summary>
    /// Base PostProcessEffect Class.
    /// </summary>
    public abstract class PostEffectAssetBase : ScriptableObject
    {
        public bool enabled = true;
        public Shader shader;

        internal Material m_Material;

        /// <summary>
        /// Material created from Shader. Creates via CreateEngineMaterial if not yet created.
        /// </summary>
        public void GetMaterial()
        {
            if (m_Material == null && shader != null)
                m_Material = CoreUtils.CreateEngineMaterial(shader);
        }

        /// <summary>
        /// Destroys the held Material. Called from Feature.Dispose.
        /// </summary>
        public void ReleaseMaterial()
        {
            if (m_Material != null)
            {
                CoreUtils.Destroy(m_Material);
                m_Material = null;
            }
        }

        /// <summary>
        /// Processes input sourceColor and returns the output TextureHandle.
        /// </summary>
        public abstract void Record(RenderGraph rg, ContextContainer frameData, in PostEffectContext ctx);
    }
}
