Shader "Custom/InkSplash"
{
    Properties
    {
        _Color ("Tint Color", Color) = (1,1,1,1) // 纹理的整体颜色
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Ink Outline Color", Color) = (0,0,0,1) // 边缘水墨的颜色
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.05 // 边缘水墨的厚度 (调试时可以设大一点)
        _InkAmount ("Ink Amount", Range(0,1)) = 0 // 控制边缘水墨的扩散/显示程度 (0=无水墨, 1=全水墨)
        _InkSpreadSharpness ("Ink Spread Sharpness", Range(1, 30)) = 10 // 控制边缘水墨的锐度/平滑度
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha // 标准的透明度混合，让水墨可以叠加在原有内容上
        Cull Off // 关闭背面剔除，确保无论从哪个方向看都显示
        Lighting Off // 关闭光照，确保颜色不受场景光照影响

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog // 启用雾效编译（如果不需要可以删除，但通常无害）

            #include "UnityCG.cginc" // 包含Unity内置的CG函数库

            // 应用程序到顶点着色器的数据结构
            struct appdata
            {
                float4 vertex : POSITION;    // 顶点位置
                float2 uv : TEXCOORD0;       // UV坐标
                float4 color : COLOR;        // 顶点颜色 (SpriteRenderer会通过此项传递)
            };

            // 顶点着色器到片段着色器的数据结构
            struct v2f
            {
                float2 uv : TEXCOORD0;       // UV坐标
                UNITY_FOG_COORDS(1)          // 雾效坐标
                float4 vertex : SV_POSITION; // 裁剪空间中的顶点位置
                fixed4 color : COLOR;        // 顶点颜色
            };

            // 着色器属性（由Inspector设置，并在着色器中声明为全局变量）
            sampler2D _MainTex;
            float4 _MainTex_ST; // 纹理平铺和偏移
            fixed4 _Color;      // 纹理整体颜色
            fixed4 _OutlineColor; // 描边颜色
            float _OutlineWidth;  // 描边宽度
            float _InkAmount;     // 水墨量（0到1，控制水墨的显示强度）
            float _InkSpreadSharpness; // 水墨边缘的锐度

            // 顶点着色器函数
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // 将顶点从模型空间转换到裁剪空间
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);     // 应用纹理平铺和偏移
                UNITY_TRANSFER_FOG(o,o.vertex);           // 传递雾效数据
                o.color = v.color;                        // 传递顶点颜色
                return o;
            }

            // 片段着色器
            fixed4 frag (v2f i) : SV_Target
            {
                // 采样原始纹理，并应用Tint Color和顶点颜色
                fixed4 texColor = tex2D(_MainTex, i.uv) * _Color * i.color; 
                
                // ------------------ 最简单的边缘透明度控制 START ------------------

                // 1. 计算每个像素离最近的矩形边缘有多远
                float borderX = min(i.uv.x, 1.0 - i.uv.x); // 离左右边界的距离
                float borderY = min(i.uv.y, 1.0 - i.uv.y); // 离上下边界的距离
                float distToNearestEdge = min(borderX, borderY); // 离最近矩形边缘的距离

                float inkEffectAlpha = 0; // 默认水墨效果透明度为0 (无水墨)

                // 如果像素在 _OutlineWidth 定义的描边区域内
                if (distToNearestEdge < _OutlineWidth) 
                {
                    // **核心简化：让描边区域的透明度直接等于 _InkAmount**
                    inkEffectAlpha = _InkAmount; 
                    
                    // 确保透明度在0到1之间
                    inkEffectAlpha = saturate(inkEffectAlpha); 
                }
                
                // ------------------ 最简单的边缘透明度控制 END ------------------

                // 2. 将水墨效果 (描边颜色) 混合到最终颜色中
                // 只有 inkEffectAlpha > 0 的地方会显示 _OutlineColor
                fixed4 finalColor = texColor; // 默认是原始纹理颜色
                finalColor.rgb = lerp(finalColor.rgb, _OutlineColor.rgb, inkEffectAlpha); // 混合RGB颜色
                finalColor.a = lerp(finalColor.a, _OutlineColor.a, inkEffectAlpha);       // 混合Alpha值

                UNITY_APPLY_FOG(i.fogCoord, finalColor); // 应用雾效（如果启用）
                return finalColor;
            }
            ENDCG
        }
    }
}