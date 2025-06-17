using AdventurersHaven;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IConstructionSubData
{
    string ID { get; }         
    string Tag { get; }        
    string Name { get; }       
    int BuildCost { get; }  
    int[] BlockSize { get; } 
    int Sales { get; } 
    int SalesIncrement { get; } 
    int UpgradeCost { get; }
    int CostIncrement { get; }
    int MaxLevel { get; }
}


public class DataManager : SingletonBase<DataManager>
{
    private Dictionary<BgmType, float> individualBgmVolumeDict;
    private Dictionary<SfxType, float> individualSfxVolumeDict;

    // Constructino 데이터
    Dictionary<ConstructionType, List<Construction_Data>> constructionDataListDict;
    Dictionary<string, Construction_Data> constructionDict;

    // Build, Element 데이터
    Dictionary<string, BuildCon_Data> buildDict;
    Dictionary<string, ElementCon_Data> elementDict;

    // 최하위 데이터
    Dictionary<string, EquipmentCon_Data> equipmentDict;
    Dictionary<string, RestaurantCon_Data> restaurantDict;
    Dictionary<string, DemolishCon_Data> demolishDict;
    Dictionary<string, RoadCon_Data> roadDict;

    private Dictionary<AdventurerType, List<Adventurer_Data>> adventurerDataDict;
    
    protected override void Awake()
    {
        base.Awake();

        SetIndividualSfxVolumeDict();
        SetIndividualBgmVolumeDict();
        SetAllConstructionData();
        SetConstructionDatListDict();
        SetAdventurerDataDict();
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

    private void SetAllConstructionData()
    {
        constructionDict = Construction_Data.GetList().ToDictionary(c => c.id);
        buildDict = BuildCon_Data.GetList().ToDictionary(b => b.id);
        elementDict = ElementCon_Data.GetList().ToDictionary(e => e.id);
        equipmentDict = EquipmentCon_Data.GetList().ToDictionary(e => e.id);
        restaurantDict = RestaurantCon_Data.GetList().ToDictionary(r => r.id);
        demolishDict = DemolishCon_Data.GetList().ToDictionary(d => d.id);
        roadDict = RoadCon_Data.GetList().ToDictionary(r => r.id);

        // Construction 데이터
        Debug.Log("==== ConstructionCon_Data ====");
        foreach (var kvp in constructionDict)
        {
            Debug.Log($"ID: {kvp.Key}, Type: {kvp.Value.constructionType}, SubTypeID: {kvp.Value.subTypeID}");
        }

        // Build 데이터
        Debug.Log("==== BuildCon_Data ====");
        foreach (var kvp in buildDict)
        {
            Debug.Log($"ID: {kvp.Key}, SubType: {kvp.Value.subType}, SubTypeID: {kvp.Value.subTypeID}");
        }

        // Element 데이터
        Debug.Log("==== ElementCon_Data ====");
        foreach (var kvp in elementDict)
        {
            Debug.Log($"ID: {kvp.Key}, SubType: {kvp.Value.subType}, SubTypeID: {kvp.Value.subTypeID}");
        }

        // Equipment
        Debug.Log("==== EquipmentCon_Data ====");
        foreach (var kvp in equipmentDict)
        {
            Debug.Log($"ID: {kvp.Key}, Size: {kvp.Value.blockSize[0]}x{kvp.Value.blockSize[1]}");
        }

        // Restaurant
        Debug.Log("==== RestaurantCon_Data ====");
        foreach (var kvp in restaurantDict)
        {
            Debug.Log($"ID: {kvp.Key}, Size: {kvp.Value.blockSize[0]}x{kvp.Value.blockSize[1]}");

        }

        // Demolish
        Debug.Log("==== DemolishCon_Data ====");
        foreach (var kvp in demolishDict)
        {
            Debug.Log($"ID: {kvp.Key}, Size: {kvp.Value.blockSize[0]}x{kvp.Value.blockSize[1]}");
        }

        // Road
        Debug.Log("==== RoadCon_Data ====");
        foreach (var kvp in roadDict)
        {
            Debug.Log($"ID: {kvp.Key}, Size: {kvp.Value.blockSize[0]}x{kvp.Value.blockSize[1]}");
        }
    }

    private void SetConstructionDatListDict()
    {
        List<Construction_Data> _constructionDataList = Construction_Data.GetList();
        Dictionary<ConstructionType, List<Construction_Data>> constructionDataDict = new Dictionary<ConstructionType, List<Construction_Data>>();

        foreach (var data in _constructionDataList)
        {
            if (!constructionDataDict.ContainsKey(data.constructionType))
            {
                constructionDataDict[data.constructionType] = new List<Construction_Data>();
            }
            constructionDataDict[data.constructionType].Add(data);
        }

        this.constructionDataListDict = constructionDataDict;
    }

    public List<Construction_Data> GetConstructionDataList(ConstructionType type)
    {
        if (constructionDataListDict == null)
        {
            SetConstructionDatListDict();
        }
        if (constructionDataListDict.TryGetValue(type, out var dataList))
        {
            return dataList;
        }
        return new List<Construction_Data>();
    }

    public IConstructionSubData GetDeepConstructionData(ConstructionType type, string typeID)
    {
        if (constructionDict == null) { SetAllConstructionData(); }

        switch (type)
        {
            case ConstructionType.Build:
                if (!buildDict.TryGetValue(typeID, out var buildData)) return null;
                if (!Enum.TryParse(buildData.subType, out BuildType buildType)) return null;

                switch (buildType)
                {
                    case BuildType.Equipment:
                        return equipmentDict.TryGetValue(buildData.subTypeID, out var eqData)
                            ? eqData as IConstructionSubData : null;

                    case BuildType.Restaurant:
                        return restaurantDict.TryGetValue(buildData.subTypeID, out var resData)
                            ? resData as IConstructionSubData : null;
                }
                break;

            case ConstructionType.Element:
                if (!elementDict.TryGetValue(typeID, out var elementData)) return null;
                Debug.Log($"Element Data: {elementData.id}, SubType: {elementData.subType}, SubTypeID: {elementData.subTypeID}");

                if (!Enum.TryParse(elementData.subType, out ElementType elementType)) return null;
                Debug.Log($"Parsed ElementType: {elementType}");

                switch (elementType)
                {
                    case ElementType.Demolish:
                        return demolishDict.TryGetValue(elementData.subTypeID, out var demoData)
                            ? demoData as IConstructionSubData : null;
                    case ElementType.Road:
                        return roadDict.TryGetValue(elementData.subTypeID, out var roadData)
                            ? roadData as IConstructionSubData : null;
                }
                break;
        }

        return null;
    }

    public string GetMiddleSubType(ConstructionType type, string subTypeID)
    {
        switch (type)
        {
            case ConstructionType.Build:
                if (buildDict.TryGetValue(subTypeID, out var buildData))
                {
                    return buildData.subType;
                }
                break;
            case ConstructionType.Element:
                if (elementDict.TryGetValue(subTypeID, out var elementData))
                {
                    return elementData.subType;
                }
                break;
        }
        return null;
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
