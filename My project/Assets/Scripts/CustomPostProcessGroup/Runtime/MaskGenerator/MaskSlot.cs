using System;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Defines one mask output (RT + global publication).
    /// </summary>
    [Serializable]
    public class MaskSlot
    {
        [SerializeField] public string globalPropertyName = "_CustomMask";
        [SerializeField] public GraphicsFormat graphicsFormat = GraphicsFormat.R8_UNorm;

        internal int globalPropertyId;
        internal RTHandle rtHandle;

        /// <summary>
        /// Caches the property ID when the slot becomes valid or on first reference each frame.
        /// </summary>
        public void EnsurePropertyId()
        {
            if (string.IsNullOrEmpty(globalPropertyName))
                return;
            globalPropertyId = Shader.PropertyToID(globalPropertyName);
        }

        public void ReleaseRTHandle()
        {
            if (rtHandle != null && rtHandle.rt != null)
            {
                RTHandles.Release(rtHandle);
                rtHandle = null;
            }
        }
    }
}
