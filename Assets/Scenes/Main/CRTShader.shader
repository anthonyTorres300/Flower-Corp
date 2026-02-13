Shader "Custom/CRTShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScanlineSize ("Scanline Size", Range(0, 1000)) = 100.0
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.5
        _Curvature ("Curvature", Range(0, 1)) = 0.1
        _Aberration ("Chromatic Aberration", Range(0, 0.05)) = 0.005
        _Vignette ("Vignette", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off

        Pass
        {
            Name "CRT"
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
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float _ScanlineSize;
            float _ScanlineIntensity;
            float _Curvature;
            float _Aberration;
            float _Vignette;

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            // Curve function to warp UVs
            float2 CurveUV(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                float2 offset = abs(uv.yx) / float2(5.0, 5.0);
                uv = uv + uv * offset * offset * _Curvature;
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            half4 frag (Varyings input) : SV_Target
            {
                // 1. Warp the UV coordinates
                float2 uv = CurveUV(input.uv);

                // Check if UV is outside screen (black borders)
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    return half4(0, 0, 0, 1);

                // 2. Chromatic Aberration (Split RGB channels)
                float4 color;
                color.r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv + float2(_Aberration, 0)).r;
                color.g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv).g;
                color.b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv - float2(_Aberration, 0)).b;
                color.a = 1.0;

                // 3. Scanlines (Sine wave pattern)
                float scanline = sin(uv.y * _ScanlineSize * 3.14159 * 2.0);
                // Convert sine wave (-1 to 1) to a darken factor (0 to 1)
                scanline = (scanline + 1.0) * 0.5; 
                // Mix between full color and darkened scanline
                color.rgb -= scanline * _ScanlineIntensity;

                // 4. Vignette (Darken corners)
                float2 dist = uv - 0.5;
                float vign = 1.0 - dot(dist, dist) * _Vignette * 2.0;
                color.rgb *= vign;

                return color;
            }
            ENDHLSL
        }
    }
}