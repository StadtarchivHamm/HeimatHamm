Shader "grassKit/wavingGrass"
{
    Properties
    {
        _Color  ("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB), Alpha(A)", 2D) = "white" {}
        _Cutoff ("Shadows Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _BendByView("Bend By View", Float) = 0.5
        [Space]
        _GrassBottom("Grass Bottom(Y)", Float) = 0.0
        _GrassTop   ("Grass Top(Y)",    Float) = 1.0
        [Space]
        _WaveSpeed        ("Waves Speed", Float) = 2.0
        _WaveFrequency    ("Waves Frequency", Float) = 20.0
        _WaveAndDistortion("Waves Distortion", Float) = 0.75
        [Space]
        _WavingTintLighten("Waving Tint Lighten", Color) = (0.5, 0.5, 0.5, 0.5)
        _WavingTintDarken ("Waving Tint Darken",  Color) = (0.5, 0.5, 0.5, 0.5)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue"          = "Transparent"
            "RenderType"     = "Transparent"
            "IgnoreProjector"= "True"
        }

        // ─────────────────────────────────────────────────────────────
        // Pass 1 — Opaque cutout (ZWrite On, no blend)
        //
        // Renders only fully opaque pixels (alpha >= _Cutoff) and writes
        // their depth. This gives every blade a correct depth footprint so
        // the blending pass below can sort against it without MSAA.
        // ─────────────────────────────────────────────────────────────
        Pass
        {
            Name "ForwardLitOpaque"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite On
            ZTest LEqual
            // Colour mask off: this pass only writes depth, not colour,
            // so we don't draw the grass twice.
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment fragOpaque
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _Color;
                half   _Cutoff;
                half   _BendByView;
                half   _GrassBottom;
                half   _GrassTop;
                half   _WaveSpeed;
                half   _WaveFrequency;
                half   _WaveAndDistortion;
                half4  _WavingTintLighten;
                half4  _WavingTintDarken;
            CBUFFER_END

            #include "WavingGrassCommon.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                half4  color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                half4  gColor      : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                float3 normalWS    : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.gColor = ApplyGrassDisplacement(IN.positionOS, IN.color * _Color);

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS  = posInputs.positionWS;
                OUT.normalWS    = nrmInputs.normalWS;
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            // Depth-only pass: discard transparent pixels, write nothing to colour.
            half4 fragOpaque(Varyings IN) : SV_Target
            {
                half4 texcol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                clip(texcol.a * IN.gColor.a - _Cutoff);
                return 0; // ColorMask 0 means this is never written anyway
            }
            ENDHLSL
        }

        // ─────────────────────────────────────────────────────────────
        // Pass 2 — Transparent blend (ZWrite Off, ZTest Equal)
        //
        // Renders all pixels with colour and blending. ZTest Equal means
        // only pixels that won the depth test in Pass 1 are shaded, so
        // blades never incorrectly blend over each other.
        // ─────────────────────────────────────────────────────────────
        Pass
        {
            Name "ForwardLitTransparent"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Cull Off
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _Color;
                half   _Cutoff;
                half   _BendByView;
                half   _GrassBottom;
                half   _GrassTop;
                half   _WaveSpeed;
                half   _WaveFrequency;
                half   _WaveAndDistortion;
                half4  _WavingTintLighten;
                half4  _WavingTintDarken;
            CBUFFER_END

            #include "WavingGrassCommon.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                half4  color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                half4  gColor      : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                float3 normalWS    : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.gColor = ApplyGrassDisplacement(IN.positionOS, IN.color * _Color);

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS  = posInputs.positionWS;
                OUT.normalWS    = nrmInputs.normalWS;
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texcol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half  alpha  = texcol.a * IN.gColor.a;

                half3 albedo   = texcol.rgb * IN.gColor.rgb;
                half3 normalWS = normalize(IN.normalWS);

                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light  mainLight   = GetMainLight(shadowCoord);

                // Two-sided wrap lighting
                half NdotL    = dot(normalWS, mainLight.direction);
                half wrapDiff = (abs(NdotL) + 1.0) * 0.5;
                half3 direct  = albedo * mainLight.color * wrapDiff * mainLight.shadowAttenuation;

                // Ambient — sample both normals for double-sided geometry
                half3 ambient     = SampleSH(normalWS)  * albedo;
                half3 ambientBack = SampleSH(-normalWS) * albedo;
                ambient = max(ambient, ambientBack);

                return half4(direct + ambient, alpha);
            }
            ENDHLSL
        }

        // ─────────────────────────────────────────────────────────────
        // Shadow Caster Pass
        // ─────────────────────────────────────────────────────────────
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            Cull Off
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex   vertShadow
            #pragma fragment fragShadow
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _Color;
                half   _Cutoff;
                half   _BendByView;
                half   _GrassBottom;
                half   _GrassTop;
                half   _WaveSpeed;
                half   _WaveFrequency;
                half   _WaveAndDistortion;
                half4  _WavingTintLighten;
                half4  _WavingTintDarken;
            CBUFFER_END

            #include "WavingGrassCommon.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct AttributesShadow
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                half4  color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VaryingsShadow
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            VaryingsShadow vertShadow(AttributesShadow IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                VaryingsShadow OUT;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                ApplyGrassDisplacement(IN.positionOS, IN.color * _Color);

                float3 posWS  = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normWS = TransformObjectToWorldNormal(IN.normalOS);

            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDir = normalize(_LightPosition - posWS);
            #else
                float3 lightDir = _LightDirection;
            #endif

                float4 posCS = TransformWorldToHClip(ApplyShadowBias(posWS, normWS, lightDir));

            #if UNITY_REVERSED_Z
                posCS.z = min(posCS.z, posCS.w * UNITY_NEAR_CLIP_VALUE);
            #else
                posCS.z = max(posCS.z, posCS.w * UNITY_NEAR_CLIP_VALUE);
            #endif

                OUT.positionHCS = posCS;
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 fragShadow(VaryingsShadow IN) : SV_Target
            {
                half4 texcol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                clip(texcol.a * _Color.a - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
