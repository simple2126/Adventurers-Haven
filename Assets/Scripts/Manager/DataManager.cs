using AdventurersHaven;
using System.Collections.Generic;

public class DataManager : SingletonBase<DataManager>
{
    private Dictionary<BgmType, float> individualBgmVolumeDict;
    private Dictionary<SfxType, float> individualSfxVolumeDict;

    protected override void Awake()
    {
        base.Awake();

        SetIndividualSfxVolumeDict<SfxVolume_Data>();
        SetIndividualBgmVolumeDict<BgmVolume_Data>();

        DontDestroyOnLoad(gameObject);
    }

    private void SetIndividualSfxVolumeDict<T>()
    {
        List<SfxVolume_Data> _sfxVolumeDataList = SfxVolume_Data.GetList();

        Dictionary<SfxType, float> individualSfxVolumeDict = new Dictionary<SfxType, float>();
        for (int i = 0; i < _sfxVolumeDataList.Count; i++)
        {
            individualSfxVolumeDict.Add(_sfxVolumeDataList[i].sfxType, _sfxVolumeDataList[i].volume);
        }

        this.individualSfxVolumeDict = individualSfxVolumeDict;
    }

    public Dictionary<SfxType, float> GetIndvidualSfxVolumeDict()
    {
        if (individualSfxVolumeDict == null)
        {
            SetIndividualSfxVolumeDict<SfxType>();
        }
        return individualSfxVolumeDict;
    }

    private void SetIndividualBgmVolumeDict<T>()
    {
        List<BgmVolume_Data> _bgmVolumeDataList = BgmVolume_Data.GetList();

        Dictionary<BgmType, float> individualBgmVolumeDict = new Dictionary<BgmType, float>();
        for (int i = 0; i < _bgmVolumeDataList.Count; i++)
        {
            individualBgmVolumeDict.Add(_bgmVolumeDataList[i].bgmType, _bgmVolumeDataList[i].volume);
        }

        this.individualBgmVolumeDict = individualBgmVolumeDict;
    }

    public Dictionary<BgmType, float> GetIndvidualBgmVolumeDict()
    {
        if (individualBgmVolumeDict == null)
        {
            SetIndividualBgmVolumeDict<BgmType>();
        }
        return individualBgmVolumeDict;
    }
}
