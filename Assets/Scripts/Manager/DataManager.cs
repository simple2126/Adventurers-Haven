using AdventurersHaven;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : SingletonBase<DataManager>
{
    private Dictionary<SfxType, float> individualSfxVolumeDict;

    protected override void Awake()
    {
        base.Awake();
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
}
