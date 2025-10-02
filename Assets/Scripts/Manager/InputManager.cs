// InputManager.cs
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private DraggablePiece currentDraggedPiece;
    private Vector3 offset;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // --- 处理鼠标按下 ---
        if (Input.GetMouseButtonDown(0))
        {
            // 从摄像机发射一条射线
            RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                // 检查是否打中了带 DraggablePiece 脚本的物体
                DraggablePiece piece = hit.collider.GetComponent<DraggablePiece>();
                if (piece != null && !piece.IsLocked()) // 只有未锁定的拼图块才能被拖动
                {
                    currentDraggedPiece = piece;
                    // 调用拼图块自己的 OnDragStart 方法
                    currentDraggedPiece.OnDragStart(); 
                    
                    // 计算偏移
                    Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                    offset = currentDraggedPiece.transform.position - mousePos;
                }
            }
        }

        // --- 处理鼠标拖拽 ---
        if (Input.GetMouseButton(0) && currentDraggedPiece != null)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            // 调用拼图块自己的 OnDragging 方法
            currentDraggedPiece.OnDragging(mousePos + offset); 
        }

        // --- 处理鼠标抬起 ---
        if (Input.GetMouseButtonUp(0) && currentDraggedPiece != null)
        {
            // 调用拼图块自己的 OnDragEnd 方法
            currentDraggedPiece.OnDragEnd();
            currentDraggedPiece = null; // 清空当前拖拽的物体
        }
    }
}
