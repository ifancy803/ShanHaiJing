using UnityEngine;
using UnityEngine.UI;

public class BlurController : MonoBehaviour
{
    [Header("Blur Settings")]
    [SerializeField] private Material blurMaterial;
    [SerializeField] private BlurType blurType = BlurType.Gaussian;
    [SerializeField, Range(0f, 10f)] private float blurAmount = 1.0f;
    [SerializeField, Range(0f, 5f)] private float blurRadius = 1.0f;
    [SerializeField, Range(1, 8)] private int blurQuality = 4;
    [SerializeField, Range(0f, 2f)] private float intensity = 1.0f;
    
    [Header("Radial Blur Settings")]
    [SerializeField, Range(0f, 1f)] private float centerX = 0.5f;
    [SerializeField, Range(0f, 1f)] private float centerY = 0.5f;
    [SerializeField, Range(0f, 5f)] private float radialStrength = 1.0f;
    
    [Header("Directional Blur Settings")]
    [SerializeField, Range(-1f, 1f)] private float directionX = 1.0f;
    [SerializeField, Range(-1f, 1f)] private float directionY = 0.0f;
    
    [Header("Animation")]
    [SerializeField] private bool enableAnimation = false;
    [SerializeField] private AnimationCurve blurCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float animationDuration = 2.0f;
    [SerializeField] private bool loopAnimation = false;
    
    [Header("Auto Setup")]
    [SerializeField] private bool autoDetectImage = true;
    [SerializeField] private bool createMaterialInstance = true;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugMode = false;
    [SerializeField] private bool showDebugInfo = true;
    
    private float animationTime = 0f;
    private bool isAnimating = false;
    private Image targetImage;
    private Material originalMaterial;
    
    public enum BlurType
    {
        Gaussian = 0,
        Radial = 1,
        Directional = 2,
        Zoom = 3
    }
    
    private void Start()
    {
        SetupBlurEffect();
    }
    
    private void SetupBlurEffect()
    {
        // 自动检测Image组件
        if (autoDetectImage)
        {
            targetImage = GetComponent<Image>();
            if (targetImage == null)
            {
                Debug.LogWarning("No Image component found on this GameObject!");
                return;
            }
        }
        
        // 如果没有指定材质，尝试从Image获取
        if (blurMaterial == null && targetImage != null)
        {
            blurMaterial = targetImage.material;
            if (blurMaterial == null)
            {
                Debug.LogError("No material found on Image component!");
                return;
            }
        }
        
        // 保存原始材质
        if (targetImage != null)
        {
            originalMaterial = targetImage.material;
            
            // 创建材质实例
            if (createMaterialInstance && blurMaterial != null)
            {
                Material materialInstance = new Material(blurMaterial);
                targetImage.material = materialInstance;
                blurMaterial = materialInstance;
            }
        }
        
        UpdateShaderProperties();
    }
    
    private void Update()
    {
        if (enableAnimation && isAnimating)
        {
            UpdateAnimation();
        }
        
        UpdateShaderProperties();
    }
    
    private void UpdateAnimation()
    {
        animationTime += Time.deltaTime;
        float normalizedTime = animationTime / animationDuration;
        
        if (normalizedTime >= 1.0f)
        {
            if (loopAnimation)
            {
                animationTime = 0f;
                normalizedTime = 0f;
            }
            else
            {
                isAnimating = false;
                normalizedTime = 1.0f;
            }
        }
        
        float curveValue = blurCurve.Evaluate(normalizedTime);
        blurAmount = curveValue * 10f; // 映射到0-10范围
    }
    
