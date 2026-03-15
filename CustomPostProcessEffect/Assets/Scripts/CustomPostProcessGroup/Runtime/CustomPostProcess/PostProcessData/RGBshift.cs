using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.UIElements;

namespace PostProcessEffects
{
    [CreateAssetMenu(menuName = "PostProcessEffects/RGBshift")]
    public class RGBshift : PostEffectAssetBase
    {
        public float shiftAmount = 1.0f;
        public float noiseSize = 1.0f;
        public float noiseSpeed = 1.0f;
        
        private const string ShaderName = "PostProcessEffects/RGBshift";
        private static readonly int EffectShiftamountVal = Shader.PropertyToID("_effect_shiftamount_val");
        private static readonly int EffectNoisesizeVal = Shader.PropertyToID("_effect_noisesize_val");
        private static readonly int EffectTotaltimeVal = Shader.PropertyToID("_effect_totaltime_val");

        private float _totalTime = 0.0f;

        public override void Initialize()
        {
            if (m_Material == null)
            {
                m_Material = CoreUtils.CreateEngineMaterial(ShaderName);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _totalTime = 0.0f;
        }

        public override void Record(RenderGraph rg, ContextContainer frameData, in PostEffectContext ctx)
        {
            if (!ctx.sourceColor.IsValid() || m_Material == null)
               return;
            
            _totalTime += Time.deltaTime;
            
            m_Material.SetFloat(EffectShiftamountVal, shiftAmount);
            m_Material.SetFloat(EffectNoisesizeVal, noiseSize);
            m_Material.SetFloat(EffectTotaltimeVal, _totalTime * noiseSpeed);

            var para = new RenderGraphUtils.BlitMaterialParameters(ctx.sourceColor, ctx.sourceColor, m_Material, 0);
            rg.AddBlitPass(para, passName: "FullscreenBlitEffect");
        }
    }
}