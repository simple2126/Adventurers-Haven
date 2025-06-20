
using AdventurersHaven;
using System.Collections.Generic;

public class SoundDataManager
{
    private Dictionary<BgmType, float> individualBgmVolumeDict;
    private Dictionary<SfxType, float> individualSfxVolumeDict;

    public void Init()
    {
        SetIndividualSfxVolumeDict();
        SetIndividualBgmVolumeDict();
    }

    private void SetIndividualSfxVolumeDict()
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
            SetIndividualSfxVolumeDict();
        }
        return individualSfxVolumeDict;
    }

    private void SetIndividualBgmVolumeDict()
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
            SetIndividualBgmVolumeDict();
        }
        return individualBgmVolumeDict;
    }
}