    private void UpdateShaderProperties()
    {
        if (blurMaterial == null) 
        {
            if (showDebugInfo)
                Debug.LogWarning("BlurController: Blur material is null!");
            return;
        }
        
        // 设置模糊类型
        blurMaterial.SetInt("_BlurType", (int)blurType);
        
        // 设置基本参数
        blurMaterial.SetFloat("_BlurAmount", blurAmount);
        blurMaterial.SetFloat("_BlurRadius", blurRadius);
        blurMaterial.SetInt("_BlurQuality", blurQuality);
        blurMaterial.SetFloat("_Intensity", intensity);
        
        // 设置径向模糊参数
        blurMaterial.SetFloat("_CenterX", centerX);
        blurMaterial.SetFloat("_CenterY", centerY);
        blurMaterial.SetFloat("_RadialStrength", radialStrength);
        
        // 设置方向模糊参数
        blurMaterial.SetFloat("_DirectionX", directionX);
        blurMaterial.SetFloat("_DirectionY", directionY);
        
        // 调试信息
        if (enableDebugMode && showDebugInfo)
        {
            Debug.Log($"BlurController Debug - BlurAmount: {blurAmount}, BlurRadius: {blurRadius}, BlurType: {blurType}");
        }
    }
    
    /// <summary>
    /// 设置模糊类型
    /// </summary>
    public void SetBlurType(BlurType type)
    {
        blurType = type;
    }
    
    /// <summary>
    /// 设置模糊强度
    /// </summary>
    public void SetBlurAmount(float amount)
    {
        blurAmount = Mathf.Clamp(amount, 0f, 10f);
    }
    
    /// <summary>
    /// 设置模糊半径
    /// </summary>
    public void SetBlurRadius(float radius)
    {
        blurRadius = Mathf.Clamp(radius, 0f, 5f);
    }
    
    /// <summary>
    /// 设置模糊质量
    /// </summary>
    public void SetBlurQuality(int quality)
    {
        blurQuality = Mathf.Clamp(quality, 1, 8);
    }
    
    /// <summary>
    /// 设置径向模糊中心点
    /// </summary>
    public void SetRadialCenter(float x, float y)
    {
        centerX = Mathf.Clamp01(x);
        centerY = Mathf.Clamp01(y);
    }
    
    /// <summary>
    /// 设置径向模糊强度
    /// </summary>
    public void SetRadialStrength(float strength)
    {
        radialStrength = Mathf.Clamp(strength, 0f, 5f);
    }
    
    /// <summary>
    /// 设置方向模糊方向
    /// </summary>
    public void SetDirectionalBlurDirection(float x, float y)
    {
        directionX = Mathf.Clamp(x, -1f, 1f);
        directionY = Mathf.Clamp(y, -1f, 1f);
    }
    
    /// <summary>
    /// 设置整体强度
    /// </summary>
    public void SetIntensity(float intensity)
    {
        this.intensity = Mathf.Clamp(intensity, 0f, 2f);
    }
    
    /// <summary>
    /// 开始模糊动画
    /// </summary>
    public void StartBlurAnimation()
    {
        if (enableAnimation)
        {
            isAnimating = true;
            animationTime = 0f;
        }
    }
    
    /// <summary>
    /// 停止模糊动画
    /// </summary>
    public void StopBlurAnimation()
    {
        isAnimating = false;
    }
    
    /// <summary>
    /// 重置模糊参数
    /// </summary>
    public void ResetBlur()
    {
        blurAmount = 0f;
        blurRadius = 1f;
        intensity = 1f;
        centerX = 0.5f;
        centerY = 0.5f;
        radialStrength = 1f;
        directionX = 1f;
        directionY = 0f;
    }
    
    /// <summary>
    /// 创建模糊材质实例
    /// </summary>
    public void CreateBlurMaterialInstance()
    {
        if (blurMaterial != null)
        {
            blurMaterial = new Material(blurMaterial);
        }
    }
    
    /// <summary>
    /// 获取当前模糊强度
    /// </summary>
    public float GetBlurAmount()
    {
        return blurAmount;
    }
    
    /// <summary>
    /// 获取当前模糊类型
    /// </summary>
    public BlurType GetBlurType()
    {
        return blurType;
    }
    
