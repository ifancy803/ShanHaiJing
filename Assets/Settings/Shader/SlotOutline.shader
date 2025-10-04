Shader "Custom/SlotOutlineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // 可以用于一些背景纹理，如果不需要可以忽略
        _Color ("Inner Color", Color) = (0.5,0.5,0.5,0.3) // 卡槽内部颜色 (半透明灰色)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1) // 边框颜色 (黑色)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.03 // 边框宽度 (占UV的比例)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        // 启用透明混合
        Blend SrcAlpha OneMinusSrcAlpha 

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color;
            float4 _OutlineColor;
            float _OutlineWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv); // 如果没有贴图，这将是白色

                // 计算边框
                float outlineFactor = 0;
                if(i.uv.x < _OutlineWidth || i.uv.x > (1.0 - _OutlineWidth) ||
                   i.uv.y < _OutlineWidth || i.uv.y > (1.0 - _OutlineWidth))
                {
                    outlineFactor = 1; // 在边框区域内
                }
                
                // lerp(a, b, t) = a * (1-t) + b * t
                // 当 outlineFactor 为 0 时，使用 _Color (内部颜色) * texColor
                // 当 outlineFactor 为 1 时，使用 _OutlineColor (边框颜色)
                fixed4 finalColor = lerp(_Color * texColor, _OutlineColor, outlineFactor);
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}