using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random; // 引入 Random 命名空间是为了明确使用 UnityEngine 的 Random

public class PuzzleManager : MonoBehaviour
{
    public List<Texture2D> sourceImageNums; // 在 Inspector 中设置的图片列表
    private Dictionary<int,Texture2D> sourceImageToIndexNums; // 运行时使用的字典

    [Header("拼图基础设置")]
    [Tooltip("拼图的行数")]
    public int rows = 3;
    [Tooltip("拼图的列数")]
    public int cols = 3;

    private Texture2D image; // 实际用于生成拼图的图片

    private int imgIndex = 0;

    public float PPUvalue = 0f;

    [Header("预制体 (Prefabs)")]
    [Tooltip("拼图块的预制体 (必须带SpriteRenderer和DraggablePiece脚本)")]
    public GameObject piecePrefab;
    [Tooltip("卡槽的预制体 (必须带有SpriteRenderer和PuzzleSlot脚本)")]
    public GameObject slotPrefab;

    [Header("场景中的父物体")]
    [Tooltip("一个空物体, 用来存放所有生成的拼图块, 保持场景整洁")]
    public Transform pieceAreaParent;
    [Tooltip("一个空物体, 用来存放所有生成的卡槽, 作为网格的中心")]
    public Transform slotAreaParent;

    [Header("拼图块随机生成区域")]
    [Tooltip("一个空物体, 标记随机区域的中心点")]
    public Transform spawnAreaCenter;
    [Tooltip("随机区域的尺寸 (宽和高)")]
    public Vector2 spawnAreaSize = new Vector2(5, 5);

    [Header("广播")] 
    public IntEventSO afterPuzzleCompletionEvent;
    

    // 运行时使用的私有列表
    private List<DraggablePiece> draggablePieces = new List<DraggablePiece>(); // 存储所有生成的 DraggablePiece
    private List<PuzzleSlot> puzzleSlots = new List<PuzzleSlot>();

    void Awake()
    {
        // === 修复点在这里！ ===
        // 在使用字典之前，先实例化它
        sourceImageToIndexNums = new Dictionary<int, Texture2D>();
        // ===================

        int index = 0;
        // 遍历 Inspector 中设置的 sourceImageNums 列表，填充到字典中
        foreach(Texture2D img in sourceImageNums) // 将循环变量名从 sourceImage 改为 img，避免与 public sourceImage 混淆
        {
            if (img != null) // 检查图片是否为空，防止列表中有空槽
            {
                sourceImageToIndexNums.Add(index, img);
            }
            else
            {
                Debug.LogWarning($"sourceImageNums 列表中索引 {index} 处的图片为空，已跳过添加到字典。");
            }
            index++; // 无论图片是否为空，索引都应该递增
        }

        if (sourceImageToIndexNums.ContainsKey(imgIndex))
        {
            image = sourceImageToIndexNums[imgIndex];
        }
        
        
    }

    void Start()
    {
        // 检查所有必要的引用是否已在编辑器中设置好
        // 注意：这里 AreReferencesAssigned() 检查的是 image 字段，而不是最初的 sourceImage 字段。
        // 这是更合理的，因为 image 才是真正用于生成拼图的图片。
        if (!AreReferencesAssigned())
        {
            return; // 如果有引用未设置, 则停止运行, 防止报错
        }

        GeneratePuzzle();
    }

    /// <summary>
    /// 生成所有的卡槽和拼图块
    /// </summary>
    void GeneratePuzzle()
    {
        // 清理旧的拼图块和卡槽
        foreach (Transform child in pieceAreaParent) Destroy(child.gameObject);
        foreach (Transform child in slotAreaParent) Destroy(child.gameObject);
        
        puzzleSlots.Clear();
        draggablePieces.Clear(); // 清空列表，准备存储新的拼图块

        
        
        int pieceWidth = image.width / cols ;
        int pieceHeight = image.height / rows ;

        float pieceWorldWidth = (float)pieceWidth / PPUvalue; // 假设 PPU 是 100，即 100 像素 = 1 世界单位
        float pieceWorldHeight = (float)pieceHeight / PPUvalue;

        float gridWorldWidth = cols * pieceWorldWidth;
        float gridWorldHeight = rows * pieceWorldHeight;
        float startX = slotAreaParent.position.x - gridWorldWidth / 2f + pieceWorldWidth / 2f;
        float startY = slotAreaParent.position.y + gridWorldHeight / 2f - pieceWorldHeight / 2f;

        int pieceCounter = 0; // 计数器, 同时作为ID


        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector3 slotPosition = new Vector3(startX + x * pieceWorldWidth, startY - y * pieceWorldHeight, 0);
                GameObject slotObject = Instantiate(slotPrefab, slotPosition, Quaternion.identity, slotAreaParent);
                slotObject.name = $"Slot_{x}_{y}";

                SpriteRenderer sr = slotObject.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.drawMode = SpriteDrawMode.Sliced; // 将绘制模式设置为 Sliced，这样修改 size 才有效
                    sr.size = new Vector2(pieceWorldWidth, pieceWorldHeight); // 设置卡槽尺寸
                }
                