    /// <summary>
    /// 快速设置高斯模糊
    /// </summary>
    public void SetGaussianBlur(float amount, float radius = 1f)
    {
        SetBlurType(BlurType.Gaussian);
        SetBlurAmount(amount);
        SetBlurRadius(radius);
    }
    
    /// <summary>
    /// 快速设置径向模糊
    /// </summary>
    public void SetRadialBlur(float amount, float centerX = 0.5f, float centerY = 0.5f, float strength = 1f)
    {
        SetBlurType(BlurType.Radial);
        SetBlurAmount(amount);
        SetRadialCenter(centerX, centerY);
        SetRadialStrength(strength);
    }
    
    /// <summary>
    /// 快速设置方向模糊
    /// </summary>
    public void SetDirectionalBlur(float amount, float directionX = 1f, float directionY = 0f, float radius = 1f)
    {
        SetBlurType(BlurType.Directional);
        SetBlurAmount(amount);
        SetDirectionalBlurDirection(directionX, directionY);
        SetBlurRadius(radius);
    }
    
    /// <summary>
    /// 快速设置缩放模糊
    /// </summary>
    public void SetZoomBlur(float amount, float centerX = 0.5f, float centerY = 0.5f, float strength = 1f)
    {
        SetBlurType(BlurType.Zoom);
        SetBlurAmount(amount);
        SetRadialCenter(centerX, centerY);
        SetRadialStrength(strength);
    }
    
    /// <summary>
    /// 启用/禁用模糊效果
    /// </summary>
    public void SetBlurEnabled(bool enabled)
    {
        if (targetImage != null)
        {
            if (enabled)
            {
                targetImage.material = blurMaterial;
            }
            else
            {
                targetImage.material = originalMaterial;
            }
        }
    }
    
    /// <summary>
    /// 检查模糊是否启用
    /// </summary>
    public bool IsBlurEnabled()
    {
        return targetImage != null && targetImage.material == blurMaterial;
    }
    
    /// <summary>
    /// 获取目标Image组件
    /// </summary>
    public Image GetTargetImage()
    {
        return targetImage;
    }
    
    /// <summary>
    /// 手动设置目标Image
    /// </summary>
    public void SetTargetImage(Image image)
    {
        targetImage = image;
        if (image != null)
        {
            originalMaterial = image.material;
            SetupBlurEffect();
        }
    }
    
    /// <summary>
    /// 启用调试模式
    /// </summary>
    public void EnableDebugMode(bool enable)
    {
        enableDebugMode = enable;
        if (enable && blurMaterial != null)
        {
            blurMaterial.SetInt("_DebugMode", 1);
        }
        else if (blurMaterial != null)
        {
            blurMaterial.SetInt("_DebugMode", 0);
        }
    }
    
    /// <summary>
    /// 测试原始图片显示
    /// </summary>
    public void TestOriginalImage()
    {
        if (blurMaterial != null)
        {
            blurMaterial.SetFloat("_BlurAmount", 0f);
            blurMaterial.SetInt("_DebugMode", 3);
        }
    }
    
    /// <summary>
    /// 检查材质和纹理状态
    /// </summary>
    public void CheckMaterialStatus()
    {
        if (targetImage == null)
        {
            Debug.LogError("Target Image is null!");
            return;
        }
        
        if (blurMaterial == null)
        {
            Debug.LogError("Blur Material is null!");
            return;
        }
        
        if (targetImage.sprite == null)
        {
            Debug.LogWarning("Image sprite is null!");
        }
        
        Debug.Log($"Material: {blurMaterial.name}");
        Debug.Log($"Image Material: {targetImage.material.name}");
        Debug.Log($"Sprite: {(targetImage.sprite != null ? targetImage.sprite.name : "null")}");
        Debug.Log($"Blur Amount: {blurAmount}");
        Debug.Log($"Blur Type: {blurType}");
    }
    
    /// <summary>
    /// 重置为原始状态
    /// </summary>
    public void ResetToOriginal()
    {
        if (targetImage != null && originalMaterial != null)
        {
            targetImage.material = originalMaterial;
        }
    }
}
