using AdventurersHaven;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemBox : UIBase
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI blockSize;
    [SerializeField] private TextMeshProUGUI buildCost;

    private Construction_Data data;
    private IConstructionSubData subData;

    protected override void Awake()
    {
        base.Awake();
        GetComponent<Button>().onClick.AddListener(CreateItem);
    }

    public void SetData(Sprite sprite, Construction_Data data)
    {
        image.sprite = sprite;

        var subDate = DataManager.Instance.GetDeepConstructionData(data.constructionType, data.subTypeID);

        itemName.text = subDate.Name;
        blockSize.text = $"{subDate.BlockSize[0] * subDate.BlockSize[1]}칸";
        buildCost.text = $"{subDate.BuildCost}G";
        SetOutline(blockSize);
        SetOutline(buildCost);
        this.data = data;
        this.subData = subDate;
    }

    private void SetOutline(TextMeshProUGUI tmp)
    {
        // Outline 설정
        tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.1f);
        tmp.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
    }

    private void CreateItem()
    {
        PlacerManager.Instance.StartPlacing(data, subData);
        UIManager.Instance.Hide<ConstructionPanel>();
        UIManager.Instance.Hide<MenuButtons>();
    }
}
