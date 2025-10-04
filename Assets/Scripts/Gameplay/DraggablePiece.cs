using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class DraggablePiece : MonoBehaviour
{
    public int pieceId;
    public Transform targetSlotTransform; // 目标卡槽的Transform
    private PuzzleSlotOutline targetSlotOutline; // 新增：引用目标卡槽的PuzzleSlotOutline脚本

    [Tooltip("当拖拽时, 拼图块的渲染层级, 确保它在最顶层")]
    public int sortingOrderWhileDragging = 10;
    [Tooltip("拼图块与卡槽中心点的距离小于这个值时, 会自动吸附")]
    public float snapThreshold = 0.7f;

    [Header("卡槽高亮设置")]
    public Color highlightColor = Color.green; // 拖拽时卡槽高亮的颜色
    [Range(0, 0.1f)]
    public float highlightOutlineWidth = 0.06f; // 拖拽时卡槽高亮的边框宽度

    private bool isLocked = false;
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;

    private PuzzleManager puzzleManager; 
    private PuzzlePieceInkEffect inkEffect; // 引用水墨效果脚本

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSortingOrder = spriteRenderer.sortingOrder;
    
        puzzleManager = FindFirstObjectByType<PuzzleManager>(); 
        inkEffect = GetComponent<PuzzlePieceInkEffect>(); // 获取水墨效果组件

        if (puzzleManager == null)
        {
            Debug.LogError("错误: 场景中未找到 PuzzleManager 实例! 无法检查拼图完成状态。");
        }
        if (inkEffect == null)
        {
            Debug.LogError("错误: 拼图块 " + gameObject.name + " 上未找到 PuzzlePieceInkEffect 脚本!");
        }
    }

    // 由 PuzzleManager 在创建时调用，设置对应的目标卡槽的 PuzzleSlotOutline 引用
    public void SetTargetSlotOutline(PuzzleSlotOutline slotOutline)
    {
        targetSlotOutline = slotOutline;
    }

    public void OnDragStart()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (isLocked) return; 

        spriteRenderer.sortingOrder = sortingOrderWhileDragging;

        if (inkEffect != null)
        {
            inkEffect.StartInkSpread();
        }

        // 高亮目标卡槽
        if (targetSlotOutline != null)
        {
            targetSlotOutline.HighlightSlot(highlightColor, highlightOutlineWidth);
        }
    }

    public void OnDragging(Vector3 newPosition)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (isLocked) return; 
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
    }

    public void OnDragEnd()
    {
        spriteRenderer.sortingOrder = originalSortingOrder;

        float distance = Vector2.Distance(transform.position, targetSlotTransform.position);

        if (distance <= snapThreshold)
        {
            transform.position = targetSlotTransform.position;
            
            if (!isLocked) 
            {
                isLocked = true;
                Debug.Log($"拼图块 {pieceId} 已正确放置!");

                if (inkEffect != null)
                {
                    inkEffect.EndInkSpread(); // 拼图块吸附后，水墨消散
                }

                if (puzzleManager != null)
                {
                    puzzleManager.CheckPuzzleCompletion();
                }

                if (targetSlotOutline != null)
                {
                    targetSlotOutline.ResetAppearance(); // 卡槽恢复默认外观
                }
            }
        }
        else
        {
            if (isLocked)
            {
                isLocked = false;
                Debug.Log($"拼图块 {pieceId} 已被移出卡槽，解除锁定。");
            }

            if (inkEffect != null)
            {
                inkEffect.EndInkSpread(); // 拖拽结束但未吸附，水墨消散
            }

            if (targetSlotOutline != null)
            {
                targetSlotOutline.ResetAppearance(); // 卡槽恢复默认外观
            }
        }
    }

    public bool IsLocked()
    {
        return isLocked;
    }
}