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
        bgmVolume.onValueChanged.AddListener(value => SoundManager.Instance.SetBgmVolume(value));
        sfxVolume.onValueChanged.AddListener(value => SoundManager.Instance.SetSfxVolume(value));
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
        bgmVolume.value = SoundManager.Instance.BgmVolume;
        sfxVolume.value = SoundManager.Instance.GlobalSfxVolume;
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
