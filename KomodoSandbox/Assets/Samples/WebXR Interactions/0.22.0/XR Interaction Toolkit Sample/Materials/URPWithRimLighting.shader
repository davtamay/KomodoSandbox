Shader "Custom/SimpleLitRimShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(1, 6)) = 6
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
            Name "FORWARD"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : NORMAL;
                float3 viewDirWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            float4 _BaseColor;
            float4 _RimColor;
            float _RimPower;
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = SafeNormalize(_WorldSpaceCameraPos - TransformObjectToWorld(IN.positionOS).xyz);
                OUT.uv = IN.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return OUT;
            }

              half4 frag(Varyings IN) : SV_Target {
    float2 uv = IN.uv;
    half3 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb * _BaseColor.rgb;
    half3 normal = normalize(IN.normalWS);
    half3 viewDir = normalize(IN.viewDirWS);

     // Get main directional light data
   // Get main directional light data
    Light mainLight = GetMainLight();
    half3 lightDir = normalize(mainLight.direction);

    // Calculate the dot product for directional lighting
    half NdotL = max(dot(normal, lightDir), 0.0);

    // Since URP does not provide a direct way to access ambient light,
    // you can define a constant value for ambient light or adjust this approach based on your needs.
 half3 ambientLight = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
    // Apply directional lighting with correct intensity and add ambient light
    half3 litColor = (albedo * mainLight.color.rgb * NdotL) + (albedo * ambientLight);



    // Rim effect
    half rim = 1.0 - saturate(dot(viewDir, normal));
    rim = pow(rim, _RimPower);
    half edgeFactor = saturate(dot(normal, viewDir));
    rim *= edgeFactor;
    half3 rimColor = _RimColor.rgb * rim;

    // Combine lit color with rim effect
    half3 finalColor = litColor + rimColor;
    return half4(finalColor, 1.0);
}
           
            ENDHLSL
        }
    }
}

