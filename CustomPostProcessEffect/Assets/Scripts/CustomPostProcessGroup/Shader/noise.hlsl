#ifndef _INCLUDE_POSTPROCESSEFFECT_NOISE_HLSL_
#define _INCLUDE_POSTPROCESSEFFECT_NOISE_HLSL_

float Hash21(float2 p)
{
    p = frac(p * float2(12.9898, 352.5453));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float NoiseSimple(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);

    float a = Hash21(i);
    float b = Hash21(i + float2(1.0, 0.0));
    float c = Hash21(i + float2(0.0, 1.0));
    float d = Hash21(i + float2(1.0, 1.0));

    float2 u = f * f * (3.0 - 2.0 * f);

    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

float Hash31(float3 p)
{
    p = frac(p * 0.1031);
    p += dot(p, p.yzx + 33.33);
    return frac((p.x + p.y) * p.z);
}

float NoiseSimple3D(float3 p)
{
    float3 i = floor(p);
    float3 f = frac(p);

    float n000 = Hash31(i + float3(0.0, 0.0, 0.0));
    float n100 = Hash31(i + float3(1.0, 0.0, 0.0));
    float n010 = Hash31(i + float3(0.0, 1.0, 0.0));
    float n110 = Hash31(i + float3(1.0, 1.0, 0.0));

    float n001 = Hash31(i + float3(0.0, 0.0, 1.0));
    float n101 = Hash31(i + float3(1.0, 0.0, 1.0));
    float n011 = Hash31(i + float3(0.0, 1.0, 1.0));
    float n111 = Hash31(i + float3(1.0, 1.0, 1.0));

    float3 u = f * f * (3.0 - 2.0 * f);

    float nx00 = lerp(n000, n100, u.x);
    float nx10 = lerp(n010, n110, u.x);
    float nx01 = lerp(n001, n101, u.x);
    float nx11 = lerp(n011, n111, u.x);

    float nxy0 = lerp(nx00, nx10, u.y);
    float nxy1 = lerp(nx01, nx11, u.y);

    return lerp(nxy0, nxy1, u.z);
}

#endif