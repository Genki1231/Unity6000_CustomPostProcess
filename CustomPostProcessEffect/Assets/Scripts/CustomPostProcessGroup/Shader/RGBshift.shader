Shader "PostProcessEffects/RGBshift"
{
    Properties
    {
        [Toggle(_EFFECT_RGB_BOOL_ON)] _effect_rgb_bool_on ("effect_rgb_bool_on", Float) = 1
        _effect_shiftamount_val ("effect_shiftamount_val", Float) = 0.1
        _effect_noisesize_val ("effect_noisesize_val", Float) = 1
        _effect_totaltime_val ("effect_totaltime_val", Float) = 0
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
            #include "noise.hlsl"

            #pragma shader_feature_local _EFFECT_RGB_BOOL_ON

            Texture2D _customMask;
            float _effect_shiftamount_val;
            float _effect_noisesize_val;
            float _effect_totaltime_val;

            half4 Frag(Varyings i) : SV_Target
            {
                float mask = SAMPLE_TEXTURE2D(_customMask, sampler_LinearClamp, i.texcoord).r;
                
                half4 output;
                
                float3 uv = float3(1, i.texcoord.y * _effect_noisesize_val + _effect_totaltime_val, 0);
                float2 noise_r = float2(NoiseSimple3D(uv) * _effect_shiftamount_val + i.texcoord.x, i.texcoord.y) * mask;
                float2 noise_g = float2(NoiseSimple3D(uv + 235.543 * _effect_noisesize_val) * _effect_shiftamount_val + i.texcoord.x, i.texcoord.y) * mask;
                float2 noise_b = float2(NoiseSimple3D(uv + 18.7421 * _effect_noisesize_val) * _effect_shiftamount_val + i.texcoord.x, i.texcoord.y) * mask;
                // float mask = SAMPLE_TEXTURE2D(_customMask, sampler_LinearClamp, noise_r).r;
                // mask = max(SAMPLE_TEXTURE2D(_customMask, sampler_LinearClamp, noise_g), mask);
                // mask = max(SAMPLE_TEXTURE2D(_customMask, sampler_LinearClamp, noise_b), mask);
                
                half4 background = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.texcoord);
                output.r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, noise_r).r;
                output.g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, noise_g).g;
                output.b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, noise_b).b;
                
                output.a = 1;
                
                return output;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
