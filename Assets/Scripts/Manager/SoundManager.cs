using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SoundManager : SingletonBase<SoundManager>
{
    [Header("BGM List")]
    private Dictionary<BgmType, AudioClip> _bgmDict = new Dictionary<BgmType, AudioClip>();

    [System.Serializable]
    private class BgmKeyValuePair
    {
        public BgmType bgmType;
        public AudioClip clip;
    }
    [SerializeField] private List<BgmKeyValuePair> _bgmList;

    [Header("SFX List")]
    private Dictionary<SfxType, AudioClip> _sfxDict = new Dictionary<SfxType, AudioClip>();

    [System.Serializable]
    private class SfxKeyValuePair
    {
        public SfxType sfxType;
        public AudioClip clip;
    }
    [SerializeField] private List<SfxKeyValuePair> _sfxList;

    private AudioSource _audioBgm;

    // BGM 볼륨
    [Range(0f, 1f)] public float bgmVolume;

    // SFX 볼륨 및 음 높낮이 조절
    [Range(0f, 1f)] public float globalSfxVolume;
    [SerializeField][Range(0f, 1f)] private float _sfxPitchVariance; // 높은 음이 나옴

    private Dictionary<SfxType, float> _individualSfxVolumeDict;

    public bool IsPlayBGM { get; private set; } // BGM 출력 설정 (On / Off)
    public bool IsPlaySFX { get; private set; } // SFX 출력 설정 (On / Off)

    public PoolManager.PoolConfig[] poolconfigs;

    protected override void Awake()
    {
        base.Awake();
        _audioBgm = GetComponent<AudioSource>();
        _audioBgm.volume = bgmVolume;
        _audioBgm.loop = true;
        IsPlayBGM = true;
        IsPlaySFX = true;

        SetBgmDictionary();
        SetSfxDictionary();
        SetSfxVolumeDictionary();
        PoolManager.Instance.AddPools<SfxSoundSource>(poolconfigs);
        DontDestroyOnLoad(gameObject);
    }

    private void SetBgmDictionary()
    {
        if (_bgmList == null) return;

        foreach (BgmKeyValuePair bgm in _bgmList)
        {
            if (!_bgmDict.ContainsKey(bgm.bgmType))
            {
                _bgmDict.Add(bgm.bgmType, bgm.clip);
            }
        }
    }

    private void SetSfxDictionary()
    {
        if (_sfxList == null) return;

        foreach (SfxKeyValuePair sfx in _sfxList)
        {
            if (!_sfxDict.ContainsKey(sfx.sfxType))
            {
                _sfxDict.Add(sfx.sfxType, sfx.clip);
            }
        }
    }

    private void SetSfxVolumeDictionary()
    {
        _individualSfxVolumeDict = DataManager.Instance.GetIndvidualSfxVolumeDict();

        foreach (SfxType key in _sfxDict.Keys)
        {
            // 해당 키값이 없을 때 개별 볼륨을 1.0 으로 초기화
            // 즉 전체 Sfx 볼륨과 일치
            if (!_individualSfxVolumeDict.ContainsKey(key))
            {
                _individualSfxVolumeDict.Add(key, 1.0f);
            }
        }
    }

    // 배경 음악 시작
    public void PlayBGM(BgmType bgmType)
    {
        if (IsPlayBGM)
        {
            _audioBgm.clip = _bgmDict[bgmType];
            _audioBgm.mute = false;
            _audioBgm.loop = true;
            _audioBgm.Play();
        }
    }

    // 배경 음악 정지
    public void StopBGM()
    {
        _audioBgm.Stop();
    }

    // 효과음 재생
    public void PlaySFX(SfxType sfxType)
    {
        if (IsPlaySFX)
        {
            SfxSoundSource soundSource = PoolManager.Instance.SpawnFromPool<SfxSoundSource>(sfxType.ToString());

            if (soundSource == null) return;
            soundSource.gameObject.SetActive(true);
            soundSource.Play(_sfxDict[sfxType], sfxType, globalSfxVolume * _individualSfxVolumeDict[sfxType], _sfxPitchVariance);
        }
    }

    // BGM On / Off
    public void ChangeIsPlayBGM()
    {
        IsPlayBGM = !IsPlayBGM;
    }

    // SFX On / Off
    public void ChangeIsPlaySFX()
    {
        IsPlaySFX = !IsPlaySFX;
    }

    public void OnClick()
    {
        // IsPointerOverGameObject() -> UI만 작동하도록 제어
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            PlaySFX(SfxType.Click);
        }
    }

    public void SetSfxVolume(float volume)
    {
        globalSfxVolume = volume;
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = volume;
        _audioBgm.volume = bgmVolume;
    }
}