using AdventurersHaven;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class ConstructionSubDataWrapper<T>
{
    protected T data;

    public ConstructionSubDataWrapper(T data)
    {
        this.data = data;
    }
}

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

public class EquipmentDataWrapper : ConstructionSubDataWrapper<EquipmentCon_Data>, IConstructionSubData
{
    public EquipmentDataWrapper(EquipmentCon_Data data) : base(data) { }

    public string ID => data.id;
    public string Tag => data.tag;
    public string Name => data.name;
    public int BuildCost => data.buildCost;
    public int[] BlockSize => data.blockSize?.ToArray() ?? new int[0];
    public int Sales => data.sales;
    public int SalesIncrement => data.salesIncrement;
    public int UpgradeCost => data.upgradeCost;
    public int CostIncrement => data.costIncrement;
    public int MaxLevel => data.maxLevel;
}

public class RestaurantDataWrapper : ConstructionSubDataWrapper<RestaurantCon_Data>, IConstructionSubData
{
    public RestaurantDataWrapper(RestaurantCon_Data data) : base(data) { }

    public string ID => data.id;
    public string Tag => data.tag;
    public string Name => data.name;
    public int BuildCost => data.buildCost;
    public int[] BlockSize => data.blockSize?.ToArray() ?? new int[0];
    public int Sales => data.sales;
    public int SalesIncrement => data.salesIncrement;
    public int UpgradeCost => data.upgradeCost;
    public int CostIncrement => data.costIncrement;
    public int MaxLevel => data.maxLevel;
}

public class DemolishDataWrapper : ConstructionSubDataWrapper<DemolishCon_Data>, IConstructionSubData
{
    public DemolishDataWrapper(DemolishCon_Data data) : base(data) { }

    public string ID => data.id;
    public string Tag => data.tag;
    public string Name => data.name;
    public int BuildCost => data.buildCost;
    public int[] BlockSize => data.blockSize?.ToArray() ?? new int[0];
    public int Sales => data.sales;
    public int SalesIncrement => data.salesIncrement;
    public int UpgradeCost => data.upgradeCost;
    public int CostIncrement => data.costIncrement;
    public int MaxLevel => data.maxLevel;
}

public class RoadDataWrapper : ConstructionSubDataWrapper<RoadCon_Data>, IConstructionSubData
{
    public RoadDataWrapper(RoadCon_Data data) : base(data) { }

    public string ID => data.id;
    public string Tag => data.tag;
    public string Name => data.name;
    public int BuildCost => data.buildCost;
    public int[] BlockSize => data.blockSize?.ToArray() ?? new int[0];
    public int Sales => data.sales;
    public int SalesIncrement => data.salesIncrement;
    public int UpgradeCost => data.upgradeCost;
    public int CostIncrement => data.costIncrement;
    public int MaxLevel => data.maxLevel;
}

public class ConstructionDataManager
{
    // Constructino 데이터
    private Dictionary<ConstructionType, List<Construction_Data>> constructionDataListDict;
    private Dictionary<string, Construction_Data> constructionDict;

    // Build, Element 데이터
    private Dictionary<string, BuildCon_Data> buildDict;
    private Dictionary<string, ElementCon_Data> elementDict;

    // 최하위 데이터
    private Dictionary<string, EquipmentCon_Data> equipmentDict;
    private Dictionary<string, RestaurantCon_Data> restaurantDict;
    private Dictionary<string, DemolishCon_Data> demolishDict;
    private Dictionary<string, RoadCon_Data> roadDict;
    private Dictionary<string, IConstructionSubData> constructionWrapperDict = new Dictionary<string, IConstructionSubData>();

    public void Init()
    {
        SetAllConstructionData();
        SetConstructionDatListDict();
        SetAllConstructionWrappers();
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

    private string GenerateCacheKey(ConstructionType type, string typeID)
    {
        return $"{type}_{typeID}";
    }

    private void SetAllConstructionWrappers()
    {
        if (buildDict != null)
        {
            foreach (var kvp in buildDict)
            {
                var buildData = kvp.Value;
                if (!Enum.TryParse(buildData.subType, out BuildType buildType)) continue;

                IConstructionSubData wrapper = null;
                switch (buildType)
                {
                    case BuildType.Equipment:
                        if (equipmentDict.TryGetValue(buildData.subTypeID, out var eqData))
                            wrapper = new EquipmentDataWrapper(eqData);
                        break;
                    case BuildType.Restaurant:
                        if (restaurantDict.TryGetValue(buildData.subTypeID, out var resData))
                            wrapper = new RestaurantDataWrapper(resData);
                        break;
                }
                if (wrapper != null)
                    constructionWrapperDict[GenerateCacheKey(ConstructionType.Build, kvp.Key)] = wrapper;
            }
        }

        if (elementDict != null)
        {
            foreach (var kvp in elementDict)
            {
                var elementData = kvp.Value;
                if (!Enum.TryParse(elementData.subType, out ElementType elementType)) continue;

                IConstructionSubData wrapper = null;
                switch (elementType)
                {
                    case ElementType.Demolish:
                        if (demolishDict.TryGetValue(elementData.subTypeID, out var demoData))
                            wrapper = new DemolishDataWrapper(demoData);
                        break;
                    case ElementType.Road:
                        if (roadDict.TryGetValue(elementData.subTypeID, out var roadData))
                            wrapper = new RoadDataWrapper(roadData);
                        break;
                }
                if (wrapper != null)
                    constructionWrapperDict[GenerateCacheKey(ConstructionType.Element, kvp.Key)] = wrapper;
            }
        }
    }

    // GetDeepConstructionData 수정: 캐시에서 반환
    public IConstructionSubData GetDeepConstructionData(ConstructionType type, string typeID)
    {
        if (constructionWrapperDict == null || constructionWrapperDict.Count == 0)
        {
            SetAllConstructionData();
            SetAllConstructionWrappers();
        }

        string key = GenerateCacheKey(type, typeID);
        if (constructionWrapperDict.TryGetValue(key, out var wrapper))
            return wrapper;

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
}
