Shader "Universal Render Pipeline/Unlit/AnimalRuler"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _Saturation ("Saturation", Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        AlphaToMask On
        Cull Off ZWrite Off

        Pass
        {
            Name "UnlitPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half _Saturation;
            half _Cutoff;
            float4 _BaseMap_ST; // Add this line for tiling and offset
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap); // Modify this line for tiling and offset
                return OUT;
            }

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            half4 frag(Varyings IN) : SV_Target
            {
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                baseColor.rgb *= _Color.rgb;
                baseColor.rgb = lerp(half3(1,1,1), baseColor.rgb, _Saturation);
                clip(baseColor.a - _Cutoff);
                return baseColor;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
