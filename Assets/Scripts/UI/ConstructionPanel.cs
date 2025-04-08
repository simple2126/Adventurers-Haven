using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ConstructionPanel : UIBase
{
    [SerializeField] private Button back;
    [SerializeField] private Button buildButton;
    [SerializeField] private Button roadButton;

    [SerializeField] private GameObject content;
    [SerializeField] private GameObject itemBox;

    private bool isBuildTab = true;
    private List<GameObject> buildItemList = new List<GameObject>();
    private List<GameObject> roadItemList = new List<GameObject>();

    [SerializeField] private PoolManager.PoolConfig[] poolConfigs;
    private Dictionary<string, Sprite> poolSpriteDict = new();

    private ScrollRect scrollRect;

    protected override void Awake()
    {
        base.Awake();
        back.onClick.AddListener(Hide);
        buildButton.onClick.AddListener(() => ChangeTab(ConstructionType.Build));
        roadButton.onClick.AddListener(() => ChangeTab(ConstructionType.Road));
        scrollRect = GetComponentInChildren<ScrollRect>();
        PoolManager.Instance.AddPools<Construction>(poolConfigs);
    }

    private void Start()
    {
        SetSpriteDict();
        SetItemList(ConstructionType.Build, buildItemList);
        SetItemList(ConstructionType.Road, roadItemList);
        UpdateTabVisibility();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    private void SetSpriteDict()
    {
        foreach (var config in poolConfigs)
        {
            var sprite = config.Prefab.GetComponent<SpriteRenderer>().sprite;
            poolSpriteDict.Add(config.Tag, sprite);
        }
    }

    private void SetItemList(ConstructionType type, List<GameObject> itemList)
    {
        var dataList = DataManager.Instance.GetConstructionDataDict()[type];

        foreach (var data in dataList)
        {
            var item = Instantiate(itemBox, content.transform);
            if (poolSpriteDict.ContainsKey(data.name.ToString()))
            {
                item.GetComponent<Image>().sprite = poolSpriteDict[data.name.ToString()];
            }
            itemList.Add(item);
            item.SetActive(type == ConstructionType.Build ? isBuildTab : !isBuildTab);
        }
    }

    private void ChangeTab(ConstructionType type)
    {
        bool isBuildSelected = (type == ConstructionType.Build);

        if (isBuildTab == isBuildSelected) return;

        isBuildTab = isBuildSelected;

        UpdateTabVisibility();
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    private void UpdateTabVisibility()
    {
        foreach (var item in buildItemList)
        {
            item.SetActive(isBuildTab);
        }
        foreach (var item in roadItemList)
        {
            item.SetActive(!isBuildTab);
        }
    }
}
