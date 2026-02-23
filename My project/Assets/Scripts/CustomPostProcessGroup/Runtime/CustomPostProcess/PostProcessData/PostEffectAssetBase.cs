using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// ポストエフェクトのデータ定義。Record で RenderGraph にパスを登録する（仕様 5.3）
    /// マスクは Shader 内でグローバル名を指定して参照する。Effect は maskProperties を持たない。
    /// Material は本クラス内で Shader から生成・保持し、Feature の Dispose で解放する。
    /// </summary>
    public abstract class PostEffectAssetBase : ScriptableObject
    {
        [SerializeField] protected bool enabled = true;
        [SerializeField] protected Shader shader;

        internal Material m_Material;

        public virtual bool Enabled => enabled;
        public Shader Shader => shader;

        /// <summary>
        /// Shader から生成した Material。未生成なら CreateEngineMaterial で作成して返す。
        /// </summary>
        public void GetMaterial()
        {
            if (m_Material == null && shader != null)
                m_Material = CoreUtils.CreateEngineMaterial(shader);
        }

        /// <summary>
        /// 保持している Material を破棄する。Feature の Dispose から呼ぶ。
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
        /// 入力 sourceColor を加工し、出力の TextureHandle を返す
        /// </summary>
        public abstract void Record(RenderGraph rg, ContextContainer frameData, in PostEffectContext ctx);
    }
}
