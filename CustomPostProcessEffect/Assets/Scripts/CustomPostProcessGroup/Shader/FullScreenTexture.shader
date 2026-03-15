Shader "Hidden/FullScreenTexture"
{
    Properties
    {
        _screen_maskcolor_val ("screen_maskcolor_val", Color) = (1, 1, 1, 1)
        _screen_backgroundcolor_val ("screen_backgroundcolor_val", Color) = (0, 0, 0, 1)
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            Texture2D _customMask;
            half4 _screen_maskcolor_val;
            half4 _screen_backgroundcolor_val;

            half4 Frag(Varyings i) : SV_Target
            {
                float mask = SAMPLE_TEXTURE2D(_customMask, sampler_LinearClamp, i.texcoord).r;
                half4 color = lerp(_screen_backgroundcolor_val, _screen_maskcolor_val, mask);
                return color;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
