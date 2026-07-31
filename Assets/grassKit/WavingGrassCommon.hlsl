#ifndef WAVING_GRASS_COMMON_INCLUDED
#define WAVING_GRASS_COMMON_INCLUDED

// ─────────────────────────────────────────────────────────────────────────────
// Shared wave displacement logic used by both the ForwardLit and ShadowCaster
// passes. Include this file after Core.hlsl and after the UnityPerMaterial
// CBUFFER has been declared.
// ─────────────────────────────────────────────────────────────────────────────

void FastSinCos(float4 val, out float4 s, out float4 c)
{
    val = val * 6.408849 - 3.1415927;
    float4 r5 = val * val;
    float4 r6 = r5 * r5;
    float4 r7 = r6 * r5;
    float4 r1 = r5 * val;
    float4 r2 = r1 * r5;
    float4 r3 = r2 * r5;
    c = (1.0 - 0.5 * r5 + 0.0416667 * r6 - 0.00138889 * r7 * r5);
    s = val + r1 * (-0.166667) + r2 * 8.33333e-3 - r3 * 1.98413e-4;
}

// Displaces positionOS.xz in-place and returns the wave tint color.
// waveAndDistance: x = _Time.x * _WaveSpeed, y = _WaveFrequency,
//                  z = _WaveAndDistortion,    w = 1.0
half4 TerrainWaveGrass_Mod(inout float4 positionOS, float waveAmount,
                            half4 color, float4 waveAndDistance)
{
    float4 _waveXSize = float4(0.012, 0.02,  0.06,  0.024) * waveAndDistance.y;
    float4 _waveZSize = float4(0.006, 0.02,  0.02,  0.05)  * waveAndDistance.y;
    float4  waveSpeed = float4(0.3,   0.5,   0.4,   1.2) * 4.0;

    float4 _waveXmove = float4(0.012, 0.02, -0.06,  0.048) * 2.0;
    float4 _waveZmove = float4(0.006, 0.02, -0.02,  0.1);

    float4 waves;
    waves  = positionOS.x * _waveXSize;
    waves += positionOS.z * _waveZSize;
    waves += waveAndDistance.x * waveSpeed;

    float4 s, c;
    waves = frac(waves);
    FastSinCos(waves, s, c);

    s = s * s;
    s = s * s;

    float lighting = dot(s, normalize(float4(1.0, 1.0, 0.4, 0.2))) * 0.7;

    s = s * waveAmount;

    float3 waveMove = float3(0.0, 0.0, 0.0);
    waveMove.x = dot(s, _waveXmove);
    waveMove.z = dot(s, _waveZmove);

    positionOS.xz -= waveMove.xz * waveAndDistance.z;

    half3 waveColor = lerp(_WavingTintDarken.rgb, _WavingTintLighten.rgb, lighting);
    return half4(2.0 * waveColor * color.rgb, color.a);
}

// Applies the full grass vertex displacement (view-bend + wave) to positionOS.
// Returns the wave tint color. Call this from any vertex shader.
half4 ApplyGrassDisplacement(inout float4 positionOS, half4 vertexColor)
{
    float4 waveAndDistance = float4(
        _Time.x * _WaveSpeed,
        _WaveFrequency,
        _WaveAndDistortion,
        1.0
    );

    float grassHeight = saturate((positionOS.y + _GrassBottom) / max(_GrassTop, 0.0001));

    // View-direction bend (camera lean) in object space
    float3 viewDirOS = normalize(TransformWorldToObject(GetCameraPositionWS()) - positionOS.xyz);
    float  fresnel   = viewDirOS.y;
    viewDirOS.y      = 0.0;
    viewDirOS        = normalize(viewDirOS);
    positionOS.xz   -= viewDirOS.xz * _BendByView * fresnel * grassHeight;

    float waveAmount = grassHeight * waveAndDistance.z;
    return TerrainWaveGrass_Mod(positionOS, waveAmount, vertexColor, waveAndDistance);
}

#endif // WAVING_GRASS_COMMON_INCLUDED
