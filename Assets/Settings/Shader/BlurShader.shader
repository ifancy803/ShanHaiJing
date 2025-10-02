Shader "Custom/UIBlurShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurAmount ("Blur Amount", Range(0, 10)) = 1.0
        _BlurRadius ("Blur Radius", Range(0, 5)) = 1.0
        _BlurQuality ("Blur Quality", Range(1, 8)) = 4
        _BlurType ("Blur Type", Int) = 0
        _CenterX ("Center X", Range(0, 1)) = 0.5
        _CenterY ("Center Y", Range(0, 1)) = 0.5
        _RadialStrength ("Radial Strength", Range(0, 5)) = 1.0
        _DirectionX ("Direction X", Range(-1, 1)) = 1.0
        _DirectionY ("Direction Y", Range(-1, 1)) = 0.0
        _Intensity ("Intensity", Range(0, 2)) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"          // 保证 UI 正确排序
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha   // ✅ 关键：透明混合

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;      // 支持 UI Tint
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            float _BlurAmount;
            float _BlurRadius;
            int _BlurQuality;
            int _BlurType;
            float _CenterX;
            float _CenterY;
            float _RadialStrength;
            float _DirectionX;
            float _DirectionY;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color; // UI 颜色传递
                return o;
            }

            // 示例：高斯模糊函数（保持不变）
            float4 GaussianBlur(sampler2D tex, float2 uv, float2 direction, float radius)
            {
                float4 color = float4(0, 0, 0, 0);
                float totalWeight = 0.0;

                for (int i = -_BlurQuality; i <= _BlurQuality; i++)
                {
                    float2 offset = direction * i * radius * _MainTex_TexelSize.xy;
                    float weight = exp(-(i * i) / (2.0 * _BlurQuality * _BlurQuality));
                    color += tex2D(tex, uv + offset) * weight;
                    totalWeight += weight;
                }

                return color / totalWeight;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 color = tex2D(_MainTex, i.uv);

                if (_BlurType == 0)
                {
                    color = GaussianBlur(_MainTex, i.uv, float2(1, 0), _BlurRadius);
                }

                color *= _BlurAmount * _Intensity;
                color.a = tex2D(_MainTex, i.uv).a; // 保持原始 alpha
                return color * i.color; // UI tint
            }
            ENDCG
        }
    }
}
