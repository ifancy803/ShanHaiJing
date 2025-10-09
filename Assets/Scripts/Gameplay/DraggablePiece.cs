using UnityEngine;
using UnityEngine.EventSystems; // 引入EventSystems，虽然你的拖拽处理不是直接通过接口，但用于EventSystem.current.IsPointerOverGameObject()

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class DraggablePiece : MonoBehaviour
{
    public int pieceId;
    public Transform targetSlotTransform; // 目标卡槽的Transform

    [Tooltip("当拖拽时, 拼图块的渲染层级, 确保它在最顶层")]
    public int sortingOrderWhileDragging = 10;
    [Tooltip("拼图块与卡槽中心点的距离小于这个值时, 会自动吸附")]
    public float snapThreshold = 0.7f;

    private bool isLocked = false;
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;

    private PuzzleManager puzzleManager; 
    private PuzzlePieceInkEffect inkEffect; // 引用水墨效果脚本

    void Awake() // 将Start改为Awake，确保组件在Start之前被获取
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

    // 这将是你从 InputManager 或其他拖拽控制器中调用的函数
    public void OnDragStart()
    {
        // 确保没有UI覆盖在上面
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // 提高渲染层级, 使其在拖拽时显示在最上层
        spriteRenderer.sortingOrder = sortingOrderWhileDragging;

        // 调用水墨效果：开始扩散
        if (inkEffect != null)
        {
            inkEffect.StartInkSpread();
        }
    }

    // 这将是你从 InputManager 或其他拖拽控制器中调用的函数
    public void OnDragging(Vector3 newPosition)
    {
        // 确保没有UI覆盖在上面
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
    }

    // 这将是你从 InputManager 或其他拖拽控制器中调用的函数
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
            
            // 在成功吸附到目标时播放音效
            UIAudioManager.Instance.PlaySound(UIAudioManager.Instance.correctPlaceSound);

            // 只有当之前未锁定，现在才锁定，才执行逻辑并通知 Manager
            if (!isLocked) 
            {
                isLocked = true;
                Debug.Log($"拼图块 {pieceId} 已正确放置!");

                // 调用水墨效果：消散
                if (inkEffect != null)
                {
                    inkEffect.EndInkSpread();
                }

                // 通知 PuzzleManager 检查拼图完成状态
                if (puzzleManager != null)
                {
                    puzzleManager.CheckPuzzleCompletion();
                }
            }
            // 如果已经锁定但又拖拽了一次但还在吸附区域内，不重复调用 EndInkSpread
        }
        else
        {
            // 如果离开了目标吸附区域，并且之前是锁定状态，则解除锁定
            if (isLocked)
            {
                isLocked = false;
                Debug.Log($"拼图块 {pieceId} 已被移出卡槽，解除锁定。");
                gameObject.GetComponent<BoxCollider2D>().enabled = false; // 这行可能需要根据你的游戏逻辑调整
            }
            // 无论是否锁定，如果拖拽结束但没有吸附，水墨效果也应该消散
            if (inkEffect != null && !isLocked) // 只有当它没有被正确放置时才消散
            {
                inkEffect.EndInkSpread(); // 水墨消散
            }
        }
    }

    public bool IsLocked()
    {
        return isLocked;
    }
}