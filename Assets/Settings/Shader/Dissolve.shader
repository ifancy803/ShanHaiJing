Shader "TextMeshPro/Dissolve"
{
    Properties
    {
        _MainTex ("Font Atlas", 2D) = "white" {}
        _FaceColor ("Face Color", Color) = (1,1,1,1)
        _FaceDilate ("Face Dilate", Range(-1,1)) = 0
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Thickness", Range(0,1)) = 0
        _OutlineSoftness ("Outline Softness", Range(0,1)) = 0
        _UnderlayColor ("Border Color", Color) = (0,0,0,0.5)
        _UnderlayOffsetX ("Border OffsetX", Range(-1,1)) = 0
        _UnderlayOffsetY ("Border OffsetY", Range(-1,1)) = 0
        _UnderlayDilate ("Border Dilate", Range(-1,1)) = 0
        _UnderlaySoftness ("Border Softness", Range(0,1)) = 0
        
        // 溶解属性
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.02
        _EdgeColor ("Edge Color", Color) = (1, 0.5, 0, 1)
        _EdgeIntensity ("Edge Intensity", Range(0, 10)) = 3
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.3
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ OUTLINE_ON
            #pragma multi_compile __ UNDERLAY_ON UNDERLAY_INNER

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            sampler2D _MainTex;
            fixed4 _FaceColor;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _OutlineSoftness;
            float _FaceDilate;
            fixed4 _UnderlayColor;
            float _UnderlayOffsetX;
            float _UnderlayOffsetY;
            float _UnderlayDilate;
            float _UnderlaySoftness;
            
            // 溶解属性
            sampler2D _NoiseTex;
            float _DissolveAmount;
            float _EdgeWidth;
            fixed4 _EdgeColor;
            float _EdgeIntensity;
            float _NoiseStrength;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.texcoord0 = v.texcoord0;
                o.texcoord1 = v.texcoord1;
                o.uv = v.texcoord0;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 原有的TMP文字渲染
                fixed4 color = tex2D(_MainTex, i.texcoord0);
                color *= i.color * _FaceColor;
                
                // 轮廓效果
                #ifdef OUTLINE_ON
                fixed4 outlineColor = _OutlineColor;
                outlineColor.a *= color.a;
                color = outlineColor;
                #endif
                
                // 如果alpha为0，直接返回
                if (color.a < 0.001)
                    return fixed4(0, 0, 0, 0);
                
                // 溶解效果
                fixed noise = tex2D(_NoiseTex, i.uv).r;
                float baseDissolve = i.uv.y; // 自下而上
                float dissolve = baseDissolve * (1.0 - _NoiseStrength) + noise * _NoiseStrength;
                dissolve = saturate(dissolve);
                
                float dissolveClip = dissolve - _DissolveAmount;
                float edge = smoothstep(0.0, _EdgeWidth, dissolveClip);
                float edgeGlow = 1.0 - smoothstep(0.0, _EdgeWidth, dissolveClip);
                
                if (dissolveClip < 0)
                    discard;
                
                fixed4 edgeColor = _EdgeColor * _EdgeIntensity * edgeGlow;
                fixed4 finalColor = color + edgeColor;
                finalColor.a = color.a * edge;
                
                return finalColor;
            }
            ENDCG
        }
    }

    CustomEditor "TMPro.EditorUtilities.TMP_SDFShaderGUI"
    FallBack "TextMeshPro/Mobile/Distance Field"
}