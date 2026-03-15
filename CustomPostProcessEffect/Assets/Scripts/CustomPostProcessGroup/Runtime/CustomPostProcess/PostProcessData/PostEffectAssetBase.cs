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

        internal Material m_Material;
        
        public virtual void Initialize()
        {
            
        }

        public virtual void Dispose()
        {
            ReleaseMaterial();
        }
        
        private void ReleaseMaterial()
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
