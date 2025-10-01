using DG.Tweening.Plugins;
using UnityEngine;

public class DraggablePiece : MonoBehaviour
{
    public Vector3 correctPosition;
    
    private bool isLocked = false;
    
    private Vector3 offset;
    
    private int originalSortingOrder;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSortingOrder = spriteRenderer.sortingOrder;
    }

    void OnMouseDown()
    {
        if (!isLocked)
        {
            offset = transform.position - GetMouseWorldPos();
            
            spriteRenderer.sortingOrder = originalSortingOrder + 10;
        }
    }

    void OnMouseDrag()
    {
        if (!isLocked)
        {
            transform.position = GetMouseWorldPos() + offset;
        }
    }

    void OnMouseUp()
    {
        if (!isLocked)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
            
            float snapDistance = 0.5f; 
            if (Vector3.Distance(transform.position, correctPosition) < snapDistance)
            {

                transform.position = correctPosition;
                isLocked = true;
                
                // TODO:可以通知PuzzleManager，是否归位
                // FindObjectOfType<PuzzleManager>().PiecePlaced();
            }
        }
    }

    // 获取鼠标在世界坐标系中的位置
    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.nearClipPlane + 10; // z坐标需要设置，否则可能看不见
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}