using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Post-effect data definition. Records passes into the RenderGraph via Record.
    /// Masks are referenced by global name in the shader. Effect does not hold maskProperties.
    /// Material is created and held in this class; released in Feature.Dispose.
    /// </summary>
    public abstract class PostEffectAssetBase : ScriptableObject
    {
        [SerializeField] protected bool enabled = true;
        [SerializeField] protected Shader shader;

        internal Material m_Material;

        public virtual bool Enabled => enabled;
        public Shader Shader => shader;

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
