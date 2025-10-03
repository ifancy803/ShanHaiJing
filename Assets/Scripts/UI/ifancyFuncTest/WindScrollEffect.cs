using UnityEngine;
using UnityEngine.UI;

public class WindScrollEffect : MonoBehaviour
{
    public float amplitude = 10f;   // 左右摆动的幅度（像素）
    public float speed = 1f;        // 摆动速度
    private RectTransform rect;
    private Vector2 originalPos;
    private float randomOffset;     // 随机相位偏移

    void Start()
    {
        rect = GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;

        // 每个物体随机一个相位，避免同步摆动
        randomOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float offsetX = Mathf.Sin(Time.time * speed + randomOffset) * amplitude;
        rect.anchoredPosition = originalPos + new Vector2(offsetX, 0);
    }
}