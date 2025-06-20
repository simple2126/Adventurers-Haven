using AdventurersHaven;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : SingletonBase<DataManager>
{
    public SoundDataManager Sound;
    public ConstructionDataManager Construction;
    private Dictionary<AdventurerType, List<Adventurer_Data>> adventurerDataDict;
    
    protected override void Awake()
    {
        base.Awake();

        Sound = new SoundDataManager();
        Sound.Init();

        Construction = new ConstructionDataManager();
        Construction.Init();

        SetAdventurerDataDict();
        DontDestroyOnLoad(gameObject);
    }

    private void SetAdventurerDataDict()
    {
        List<Adventurer_Data> _adventurerDataList = Adventurer_Data.GetList();
        Dictionary<AdventurerType, List<Adventurer_Data>> adventurerDataDict = new Dictionary<AdventurerType, List<Adventurer_Data>>();
        foreach (var data in _adventurerDataList)
        {
            if (!adventurerDataDict.ContainsKey(data.adventurerType))
            {
                adventurerDataDict[data.adventurerType] = new List<Adventurer_Data>();
            }
            adventurerDataDict[data.adventurerType].Add(data);
        }
        this.adventurerDataDict = adventurerDataDict;
    }

    public Adventurer_Data GetAdventurerData(AdventurerType type, string tag)
    {
        if (adventurerDataDict == null)
        {
            SetAdventurerDataDict();
        }

        if (adventurerDataDict.ContainsKey(type))
        {
            foreach (var data in adventurerDataDict[type])
            {
                Debug.Log(data.tag);
                if (data.tag == tag)
                {
                    return data;
                }
            }
        }
        return null;
    }
}
