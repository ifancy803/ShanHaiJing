using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class PuzzleManager : MonoBehaviour
{
    public List<Texture2D> sourceImageNums;
    private Dictionary<int,Texture2D> sourceImageToIndexNums;

    [Header("拼图基础设置")]
    public int rows = 3;
    public int cols = 3;

    private Texture2D image;
    private int imgIndex = 0;
    public float PPUvalue = 0f;

    [Header("预制体 (Prefabs)")]
    [Tooltip("拼图块的预制体 (必须带SpriteRenderer和DraggablePiece脚本)")]
    public GameObject piecePrefab;
    [Tooltip("卡槽的预制体 (必须带有SpriteRenderer、PuzzleSlot和PuzzleSlotOutline脚本)")]
    public GameObject slotPrefab;

    [Header("场景中的父物体")]
    public Transform pieceAreaParent;
    public Transform slotAreaParent;

    [Header("拼图块随机生成区域")]
    public Transform spawnAreaCenter;
    public Vector2 spawnAreaSize = new Vector2(5, 5);

    [Header("广播")] 
    public IntEventSO afterPuzzleCompletionEvent;
    
    private List<DraggablePiece> draggablePieces = new List<DraggablePiece>();
    private List<PuzzleSlot> puzzleSlots = new List<PuzzleSlot>();

    void Awake()
    {
        sourceImageToIndexNums = new Dictionary<int, Texture2D>();
        int index = 0;
        foreach(Texture2D img in sourceImageNums)
        {
            if (img != null)
            {
                sourceImageToIndexNums.Add(index, img);
            }
            else
            {
                Debug.LogWarning($"sourceImageNums 列表中索引 {index} 处的图片为空，已跳过添加到字典。");
            }
            index++;
        }

        if (sourceImageToIndexNums.ContainsKey(imgIndex))
        {
            image = sourceImageToIndexNums[imgIndex];
        }
    }

    void Start()
    {
        if (!AreReferencesAssigned())
        {
            enabled = false; // 禁用此组件以防止进一步错误
            return;
        }
    }

    /// <summary>
    /// 生成所有的卡槽和拼图块
    /// </summary>
    void GeneratePuzzle()
    {
        foreach (Transform child in pieceAreaParent) Destroy(child.gameObject);
        foreach (Transform child in slotAreaParent) Destroy(child.gameObject);
        
        puzzleSlots.Clear();
        draggablePieces.Clear();

        if (imgIndex < 3) cols = rows = 3;
        else if (imgIndex < 6) cols = rows = 4;
        else if (imgIndex < 9) cols = rows = 5;
        
        int pieceWidth = image.width / cols ;
        int pieceHeight = image.height / rows ;

        float pieceWorldWidth = (float)pieceWidth / PPUvalue;
        float pieceWorldHeight = (float)pieceHeight / PPUvalue;

        float gridWorldWidth = cols * pieceWorldWidth;
        float gridWorldHeight = rows * pieceWorldHeight;
        float startX = slotAreaParent.position.x - gridWorldWidth / 2f + pieceWorldWidth / 2f;
        float startY = slotAreaParent.position.y + gridWorldHeight / 2f - pieceWorldHeight / 2f;

        int pieceCounter = 0;
        
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
                    sr.drawMode = SpriteDrawMode.Sliced;
                    sr.size = new Vector2(pieceWorldWidth, pieceWorldHeight);
                }
                
                PuzzleSlot slot = slotObject.GetComponent<PuzzleSlot>();
                slot.slotId = pieceCounter;
                puzzleSlots.Add(slot);

                // 获取 PuzzleSlotOutline 脚本
                PuzzleSlotOutline slotOutline = slotObject.GetComponent<PuzzleSlotOutline>();
                if (slotOutline == null)
                {
                    Debug.LogError($"卡槽 {slotObject.name} 缺少 PuzzleSlotOutline 组件！请确保你的 slotPrefab 上有此组件。");
                }


                Color[] pixels = image.GetPixels(x * pieceWidth, (rows - 1 - y) * pieceHeight, pieceWidth, pieceHeight);
                Texture2D pieceTexture = new Texture2D(pieceWidth, pieceHeight);
                pieceTexture.SetPixels(pixels);
                pieceTexture.Apply();

                Sprite pieceSprite = Sprite.Create(pieceTexture, new Rect(0, 0, pieceWidth, pieceHeight), new Vector2(0.5f, 0.5f), PPUvalue);

                float randomX = Random.Range(spawnAreaCenter.position.x - spawnAreaSize.x / 2, spawnAreaCenter.position.x + spawnAreaSize.x / 2);
                float randomY = Random.Range(spawnAreaCenter.position.y - spawnAreaSize.y / 2, spawnAreaCenter.position.y + spawnAreaSize.y / 2);
                GameObject pieceObject = Instantiate(piecePrefab, new Vector3(randomX, randomY, 0), Quaternion.identity, pieceAreaParent);
                pieceObject.name = $"Piece_{x}_{y}";
                
                // 拼图块 SpriteRenderer 的设置
                SpriteRenderer pieceSr = pieceObject.GetComponent<SpriteRenderer>();
                if (pieceSr != null)
                {
                    pieceSr.sprite = pieceSprite;
                    // 如果你的墨水Shader是应用在Material上，那么这里不需要设置SpriteRenderer的Material
                    // 如果你希望PuzzlePieceInkEffect控制的是SpriteRenderer的Material，
                    // 确保 piecePrefab 上的SpriteRenderer使用了一个默认材质，然后PuzzlePieceInkEffect会替换它。
                    // 或者，确保 piecePrefab 上的SpriteRenderer的Material设置为 Custom/InkSplash Material
                }

                BoxCollider2D pieceCollider = pieceObject.GetComponent<BoxCollider2D>();
                if (pieceCollider != null)
                {
                    pieceCollider.autoTiling = false; 
                    pieceCollider.offset = Vector2.zero;
                    float colliderPadding = 0.1f;
                    pieceCollider.size = new Vector2(pieceWorldWidth + colliderPadding, pieceWorldHeight + colliderPadding); 
                }
                else
                {
                    Debug.LogWarning($"Piece_{x}_{y} 缺少 BoxCollider2D 组件。");
                }
                
                DraggablePiece draggable = pieceObject.GetComponent<DraggablePiece>();
                draggable.pieceId = pieceCounter;
                draggable.targetSlotTransform = slot.transform;
                draggablePieces.Add(draggable);

                // *** 关键修改：将 PuzzleSlotOutline 引用传递给 DraggablePiece ***
                if (slotOutline != null)
                {
                    draggable.SetTargetSlotOutline(slotOutline);
                }

                pieceCounter++;
            }
        }
    }

    private bool AreReferencesAssigned()
    {
        if (image == null) { Debug.LogError("错误: 当前使用的拼图原图 (image 变量) 为空! 请确保 sourceImageNums 列表设置正确。"); return false; }
        if (piecePrefab == null) { Debug.LogError("错误: 'Piece Prefab' 未在PuzzleManager中设置!"); return false; }
        if (slotPrefab == null) { Debug.LogError("错误: 'Slot Prefab' 未在PuzzleManager中设置!"); return false; }
        if (pieceAreaParent == null) { Debug.LogError("错误: 'Piece Area Parent' 未在PuzzleManager中设置!"); return false; }
        if (slotAreaParent == null) { Debug.LogError("错误: 'Slot Area Parent' 未在PuzzleManager中设置!"); return false; }
        if (spawnAreaCenter == null) { Debug.LogError("错误: 'Spawn Area Center' 未在PuzzleManager中设置!"); return false; }

        if (slotPrefab != null)
        {
            if (slotPrefab.GetComponent<PuzzleSlotOutline>() == null)
            {
                Debug.LogError("错误: 'Slot Prefab' 上缺少 PuzzleSlotOutline 脚本! 请确保卡槽预制体上附加了此脚本。");
                return false;
            }
            if (slotPrefab.GetComponent<PuzzleSlotOutline>().baseSlotMaterial == null)
            {
                Debug.LogError("错误: 'Slot Prefab' 上的 PuzzleSlotOutline 脚本中 'Base Slot Material' 未设置! 请确保将其拖拽到Inspector中。");
                return false;
            }
        }
        
        if (piecePrefab != null)
        {
            if (piecePrefab.GetComponent<PuzzlePieceInkEffect>() == null)
            {
                Debug.LogError("错误: 'Piece Prefab' 上缺少 PuzzlePieceInkEffect 脚本! 请确保拼图块预制体上附加了此脚本。");
                return false;
            }
            // 确保拼图块的SpriteRenderer使用带有 _InkAmount 属性的材质
            SpriteRenderer pieceSr = piecePrefab.GetComponent<SpriteRenderer>();
            if (pieceSr != null && pieceSr.sharedMaterial != null && !pieceSr.sharedMaterial.HasProperty("_InkAmount"))
            {
                 Debug.LogError("错误: 'Piece Prefab' 的 SpriteRenderer 材质不包含 '_InkAmount' 属性! 请确保材质使用了 'Custom/InkSplash' Shader。");
                 return false;
            }
        }


        return true;
    }

    public void CheckPuzzleCompletion()
    {
        foreach (DraggablePiece piece in draggablePieces)
        {
            if (!piece.IsLocked())
            {
                return; 
            }
        }
 
        Debug.Log("恭喜! 拼图已全部完成!");
        imgIndex++;
        var imgid = imgIndex - 1;
        afterPuzzleCompletionEvent.RaiseEvent(imgid, this);
    }

    public void GameStart(int index)
    {
        imgIndex = index;
        // 重新检查引用，以防游戏过程中有变动
        if (!AreReferencesAssigned()) {
            Debug.LogError("无法开始游戏：必要的引用未设置或配置错误。");
            return;
        }
        GeneratePuzzle();
    }
}