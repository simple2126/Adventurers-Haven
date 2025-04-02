using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class SoundData<TEnum>
{
    public abstract TEnum Type { get; }
    public abstract AudioClip SoundClip { get; }
}

[System.Serializable]
public class BgmData : SoundData<BgmType>
{
    public BgmType BgmType;
    public AudioClip Clip;

    public override BgmType Type => BgmType;
    public override AudioClip SoundClip => Clip;
}

[System.Serializable]
public class SfxData : SoundData<SfxType>
{
    public SfxType SfxType;
    public AudioClip Clip;

    public override SfxType Type => SfxType;
    public override AudioClip SoundClip => Clip;
}

[System.Serializable]
public abstract class SoundVolumeData<TEnum>
{
    public TEnum VolumeType { get; protected set; }
    public float SoundVolume { get; protected set; }

    public SoundVolumeData() { }

    public SoundVolumeData(TEnum type, float volume)
    {
        SetData(type, volume);
    }

    public void SetData(TEnum type, float volume)
    {
        VolumeType = type;
        SoundVolume = volume;
    }
}

[System.Serializable]
public class BgmVolume : SoundVolumeData<BgmType>
{
    public BgmVolume() { }

    public BgmVolume(BgmType type, float volume) : base(type, volume) { }
}

[System.Serializable]
public class SfxVolume : SoundVolumeData<SfxType>
{
    public SfxVolume() { }

    public SfxVolume(SfxType type, float volume) : base(type, volume) { }
}

public class SoundManager : SingletonBase<SoundManager>
{
    [Header("BGM List")]
    private Dictionary<BgmType, (AudioClip clip, float volume)> bgmDataDict = new();
    [SerializeField] private List<BgmData> bgmList;
    [SerializeField] private List<BgmVolume> bgmVolumeList;

    [Header("SFX List")]
    private Dictionary<SfxType, (AudioClip clip, float volume)> sfxDataDict = new();
    [SerializeField] private List<SfxData> sfxList;
    [SerializeField] private List<SfxVolume> sfxVolumeList;

    private AudioSource audioBgm;

    [Range(0f, 1f)] public float BgmVolume;

    [Range(0f, 1f)] public float GlobalSfxVolume;
    [SerializeField][Range(0f, 1f)] private float sfxPitchVariance; // 높은 음이 나옴

    public bool IsPlayBGM { get; private set; } // BGM 출력 설정 (On / Off)
    public bool IsPlaySFX { get; private set; } // SFX 출력 설정 (On / Off)

    public PoolManager.PoolConfig[] Poolconfigs;

    protected override void Awake()
    {
        base.Awake();
        audioBgm = GetComponent<AudioSource>();
        audioBgm.volume = BgmVolume;
        audioBgm.loop = true;
        IsPlayBGM = true;
        IsPlaySFX = true;

        SetSoundData<BgmType, BgmData, BgmVolume>(
            bgmList,
            bgmDataDict,
            bgmVolumeList,
            DataManager.Instance.GetIndvidualBgmVolumeDict()
        );

        SetSoundData<SfxType, SfxData, SfxVolume>(
            sfxList,
            sfxDataDict,
            sfxVolumeList,
            DataManager.Instance.GetIndvidualSfxVolumeDict()
        );

        PoolManager.Instance.AddPools<SfxSoundSource>(Poolconfigs);
        DontDestroyOnLoad(gameObject);
    }

    private void SetSoundData<TEnum, TData, TVolume>(
        List<TData> dataList,
        Dictionary<TEnum, (AudioClip, float)> soundDict,
        List<TVolume> dataVolumeList,
        Dictionary<TEnum, float> volumeDict = null
    ) where TData : SoundData<TEnum>
      where TVolume : SoundVolumeData<TEnum>, new()  // 기본 생성자가 필요함
    {
        if (dataList == null) return;

        soundDict.Clear();
        if (dataVolumeList != null)
            dataVolumeList.Clear();

        foreach (TData data in dataList)
        {
            float volume = 1.0f;

            if (volumeDict?.TryGetValue(data.Type, out float v) == true)
            {
                volume = v > 0 ? v : 1.0f;
            }

            TVolume volumeData = new TVolume();
            ((SoundVolumeData<TEnum>)volumeData).SetData(data.Type, volume);

            dataVolumeList?.Add(volumeData);

            soundDict.TryAdd(data.Type, (data.SoundClip, volume));
        }
    }

    // 배경 음악 시작
    public void PlayBGM(BgmType bgmType)
    {
        if (audioBgm.isPlaying) StopBGM();

        if (IsPlayBGM && bgmDataDict.TryGetValue(bgmType, out var bgmData))
        {
            audioBgm.clip = bgmData.clip;
            audioBgm.mute = false;
            audioBgm.loop = true;
            audioBgm.volume = BgmVolume * bgmData.volume;  // 전역 볼륨과 개별 볼륨 적용
            audioBgm.Play();
            Debug.Log($"Playing BGM: {audioBgm.volume} bgmData.volume {bgmData.volume}");
        }
    }

    // 배경 음악 정지
    public void StopBGM()
    {
        audioBgm.Stop();
    }

    // 효과음 재생
    public void PlaySFX(SfxType sfxType)
    {
        if (IsPlaySFX && sfxDataDict.TryGetValue(sfxType, out var sfxData))
        {
            SfxSoundSource soundSource = PoolManager.Instance.SpawnFromPool<SfxSoundSource>(sfxType.ToString());

            if (soundSource == null) return;

            // 전역 볼륨과 개별 볼륨 적용
            float volume = GlobalSfxVolume * sfxData.volume;
            soundSource.Play(sfxData.clip, sfxType, volume, sfxPitchVariance);
        }
    }

    // BGM On / Off 토글
    public void ToggleBGM()
    {
        IsPlayBGM = !IsPlayBGM;
        audioBgm.mute = !IsPlayBGM;
    }

    // SFX On / Off 토글
    public void ToggleSFX()
    {
        IsPlaySFX = !IsPlaySFX;
    }

    // SFX 전역 볼륨 설정
    public void SetSfxVolume(float volume)
    {
        GlobalSfxVolume = Mathf.Clamp01(volume);
    }

    // BGM 전역 볼륨 설정
    public void SetBgmVolume(float volume)
    {
        BgmVolume = Mathf.Clamp01(volume);
        if (audioBgm != null)
        {
            audioBgm.volume = BgmVolume;
        }
    }
}