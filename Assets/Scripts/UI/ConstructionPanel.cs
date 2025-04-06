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

    protected override void Awake()
    {
        base.Awake();
        back.onClick.AddListener(Hide);
        buildButton.onClick.AddListener(() => ChangeTab(ConstructionType.Build));
        roadButton.onClick.AddListener(() => ChangeTab(ConstructionType.Road));
    }

    private void Start()
    {
        InitializeItemList(ConstructionType.Build, buildItemList);
        InitializeItemList(ConstructionType.Road, roadItemList);
        UpdateTabVisibility();
    }

    private void InitializeItemList(ConstructionType type, List<GameObject> itemList)
    {
        var dataList = DataManager.Instance.GetConstructionDataDict()[type];

        foreach (var data in dataList)
        {
            var item = Instantiate(itemBox, content.transform);
            itemList.Add(item);
            item.SetActive(type == ConstructionType.Build ? isBuildTab : !isBuildTab);
        }
    }

    private void ChangeTab(ConstructionType type)
    {
        isBuildTab = (type == ConstructionType.Build);
        UpdateTabVisibility();
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
