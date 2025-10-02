// PuzzleManager.cs
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{

    [Header("1. 拼图基础设置")]
    [Tooltip("完整的拼图原图")]
    public Texture2D sourceImage;
    [Tooltip("拼图的行数")]
    public int rows = 4;
    [Tooltip("拼图的列数")]
    public int cols = 3;

    [Header("2. 预制体 (Prefabs)")]
    [Tooltip("拼图块的预制体 (必须带SpriteRenderer和DraggablePiece脚本)")]
    public GameObject piecePrefab;
    [Tooltip("卡槽的预制体 (必须带有SpriteRenderer和PuzzleSlot脚本)")]
    public GameObject slotPrefab;

    [Header("3. 场景中的父物体")]
    [Tooltip("一个空物体, 用来存放所有生成的拼图块, 保持场景整洁")]
    public Transform pieceAreaParent;
    [Tooltip("一个空物体, 用来存放所有生成的卡槽, 作为网格的中心")]
    public Transform slotAreaParent;

    [Header("4. 拼图块随机生成区域")]
    [Tooltip("一个空物体, 标记随机区域的中心点")]
    public Transform spawnAreaCenter;
    [Tooltip("随机区域的尺寸 (宽和高)")]
    public Vector2 spawnAreaSize = new Vector2(5, 5);


    // 运行时使用的私有列表
    private List<PuzzleSlot> puzzleSlots = new List<PuzzleSlot>();

    void Start()
    {
        // 检查所有必要的引用是否已在编辑器中设置好
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

        foreach (Transform child in pieceAreaParent) Destroy(child.gameObject);
        foreach (Transform child in slotAreaParent) Destroy(child.gameObject);
        puzzleSlots.Clear();


        int pieceWidth = sourceImage.width / cols;
        int pieceHeight = sourceImage.height / rows;

        float pieceWorldWidth = (float)pieceWidth / 100f;
        float pieceWorldHeight = (float)pieceHeight / 100f;

        float gridWorldWidth = cols * pieceWorldWidth;
        float gridWorldHeight = rows * pieceWorldHeight;
        float startX = slotAreaParent.position.x - gridWorldWidth / 2f + pieceWorldWidth / 2f;
        float startY = slotAreaParent.position.y + gridWorldHeight / 2f - pieceWorldHeight / 2f;

        int pieceCounter = 0; // 计数器, 同时作为ID


        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                // --- 创建卡槽 (Slot) ---
                Vector3 slotPosition = new Vector3(startX + x * pieceWorldWidth, startY - y * pieceWorldHeight, 0);
                GameObject slotObject = Instantiate(slotPrefab, slotPosition, Quaternion.identity, slotAreaParent);
                slotObject.name = $"Slot_{x}_{y}";

                SpriteRenderer sr = slotObject.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    // 将SpriteRenderer的绘制模式设置为Sliced or Tiled，这样修改size才有效且不会拉伸变形
                    sr.drawMode = SpriteDrawMode.Sliced; 
                    // 设置尺寸为我们为拼图块计算出的世界单位尺寸
                    sr.size = new Vector2(pieceWorldWidth, pieceWorldHeight); 
                }
                
                PuzzleSlot slot = slotObject.GetComponent<PuzzleSlot>();
                slot.slotId = pieceCounter;
                puzzleSlots.Add(slot);

                
                Color[] pixels = sourceImage.GetPixels(x * pieceWidth, (rows - 1 - y) * pieceHeight, pieceWidth, pieceHeight);
                Texture2D pieceTexture = new Texture2D(pieceWidth, pieceHeight);
                pieceTexture.SetPixels(pixels);
                pieceTexture.Apply();

                Sprite pieceSprite = Sprite.Create(pieceTexture, new Rect(0, 0, pieceWidth, pieceHeight), new Vector2(0.5f, 0.5f), 100f);

                // 随机放置拼图块
                float randomX = Random.Range(spawnAreaCenter.position.x - spawnAreaSize.x / 2, spawnAreaCenter.position.x + spawnAreaSize.x / 2);
                float randomY = Random.Range(spawnAreaCenter.position.y - spawnAreaSize.y / 2, spawnAreaCenter.position.y + spawnAreaSize.y / 2);
                GameObject pieceObject = Instantiate(piecePrefab, new Vector3(randomX, randomY, 0), Quaternion.identity, pieceAreaParent);
                pieceObject.name = $"Piece_{x}_{y}";
                pieceObject.GetComponent<SpriteRenderer>().sprite = pieceSprite;

                // 关联拼图块和对应的卡槽
                DraggablePiece draggable = pieceObject.GetComponent<DraggablePiece>();
                draggable.pieceId = pieceCounter;
                draggable.targetSlotTransform = slot.transform; // 告诉拼图块它的目标位置

                pieceCounter++;
            }
        }
    }

    /// <summary>
    /// 检查所有公开变量是否已在Inspector中赋值
    /// </summary>
    private bool AreReferencesAssigned()
    {
        if (sourceImage == null) { Debug.LogError("错误: 'Source Image' 未在PuzzleManager中设置!"); return false; }
        if (piecePrefab == null) { Debug.LogError("错误: 'Piece Prefab' 未在PuzzleManager中设置!"); return false; }
        if (slotPrefab == null) { Debug.LogError("错误: 'Slot Prefab' 未在PuzzleManager中设置!"); return false; }
        if (pieceAreaParent == null) { Debug.LogError("错误: 'Piece Area Parent' 未在PuzzleManager中设置!"); return false; }
        if (slotAreaParent == null) { Debug.LogError("错误: 'Slot Area Parent' 未在PuzzleManager中设置!"); return false; }
        if (spawnAreaCenter == null) { Debug.LogError("错误: 'Spawn Area Center' 未在PuzzleManager中设置!"); return false; }
        return true;
    }
}