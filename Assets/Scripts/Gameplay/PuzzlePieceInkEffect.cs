using UnityEngine;
using System.Collections; // 引入System.Collections，因为我们需要使用协程（IEnumerator）来实现动画

public class PuzzlePieceInkEffect : MonoBehaviour
{
    [Tooltip("水墨扩散或消散动画的持续时间")]
    public float inkSpreadDuration = 0.5f; 

    private Material pieceMaterial; // 引用拼图块的材质
    private Renderer spriteRenderer; // 引用Sprite Renderer组件

    void Awake() // 使用Awake确保在其他脚本的Start或OnDragStart之前获取组件
    {
        // 获取Sprite Renderer组件
        spriteRenderer = GetComponent<Renderer>(); 

        // 检查是否成功获取Renderer
        if (spriteRenderer != null)
        {
            // 获取Sprite Renderer使用的材质。
            // 注意：这里我们获取的是 'sharedMaterial' 的拷贝，以避免修改所有使用该材质的实例。
            // 或者，如果你确定每个拼图块都会有自己独立的材质实例，可以直接使用 .material
            pieceMaterial = spriteRenderer.material; 
        }
        else
        {
            // 如果没有SpriteRenderer，则尝试获取UI的Image组件（以防万一未来切换UI）
            UnityEngine.UI.Image image = GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                pieceMaterial = image.material;
            }
        }

        // 检查材质和_InkAmount属性是否存在
        if (pieceMaterial != null && pieceMaterial.HasProperty("_InkAmount"))
        {
            // 初始状态：水墨量为0 (拼图应该显示原始图片，没有水墨效果或透明)
            pieceMaterial.SetFloat("_InkAmount", 0);
        }
        else
        {
            Debug.LogError("错误: " + gameObject.name + " 上的材质或 '_InkAmount' 属性未找到! 请确保材质使用了 'Custom/InkSplash' Shader。");
        }
    }

    // 当开始拖拽时调用：水墨效果开始扩散（_InkAmount从0到1）
    public void StartInkSpread()
    {
        if (pieceMaterial != null && pieceMaterial.HasProperty("_InkAmount"))
        {
            StopAllCoroutines(); // 停止可能正在进行的动画，避免冲突
            StartCoroutine(AnimateInkAmount(0, 1)); // 启动协程，从0到1扩散水墨
        }
    }

    // 当拼图块放置就位或拖拽结束未放置时调用：水墨效果消散（_InkAmount从1到0）
    public void EndInkSpread()
    {
        if (pieceMaterial != null && pieceMaterial.HasProperty("_InkAmount"))
        {
            StopAllCoroutines(); // 停止可能正在进行的动画
            StartCoroutine(AnimateInkAmount(1, 0)); // 启动协程，从1到0消散水墨
        }
    }

    // 协程：动画 _InkAmount 属性从 startValue 到 endValue
    IEnumerator AnimateInkAmount(float startValue, float endValue)
    {
        float timer = 0f;
        while (timer < inkSpreadDuration)
        {
            // 使用Lerp进行平滑插值
            float currentValue = Mathf.Lerp(startValue, endValue, timer / inkSpreadDuration);
            pieceMaterial.SetFloat("_InkAmount", currentValue);
            timer += Time.deltaTime; // 增加计时器
            yield return null; // 等待下一帧
        }
        // 动画结束后，确保最终值设置正确，避免小误差
        pieceMaterial.SetFloat("_InkAmount", endValue); 
    }
}