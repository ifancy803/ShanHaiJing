using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    [Header("音乐源组件")]
    public AudioSource audioSource;

    [Header("音乐剪辑")]
    public AudioClip music1; // 当 ImageListPanel.activeSelf = true 时播放
    public AudioClip music2; // 当 ImageListPanel.activeSelf = false 时播放

    [Header("监控对象")]
    public GameObject imageListPanel;

    private bool lastState = false;

    void Awake()
    {
        // 持久化
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
    }

    void Start()
    {
        // 初始化状态
        if (imageListPanel != null)
        {
            lastState = imageListPanel.activeSelf;
            PlayMusicBasedOnPanelState();
        }
    }

    void Update()
    {
        if (imageListPanel == null) return;

        bool currentState = imageListPanel.activeSelf;
        if (currentState != lastState)
        {
            PlayMusicBasedOnPanelState();
            lastState = currentState;
        }
    }

    void PlayMusicBasedOnPanelState()
    {
        if (imageListPanel.activeSelf)
        {
            if (audioSource.clip != music1)
            {
                audioSource.clip = music1;
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.clip != music2)
            {
                audioSource.clip = music2;
                audioSource.Play();
            }
        }
    }
}