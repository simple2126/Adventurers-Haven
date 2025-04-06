using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class OptionPanel : UIBase
{
    [SerializeField] private Slider bgmVolume;
    [SerializeField] private Slider sfxVolume;

    [SerializeField] private Button toggleBtn;
    [SerializeField] private GameObject[] objs;
    
    private bool isFrame30;
    private const int FrameRate30 = 30;
    private const int FrameRate60 = 60;

    protected override void Awake()
    {
        base.Awake();
        bgmVolume.onValueChanged.AddListener(OnBgmVolumeChanged);
        sfxVolume.onValueChanged.AddListener(OnSfxVolumeChanged);
        toggleBtn.onClick.AddListener(SwapButtonImage);
        isFrame30 = GameManager.Instance.IsFrame30;
    }

    private void Start()
    {
        transform.DOLocalMove(Vector3.up * 10, 1).SetLink(gameObject);
        SetVolumeImage();
        SetButtonImage();
    }

    private void SetVolumeImage()
    {
        // 이벤트 리스너를 일시적으로 제거
        bgmVolume.onValueChanged.RemoveListener(OnBgmVolumeChanged);
        sfxVolume.onValueChanged.RemoveListener(OnSfxVolumeChanged);

        bgmVolume.value = SoundManager.Instance.BgmVolume;
        sfxVolume.value = SoundManager.Instance.GlobalSfxVolume;

        // 이벤트 리스너를 다시 추가
        bgmVolume.onValueChanged.AddListener(OnBgmVolumeChanged);
        sfxVolume.onValueChanged.AddListener(OnSfxVolumeChanged);
    }

    private void OnBgmVolumeChanged(float value)
    {
        SoundManager.Instance.SetBgmVolume(value);
    }

    private void OnSfxVolumeChanged(float value)
    {
        SoundManager.Instance.SetSfxVolume(value);
    }

    private void SetButtonImage()
    {
        isFrame30 = GameManager.Instance.IsFrame30;
        UpdateButtonImage();
    }

    private void SwapButtonImage()
    {
        isFrame30 = !isFrame30;
        GameManager.Instance.SetIsFrame30(isFrame30);
        Application.targetFrameRate = isFrame30 ? FrameRate30 : FrameRate60;
        UpdateButtonImage();
    }

    private void UpdateButtonImage()
    {
        objs[0].SetActive(!isFrame30); // 60
        objs[1].SetActive(isFrame30);  // 30
    }
}