                PuzzleSlot slot = slotObject.GetComponent<PuzzleSlot>();
                slot.slotId = pieceCounter;
                puzzleSlots.Add(slot);
                
                
                // 从当前选中的 image 中裁剪出像素
                Color[] pixels = image.GetPixels(x * pieceWidth, (rows - 1 - y) * pieceHeight, pieceWidth, pieceHeight);
                Texture2D pieceTexture = new Texture2D(pieceWidth, pieceHeight);
                pieceTexture.SetPixels(pixels);
                pieceTexture.Apply();

                Sprite pieceSprite = Sprite.Create(pieceTexture, new Rect(0, 0, pieceWidth, pieceHeight), new Vector2(0.5f, 0.5f), PPUvalue);

                // 随机放置拼图块
                float randomX = Random.Range(spawnAreaCenter.position.x - spawnAreaSize.x / 2, spawnAreaCenter.position.x + spawnAreaSize.x / 2);
                float randomY = Random.Range(spawnAreaCenter.position.y - spawnAreaSize.y / 2, spawnAreaCenter.position.y + spawnAreaSize.y / 2);
                GameObject pieceObject = Instantiate(piecePrefab, new Vector3(randomX, randomY, 0), Quaternion.identity, pieceAreaParent);
                pieceObject.name = $"Piece_{x}_{y}";
                
                pieceObject.GetComponent<SpriteRenderer>().sprite = pieceSprite;

                //BoxCollider2D 的大小
                BoxCollider2D pieceCollider = pieceObject.GetComponent<BoxCollider2D>();
                if (pieceCollider != null)
                {
                    // 确保碰撞器没有自动缩放
                    pieceCollider.autoTiling = false; 
                    pieceCollider.offset = Vector2.zero; // 确保碰撞器的中心点与GameObject的中心点对齐
                    
                    float colliderPadding = 0.1f; // 增加边距
                    pieceCollider.size = new Vector2(pieceWorldWidth + colliderPadding, pieceWorldHeight + colliderPadding); 
                    
                }
                else
                {
                    Debug.LogWarning($"Piece_{x}_{y} 缺少 BoxCollider2D 组件，无法动态调整碰撞范围。");
                }
                
                
                
                pieceObject.GetComponent<SpriteRenderer>().sprite = pieceSprite;

                // 关联拼图块和对应的卡槽
                DraggablePiece draggable = pieceObject.GetComponent<DraggablePiece>();
                draggable.pieceId = pieceCounter;
                draggable.targetSlotTransform = slot.transform; // 告诉拼图块它的目标位置
                draggablePieces.Add(draggable); // 将新生成的拼图块添加到列表中，以便后续检查

                pieceCounter++;
            }
        }
    }

    /// <summary>
    /// 检查所有公开变量是否已在Inspector中赋值
    /// </summary>
    private bool AreReferencesAssigned()
    {
        // 现在主要检查 image 是否有值，因为它才是实际用于生成拼图的图片
        if (image == null) { Debug.LogError("错误: 当前使用的拼图原图 (image 变量) 为空! 请确保 sourceImageNums 列表或 sourceImage 字段设置正确。"); return false; }
        if (piecePrefab == null) { Debug.LogError("错误: 'Piece Prefab' 未在PuzzleManager中设置!"); return false; }
        if (slotPrefab == null) { Debug.LogError("错误: 'Slot Prefab' 未在PuzzleManager中设置!"); return false; }
        if (pieceAreaParent == null) { Debug.LogError("错误: 'Piece Area Parent' 未在PuzzleManager中设置!"); return false; }
        if (slotAreaParent == null) { Debug.LogError("错误: 'Slot Area Parent' 未在PuzzleManager中设置!"); return false; }
        if (spawnAreaCenter == null) { Debug.LogError("错误: 'Spawn Area Center' 未在PuzzleManager中设置!"); return false; }
        return true;
    }

    /// <summary>
    /// 检查拼图是否全部完成
    /// 应该在每次拼图块放置后调用
    /// </summary>
    public void CheckPuzzleCompletion()
    {
        foreach (DraggablePiece piece in draggablePieces)
        {
            if (!piece.IsLocked())
            {
                // 只要有一个拼图块没有锁定，拼图就未完成
                return; 
            }
        }
 
        // 如果所有拼图块都已锁定，则拼图完成
        Debug.Log("恭喜! 拼图已全部完成!");
        imgIndex++;
        var imgid = imgIndex - 1;
        afterPuzzleCompletionEvent.RaiseEvent(imgid, this);
    }
}