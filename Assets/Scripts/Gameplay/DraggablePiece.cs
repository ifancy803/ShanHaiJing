// DraggablePiece.cs
using UnityEngine;

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

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSortingOrder = spriteRenderer.sortingOrder;
    }


    public void OnDragStart()
    {
        // 提高渲染层级, 使其在拖拽时显示在最上层
        spriteRenderer.sortingOrder = sortingOrderWhileDragging;
    }


    public void OnDragging(Vector3 newPosition)
    {
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
            isLocked = true;
            Debug.Log($"拼图块 {pieceId} 已正确放置!");
        }
    }


    public bool IsLocked()
    {
        return isLocked;
    }
}