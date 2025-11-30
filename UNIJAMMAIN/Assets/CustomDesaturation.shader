Shader "Custom/Desaturation"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Saturation("Saturation", Range(0,1)) = 1
        _Color("Tint", Color) = (1,1,1,1)

            // UI 마스킹을 위한 필수 프로퍼티 (Stencil 등)
            _StencilComp("Stencil Comparison", Float) = 8
            _Stencil("Stencil ID", Float) = 0
            _StencilOp("Stencil Operation", Float) = 0
            _StencilWriteMask("Stencil Write Mask", Float) = 255
            _StencilReadMask("Stencil Read Mask", Float) = 255
            _ColorMask("Color Mask", Float) = 15
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Stencil
            {
                Ref[_Stencil]
                Comp[_StencilComp]
                Pass[_StencilOp]
                ReadMask[_StencilReadMask]
                WriteMask[_StencilWriteMask]
            }

            Cull Off
            Lighting Off
            ZWrite Off
            ZTest[unity_GUIZTestMode]
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask[_ColorMask]

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #include "UnityUI.cginc"

                struct appdata_t
                {
                    float4 vertex   : POSITION;
                    float4 color    : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                    float4 worldPosition : TEXCOORD1;
                };

                sampler2D _MainTex;
                fixed4 _Color;
                float _Saturation;

                v2f vert(appdata_t IN)
                {
                    v2f OUT;
                    OUT.worldPosition = IN.vertex;
                    OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                    OUT.texcoord = IN.texcoord;
                    OUT.color = IN.color * _Color;
                    return OUT;
                }

                fixed4 frag(v2f IN) : SV_Target
                {
                    fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                // 흑백 계산 (Luminance 공식 사용)
                float gray = dot(c.rgb, float3(0.299, 0.587, 0.114));

                // 원본 색상(c.rgb)과 흑백(gray) 사이를 Saturation 값으로 보간(Lerp)
                c.rgb = lerp(float3(gray, gray, gray), c.rgb, _Saturation);

                return c;
            }
            ENDCG
        }
        }
}