using UnityEngine;
using System.Collections.Generic;

// 确保主摄像机有这个脚本时，它也有一个 LineRenderer 组件
[RequireComponent(typeof(LineRenderer))]
public class ShanhaijingMouseTrail : MonoBehaviour
{
    [Header("水墨拖尾设置")]
    [Tooltip("拖尾的材质 (用于渲染水墨效果)")]
    public Material trailMaterial;

    [Tooltip("拖尾的最大长度 (世界单位)")]
    public float maxTrailLength = 5.0f; 

    [Tooltip("拖尾点之间的最小距离 (世界单位)，防止点过于密集")]
    public float minDistanceBetweenPoints = 0.1f; 

    [Tooltip("拖尾的起始宽度 (世界单位)")]
    public float startWidth = 0.2f; 
    [Tooltip("拖尾的结束宽度 (世界单位)")]
    public float endWidth = 0.05f; 

    // 新增：拖尾淡出速度，当鼠标停止移动时使用
    [Tooltip("拖尾在鼠标停止移动时淡出和缩短的速度")]
    public float fadeOutSpeed = 2.0f; // 每秒淡出多少长度

    [Tooltip("水墨颜色 (主色调)")]
    public Color inkColor = new Color(0.2f, 0.2f, 0.2f, 0.7f); // 深灰色，带一些透明度
    [Tooltip("水墨晕染的颜色 (在拖尾末端或透明度变化时体现)")]
    public Color fadeInkColor = new Color(0.5f, 0.5f, 0.5f, 0.0f); // 浅灰色，完全透明

    [Tooltip("拖尾点的Z轴深度，确保它在场景物体之上显示")]
    public float trailZDepth = 10f; 
    
    // 新增：判断鼠标是否移动的阈值
    private Vector3 lastMousePositionScreen; // 记录上一帧鼠标的屏幕坐标
    [Tooltip("鼠标停止移动的判断阈值，低于此值则认为停止移动")]
    public float mouseStopThreshold = 0.01f; 

    [Header("山海经主题元素")]
    public bool enableShanhaijingElements = false;
    public GameObject[] shanhaijingElementPrefabs;
    [Range(0f, 1f)]
    public float elementSpawnChance = 0.1f;
    public Vector2 elementScaleRange = new Vector2(0.1f, 0.3f);
    public float elementLifetime = 2.0f;


    private LineRenderer lineRenderer;
    private List<Vector3> trailPoints = new List<Vector3>();
    private Camera mainCamera;
    private Vector3 lastMouseWorldPosition;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        mainCamera = Camera.main; 

        if (mainCamera == null)
        {
            Debug.LogError("场景中没有找到Main Camera。请确保你的摄像机被标记为 'MainCamera'。");
            enabled = false; 
            return;
        }

        SetupLineRenderer();
        lastMousePositionScreen = Input.mousePosition; // 初始化
    }

    void SetupLineRenderer()
    {
        lineRenderer.material = trailMaterial; 
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.numCapVertices = 8; // 增加顶点数使线条更平滑
        lineRenderer.numCornerVertices = 8;
        lineRenderer.useWorldSpace = true; 
        lineRenderer.textureMode = LineTextureMode.Tile; 
        lineRenderer.sortingOrder = 10; // 确保在大部分2D物体之上

        // 设置颜色渐变，模拟水墨浓淡变化和透明度变化
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(inkColor, 0.0f), new GradientColorKey(inkColor, 0.7f), new GradientColorKey(fadeInkColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(inkColor.a, 0.0f), new GradientAlphaKey(inkColor.a, 0.7f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;
    }

    void Update()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, trailZDepth));

        // 判断鼠标是否移动
        bool isMouseMoving = (Vector3.Distance(mouseScreenPos, lastMousePositionScreen) > mouseStopThreshold);
        lastMousePositionScreen = mouseScreenPos; // 更新上一帧鼠标位置

        if (isMouseMoving)
        {
            // 如果鼠标移动，则添加新点
            if (Vector3.Distance(mouseWorldPos, lastMouseWorldPosition) > minDistanceBetweenPoints)
            {
                trailPoints.Add(mouseWorldPos);
                lastMouseWorldPosition = mouseWorldPos;

                // 随机生成山海经元素
                if (enableShanhaijingElements && shanhaijingElementPrefabs != null && shanhaijingElementPrefabs.Length > 0)
                {
                    if (Random.value < elementSpawnChance)
                    {
                        SpawnShanhaijingElement(mouseWorldPos);
                    }
                }
            }
        }
        else // 鼠标停止移动时
        {
            // 逐渐缩短拖尾，模拟消失
            if (trailPoints.Count > 0)
            {
                // 计算需要移除多少长度的点
                float lengthToRemove = fadeOutSpeed * Time.deltaTime;
                float currentLength = 0f;
                int pointsRemoved = 0;

                // 从拖尾的头部开始移除点
                for (int i = 0; i < trailPoints.Count - 1; i++)
                {
                    currentLength += Vector3.Distance(trailPoints[i], trailPoints[i + 1]);
                    if (currentLength >= lengthToRemove)
                    {
                        pointsRemoved = i + 1; // 移除到第 i+1 个点
                        break;
                    }
                }
                
                if (pointsRemoved > 0)
                {
                    trailPoints.RemoveRange(0, pointsRemoved);
                }
                // 如果只剩下一个点，直接清空
                if (trailPoints.Count == 1 && lengthToRemove >= 0) // 或者根据需要设置一个更小的阈值
                {
                    trailPoints.Clear();
                }
            }
        }

        // 限制拖尾最大长度 (无论鼠标是否移动)
        while (CalculateTrailLength() > maxTrailLength && trailPoints.Count > 1)
        {
            trailPoints.RemoveAt(0);
        }
        
        // 更新LineRenderer
        if (trailPoints.Count > 0)
        {
            lineRenderer.positionCount = trailPoints.Count;
            lineRenderer.SetPositions(trailPoints.ToArray());
        }
        else
        {
            lineRenderer.positionCount = 0; 
        }
    }

    float CalculateTrailLength()
    {
        float length = 0f;
        for (int i = 0; i < trailPoints.Count - 1; i++)
        {
            length += Vector3.Distance(trailPoints[i], trailPoints[i + 1]);
        }
        return length;
    }

    void SpawnShanhaijingElement(Vector3 position)
    {
        if (shanhaijingElementPrefabs == null || shanhaijingElementPrefabs.Length == 0) return;

        GameObject elementPrefab = shanhaijingElementPrefabs[Random.Range(0, shanhaijingElementPrefabs.Length)];
        GameObject spawnedElement = Instantiate(elementPrefab, position, Quaternion.identity);
        
        spawnedElement.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));

        float randomScale = Random.Range(elementScaleRange.x, elementScaleRange.y);
        spawnedElement.transform.localScale = new Vector3(randomScale, randomScale, 1f);

        SpriteRenderer elementSpriteRenderer = spawnedElement.GetComponent<SpriteRenderer>();
        if (elementSpriteRenderer != null)
        {
            elementSpriteRenderer.sortingOrder = lineRenderer.sortingOrder + 1; 
        }

        Destroy(spawnedElement, elementLifetime);
    }
}