using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("音轨设置")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("全局点击音效")]
    [Tooltip("在这里放入你那个 freesound 点击音效")]
    public AudioClip globalClickSound;

    [Header("默认背景音乐")]
    public AudioClip defaultBGM;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (defaultBGM != null)
        {
            PlayBGM(defaultBGM);
        }
    }

    // --- 核心新增：全局监听鼠标点击 ---
    void Update()
    {
        // 0 代表鼠标左键
        if (Input.GetMouseButtonDown(0))
        {
            if (globalClickSound != null)
            {
                // 调用现有的播放音效方法
                PlaySFX(globalClickSound);
            }
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            // 使用 PlayOneShot 允许重叠播放（如果玩家点击很快，声音不会被切断）
            sfxSource.PlayOneShot(clip, volume);
        }
    }
}