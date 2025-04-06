using AdventurersHaven;
using System.Collections.Generic;

public class DataManager : SingletonBase<DataManager>
{
    private Dictionary<BgmType, float> individualBgmVolumeDict;
    private Dictionary<SfxType, float> individualSfxVolumeDict;
    private Dictionary<ConstructionType, List<Construction_Data>> constructionDataDict;

    protected override void Awake()
    {
        base.Awake();

        SetIndividualSfxVolumeDict();
        SetIndividualBgmVolumeDict();
        SetConstructionDataDict();
        DontDestroyOnLoad(gameObject);
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
    private void SetConstructionDataDict()
    {
        List<Construction_Data> _constructionDataList = Construction_Data.GetList();
        Dictionary<ConstructionType, List<Construction_Data>> constructionDataDict = new Dictionary<ConstructionType, List<Construction_Data>>();

        foreach (var data in _constructionDataList)
        {
            if (!constructionDataDict.ContainsKey(data.ConstructionType))
            {
                constructionDataDict[data.ConstructionType] = new List<Construction_Data>();
            }
            constructionDataDict[data.ConstructionType].Add(data);
        }

        this.constructionDataDict = constructionDataDict;
    }

    public Dictionary<ConstructionType, List<Construction_Data>> GetConstructionDataDict()
    {
        if (constructionDataDict == null)
        {
            SetConstructionDataDict();
        }

        return constructionDataDict;
    }
}
