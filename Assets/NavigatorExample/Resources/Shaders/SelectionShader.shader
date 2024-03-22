Shader "Example/Selection Shader"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
        _GlowStrength ("Glow Strength", Range(0, 1)) = 0.5
        _GlowSpeed ("Glow Speed", Range(0, 1)) = 0.5
    }

    SubShader {
        Tags { "RenderType"="Opaque" }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GlowColor;
            float _GlowStrength;
            float _GlowSpeed;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float glow = abs(sin(_Time.y * _GlowSpeed));
                return lerp(tex2D(_MainTex, i.uv), _GlowColor, glow * _GlowStrength);
            }
            ENDCG
        }
    }
}

