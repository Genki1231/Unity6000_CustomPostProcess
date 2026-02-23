using System;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// One unit of input (Stencil/Layer) to MaskSlot output. One source = one slot.
    /// </summary>
    [Serializable]
    public abstract class MaskSourceBase
    {
        [SerializeField] protected bool enabled = true;
        [SerializeField] protected MaskSlot slot = new MaskSlot();

        public virtual bool Enabled => enabled;
        public MaskSlot Slot => slot;

        /// <summary>
        /// Records the mask drawing pass into the RenderGraph.
        /// </summary>
        public abstract void RecordMask(RenderGraph rg, ContextContainer frameData, in MaskSourceContext ctx);
    }
}
