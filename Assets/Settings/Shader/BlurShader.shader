Shader "Custom/BlurShader"
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
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ BLUR_GAUSSIAN BLUR_RADIAL BLUR_DIRECTIONAL BLUR_ZOOM
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
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
                return o;
            }
            
            // 高斯模糊采样
            float4 GaussianBlur(sampler2D tex, float2 uv, float2 direction, float radius)
            {
                float4 color = float4(0, 0, 0, 0);
                float totalWeight = 0.0;
                
                for (int i = -_BlurQuality; i <= _BlurQuality; i++)
                {
                    float2 offset = direction * i * radius;
                    float weight = exp(-(i * i) / (2.0 * _BlurQuality * _BlurQuality));
                    color += tex2D(tex, uv + offset) * weight;
                    totalWeight += weight;
                }
                
                return color / totalWeight;
            }
            
            // 径向模糊
            float4 RadialBlur(sampler2D tex, float2 uv, float2 center, float strength)
            {
                float4 color = float4(0, 0, 0, 0);
                float2 direction = uv - center;
                float distance = length(direction);
                
                for (int i = 0; i < _BlurQuality; i++)
                {
                    float t = (float)i / (_BlurQuality - 1);
                    float2 sampleUV = center + direction * t * strength;
                    color += tex2D(tex, sampleUV);
                }
                
                return color / _BlurQuality;
            }
            
            // 方向模糊
            float4 DirectionalBlur(sampler2D tex, float2 uv, float2 direction, float radius)
            {
                float4 color = float4(0, 0, 0, 0);
                float2 normalizedDirection = normalize(direction);
                
                for (int i = -_BlurQuality; i <= _BlurQuality; i++)
                {
                    float2 offset = normalizedDirection * i * radius;
                    color += tex2D(tex, uv + offset);
                }
                
                return color / (_BlurQuality * 2 + 1);
            }
            
            // 缩放模糊
            float4 ZoomBlur(sampler2D tex, float2 uv, float2 center, float strength)
            {
                float4 color = float4(0, 0, 0, 0);
                float2 direction = uv - center;
                
                for (int i = 0; i < _BlurQuality; i++)
                {
                    float t = 1.0 + (float)i / (_BlurQuality - 1) * strength;
                    float2 sampleUV = center + direction / t;
                    color += tex2D(tex, sampleUV);
                }
                
                return color / _BlurQuality;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 originalColor = tex2D(_MainTex, uv);
                
                // 根据模糊类型选择不同的模糊算法
                float4 blurredColor = originalColor;
                
                if (_BlurType == 0) // 高斯模糊
                {
                    float2 texelSize = _MainTex_TexelSize.xy * _BlurRadius;
                    float4 horizontalBlur = GaussianBlur(_MainTex, uv, float2(1, 0), texelSize.x);
                    blurredColor = GaussianBlur(_MainTex, uv, float2(0, 1), texelSize.y);
                }
                else if (_BlurType == 1) // 径向模糊
                {
                    float2 center = float2(_CenterX, _CenterY);
                    blurredColor = RadialBlur(_MainTex, uv, center, _RadialStrength);
                }
                else if (_BlurType == 2) // 方向模糊
                {
                    float2 direction = float2(_DirectionX, _DirectionY);
                    float2 texelSize = _MainTex_TexelSize.xy * _BlurRadius;
                    blurredColor = DirectionalBlur(_MainTex, uv, direction, length(texelSize));
                }
                else if (_BlurType == 3) // 缩放模糊
                {
                    float2 center = float2(_CenterX, _CenterY);
                    blurredColor = ZoomBlur(_MainTex, uv, center, _RadialStrength);
                }
                
                // 应用模糊强度
                blurredColor = lerp(originalColor, blurredColor, _BlurAmount);
                
                // 应用整体强度
                blurredColor *= _Intensity;
                
                return blurredColor;
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
}
