using UnityEngine;
using UnityEngine.EventSystems;

// using UnityEngine.Events; // 如果你不需要DraggablePiece自己也触发事件，可以不引入

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class DraggablePiece : MonoBehaviour
{
    public int pieceId;
    public Transform targetSlotTransform;

    [Tooltip("当拖拽时, 拼图块的渲染层级, 确保它在最顶层")]
    public int sortingOrderWhileDragging = 10;
    [Tooltip("拼图块与卡槽中心点的距离小于这个值时, 会自动吸附")]
    public float snapThreshold = 0.7f;

    private bool isLocked = false;
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;

    // 引用 PuzzleManager，以便在放置时通知它
    private PuzzleManager puzzleManager; 

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSortingOrder = spriteRenderer.sortingOrder;
    
        puzzleManager = FindFirstObjectByType<PuzzleManager>(); 

        if (puzzleManager == null)
        {
            Debug.LogError("错误: 场景中未找到 PuzzleManager 实例! 无法检查拼图完成状态。");
        }
    }


    public void OnDragStart()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        // 提高渲染层级, 使其在拖拽时显示在最上层
        spriteRenderer.sortingOrder = sortingOrderWhileDragging;
    }


    public void OnDragging(Vector3 newPosition)
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
    }


    public void OnDragEnd()
    {
        // 恢复原始渲染层级
        spriteRenderer.sortingOrder = originalSortingOrder;

        // 检查与目标卡槽的距离
        float distance = Vector2.Distance(transform.position, targetSlotTransform.position);

        if (distance <= snapThreshold)
        {
            // 距离足够近, 自动吸附到卡槽位置
            transform.position = targetSlotTransform.position;
            
            // 只有当之前未锁定，现在才锁定，才执行逻辑并通知 Manager
            if (!isLocked) 
            {
                isLocked = true;
                Debug.Log($"拼图块 {pieceId} 已正确放置!");

                // 通知 PuzzleManager 检查拼图完成状态
                if (puzzleManager != null)
                {
                    puzzleManager.CheckPuzzleCompletion();
                }
            }
        }
        else
        {
            // 如果离开了目标吸附区域，并且之前是锁定状态，则解除锁定
            if (isLocked)
            {
                isLocked = false;
                Debug.Log($"拼图块 {pieceId} 已被移出卡槽，解除锁定。");
                gameObject.GetComponent<BoxCollider2D>().enabled = false;
            }
        }
    }


    public bool IsLocked()
    {
        return isLocked;
    }
}