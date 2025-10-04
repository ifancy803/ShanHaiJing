using UnityEngine;
using UnityEngine.UI;

public class BlurController : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private Material blurMaterial;
    [SerializeField] private Material mosaicMaterial;
    [SerializeField] private EffectType effectType = EffectType.None;
    
    [Header("Blur Settings")]
    [SerializeField, Range(0f, 10f)] private float blurSize = 1.0f;
    
    [Header("Mosaic Settings")]
    [SerializeField, Range(1, 256)] private int mosaicSize = 16;

    private Image targetImage;
    private Material originalMaterial;
    
    public enum EffectType
    {
        None = 0,
        Blur = 1,
        Mosaic = 2
    }
    
    private void Start()
    {
        SetupEffect();
    }
    
    private void SetupEffect()
    {
        targetImage = GetComponent<Image>();
        if (targetImage == null)
        {
            Debug.LogWarning("No Image component found!");
            return;
        }
        
        originalMaterial = targetImage.material;
        
        // 确保材质实例化
        if (blurMaterial != null)
            blurMaterial = new Material(blurMaterial);
        if (mosaicMaterial != null)
            mosaicMaterial = new Material(mosaicMaterial);
            
        ApplyEffect();
    }
    
    private void ApplyEffect()
    {
        if (targetImage == null) return;
        
        switch (effectType)
        {
            case EffectType.None:
                targetImage.material = originalMaterial;
                break;
            case EffectType.Blur:
                if (blurMaterial != null)
                {
                    targetImage.material = blurMaterial;
                    blurMaterial.SetFloat("_BlurSize", blurSize);
                }
                break;
            case EffectType.Mosaic:
                if (mosaicMaterial != null)
                {
                    targetImage.material = mosaicMaterial;
                    mosaicMaterial.SetFloat("_MosaicSize", mosaicSize);
                }
                break;
        }
    }
    
    /// <summary>
    /// 设置模糊效果
    /// </summary>
    public void SetBlurEffect(float size)
    {
        effectType = EffectType.Blur;
        blurSize = Mathf.Clamp(size, 0f, 10f);
        ApplyEffect();
    }
    
    /// <summary>
    /// 设置马赛克效果
    /// </summary>
    public void SetMosaicEffect(int size)
    {
        effectType = EffectType.Mosaic;
        mosaicSize = Mathf.Clamp(size, 1, 256);
        ApplyEffect();
    }
    
    /// <summary>
    /// 清除效果
    /// </summary>
    public void ClearEffect()
    {
        effectType = EffectType.None;
        ApplyEffect();
    }
    
    /// <summary>
    /// 设置模糊强度
    /// </summary>
    public void SetBlurRadius(float radius)
    {
        if (effectType == EffectType.Blur)
        {
            blurSize = Mathf.Clamp(radius, 0f, 10f);
            if (blurMaterial != null)
                blurMaterial.SetFloat("_BlurSize", blurSize);
        }
    }
    
    /// <summary>
    /// 设置马赛克大小
    /// </summary>
    public void SetMosaicSize(int size)
    {
        if (effectType == EffectType.Mosaic)
        {
            mosaicSize = Mathf.Clamp(size, 1, 256);
            if (mosaicMaterial != null)
                mosaicMaterial.SetFloat("_MosaicSize", mosaicSize);
        }
    }
    
    // 在Inspector中修改值时更新效果
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyEffect();
        }
    }
}