using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PuzzleSlotOutline : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Material slotMaterialInstance; // 用于保存实例化的材质

    [Header("卡槽外观设置")]
    [Tooltip("拖拽到此处的Material必须使用 'Custom/SlotOutlineShader'")]
    public Material baseSlotMaterial; // 在Inspector中拖入你创建的 SlotOutlineMaterial
    
    public Color defaultInnerColor = new Color(0.5f, 0.5f, 0.5f, 0.3f); // 默认内部半透明灰色
    public Color defaultOutlineColor = Color.black; // 默认边框颜色
    [Range(0, 0.1f)]
    public float defaultOutlineWidth = 0.03f; // 默认边框宽度

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 实例化材质，防止多个卡槽共享同一个材质实例导致互相影响
        if (baseSlotMaterial != null)
        {
            slotMaterialInstance = new Material(baseSlotMaterial);
            spriteRenderer.material = slotMaterialInstance; // 将实例化的材质赋给SpriteRenderer
        }
        else
        {
            Debug.LogWarning($"PuzzleSlotOutline on {gameObject.name}: 'Base Slot Material' is not assigned. Using default SpriteRenderer material.");
            // 如果没有指定材质，SpriteRenderer将使用默认的Sprite-Default材质
        }

        // 应用初始设置
        ApplyAppearance(defaultInnerColor, defaultOutlineColor, defaultOutlineWidth);
    }

    /// <summary>
    /// 设置卡槽的视觉效果
    /// </summary>
    /// <param name="innerColor">卡槽内部颜色</param>
    /// <param name="outlineColor">边框颜色</param>
    /// <param name="outlineWidth">边框宽度 (0到0.1)</param>
    public void ApplyAppearance(Color innerColor, Color outlineColor, float outlineWidth)
    {
        if (slotMaterialInstance != null && slotMaterialInstance.shader.name == "Custom/SlotOutlineShader")
        {
            slotMaterialInstance.SetColor("_Color", innerColor);
            slotMaterialInstance.SetColor("_OutlineColor", outlineColor);
            slotMaterialInstance.SetFloat("_OutlineWidth", outlineWidth);
        }
        else if (slotMaterialInstance == null)
        {
            Debug.LogWarning($"PuzzleSlotOutline on {gameObject.name}: Material instance is null. Cannot apply appearance.");
        }
        else
        {
            Debug.LogWarning($"PuzzleSlotOutline on {gameObject.name}: Material's shader is not 'Custom/SlotOutlineShader'. Cannot apply custom properties.");
        }
    }

    /// <summary>
    /// 当拼图块进入卡槽范围时高亮卡槽
    /// </summary>
    public void HighlightSlot(Color highlightColor, float highlightWidth)
    {
        if (slotMaterialInstance != null && slotMaterialInstance.shader.name == "Custom/SlotOutlineShader")
        {
            slotMaterialInstance.SetColor("_OutlineColor", highlightColor);
            slotMaterialInstance.SetFloat("_OutlineWidth", highlightWidth);
        }
    }

    /// <summary>
    /// 恢复卡槽到默认外观
    /// </summary>
    public void ResetAppearance()
    {
        ApplyAppearance(defaultInnerColor, defaultOutlineColor, defaultOutlineWidth);
    }

    void OnDestroy()
    {
        // 清理实例化的材质，避免内存泄漏
        if (slotMaterialInstance != null)
        {
            Destroy(slotMaterialInstance);
        }
    }
}