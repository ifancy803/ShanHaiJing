Shader "Custom/InkSplash"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.01
        _InkAmount ("Ink Amount", Range(0,1)) = 0 // 控制水墨扩散程度
        _InkSpreadSharpness ("Ink Spread Sharpness", Range(1, 30)) = 10 // 控制水墨边缘锐度
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        // 确保混合模式正确处理透明度
        Blend SrcAlpha OneMinusSrcAlpha // 这是标准的透明度混合
        // 如果你的图片本身没有透明度，或者你希望水墨效果是额外的叠加，可以考虑：
        // Blend One OneMinusSrcAlpha // 叠加模式，水墨像墨汁一样“覆盖”在图片上
        // Blend One Zero // 不透明模式，直接替换原有颜色

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _OutlineColor;
            float _OutlineWidth;
            float _InkAmount;
            float _InkSpreadSharpness; // 新增的锐度属性

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 采样原始纹理
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                // 计算水墨效果的alpha值
                float dist = distance(i.uv, float2(0.5, 0.5)); // 距离中心点的UV距离
                
                // 这里的逻辑需要改变：
                // 当_InkAmount为0时，inkEffectAlpha应该是0，不影响col.a
                // 当_InkAmount为1时，inkEffectAlpha应该根据dist和sharpness来计算，形成水墨效果
                
                // 新的水墨效果计算
                // 这个表达式的目的是：当_InkAmount越大，inkEffectAlpha才越大
                // 并且通过_InkSpreadSharpness控制扩散的边缘
                float inkEffectAlpha = saturate((_InkAmount - (dist * (1 + (1 - _InkAmount) * 0.5))) * _InkSpreadSharpness);

                // 最终的alpha = 原始图片alpha * (1 - inkEffectAlpha) + inkEffectAlpha
                // 这样当inkEffectAlpha为0时，最终alpha = 原始图片alpha
                // 当inkEffectAlpha为1时，最终alpha = 1 (完全不透明的水墨)
                // 这将使水墨效果像覆盖在原图上一样逐渐显现
                col.a = lerp(col.a, 1.0, inkEffectAlpha); // 将原始alpha与1（水墨部分不透明）进行插值

                // 如果你的原始图片本身没有透明度（完全不透明），并且你希望水墨是一种“替换”效果，
                // 可以直接 col.a = inkEffectAlpha; 但这会导致_InkAmount=0时仍然透明。
                // 因此上面的 lerp 是一个更好的通用方案。

                // 描边效果 (保持不变)
                float outline = 0;
                if(i.uv.x < _OutlineWidth || i.uv.x > (1 - _OutlineWidth) ||
                   i.uv.y < _OutlineWidth || i.uv.y > (1 - _OutlineWidth))
                {
                    outline = 1;
                }
                // 如果水墨效果不透明，描边就用描边色
                // 否则，描边是原始颜色和描边色的混合
                col = lerp(col, _OutlineColor, outline); 


                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}