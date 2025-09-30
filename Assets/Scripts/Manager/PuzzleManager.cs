using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Texture2D sourceImage;

    public int rows = 4;
    public int cols = 3;

    public Transform spawnAreaCenter;
    public Vector2 spawnAreaSize;
    
    public GameObject piecePrefab;

    void Start()
    {
        GeneratePuzzle();
    }

    void GeneratePuzzle()
    {
        int pieceWidth = sourceImage.width / cols;
        int pieceHeight = sourceImage.height / rows;
        
        for(int y = 0;y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Texture2D pieceTexture = new Texture2D(pieceWidth, pieceHeight);

                //从原图获取小块的数据
                Color[] pixels = sourceImage.GetPixels(x * pieceWidth, y * pieceHeight, pieceWidth, pieceHeight);
                
                //将获取到的数据设置到纹理里
                pieceTexture.SetPixels(pixels);
                pieceTexture.Apply();
                
                //纹理转成Sprite
                Sprite pieceSprite = Sprite.Create(pieceTexture, new Rect(0, 0, pieceWidth, pieceHeight), new Vector2(0.5f, 0.5f));

                //实例化预制体
                GameObject piece = Instantiate(piecePrefab);
                piece.name = $"Piece_{x}_{y}";
                
                //设置拼图的Sprite
                piece.GetComponent<SpriteRenderer>().sprite = pieceSprite;

                //调整正确的位置
                float correctX = (x - cols / 2f + 0.5f) * (pieceWidth / 100f); // 除以100f是将像素单位转为Unity单位
                float correctY = (y - rows / 2f + 0.5f) * (pieceHeight / 100f);
                
                DraggablePiece draggable = piece.GetComponent<DraggablePiece>();
                if (draggable != null)
                {
                    draggable.correctPosition = new Vector3(correctX, correctY, 0);
                }
                
                //区域内随机位置生成
                float randomX = Random.Range(spawnAreaCenter.position.x - spawnAreaSize.x / 2, spawnAreaCenter.position.x + spawnAreaSize.x / 2);
                float randomY = Random.Range(spawnAreaCenter.position.y - spawnAreaSize.y / 2, spawnAreaCenter.position.y + spawnAreaSize.y / 2);
                piece.transform.position = new Vector3(randomX, randomY, 0);
            }
        }
    }
}
