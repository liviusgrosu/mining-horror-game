Shader "Custom/FlipbookUnlit"
{
    Properties
    {
        _MainTex ("Sprite Sheet", 2D) = "white" {}
        _Columns ("Columns", Float) = 4
        _Rows ("Rows", Float) = 4
        _Speed ("Speed (FPS)", Float) = 12
        [HDR] _Color ("Tint", Color) = (1,1,1,1)
        [Toggle] _UseAlpha ("Use Alpha Cutout", Float) = 0
        _AlphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "FlipbookPass"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back
            ZWrite On

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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Columns;
                float _Rows;
                float _Speed;
                float4 _Color;
                float _UseAlpha;
                float _AlphaCutoff;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

                float totalFrames = _Columns * _Rows;
                float frame = floor(fmod(_Time.y * _Speed, totalFrames));

                float col = fmod(frame, _Columns);
                float row = floor(frame / _Columns);

                float2 tiling = float2(1.0 / _Columns, 1.0 / _Rows);
                float2 offset = float2(col * tiling.x, 1.0 - tiling.y - row * tiling.y);

                output.uv = input.uv * tiling + offset;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 finalColor = texColor * _Color;

                if (_UseAlpha > 0.5)
                {
                    clip(finalColor.a - _AlphaCutoff);
                }

                return finalColor;
            }
            ENDHLSL
        }
    }
}
