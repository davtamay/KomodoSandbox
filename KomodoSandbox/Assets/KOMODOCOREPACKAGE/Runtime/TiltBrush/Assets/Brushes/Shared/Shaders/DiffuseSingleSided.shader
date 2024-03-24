Shader "Universal Render Pipeline/Unlit/BrushDiffuseSingleSided"
{
    Properties
    {
        _BaseMap ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="Geometry+1" }
        LOD 200
        Cull Back

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On // Enable ZWrite
        AlphaToMask On

        Pass
        {
            Name "UnlitPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 position : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.position = TransformObjectToHClip(IN.position.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            half4 _Color;
            half _Cutoff;

            float4 frag(Varyings IN) : SV_Target
            {
                float4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _Color;
                baseColor.rgb *= IN.color.rgb;
                baseColor.a *= IN.color.a;
                clip(baseColor.a - _Cutoff);
                return baseColor;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
