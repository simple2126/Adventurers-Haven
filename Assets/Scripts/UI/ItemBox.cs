using AdventurersHaven;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemBox : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI blockSize;
    [SerializeField] private TextMeshProUGUI buildCost;

    private Construction_Data data;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(CreateItem);
    }

    public void SetData(Sprite sprite, Construction_Data data)
    {
        image.sprite = sprite;
        itemName.text = data.name;
        blockSize.text = $"{data.blockSize[0] * data.blockSize[1]}칸";
        buildCost.text = $"{data.buildCost}G";
        SetOutline(blockSize);
        SetOutline(buildCost);
        this.data = data;
    }

    private void SetOutline(TextMeshProUGUI tmp)
    {
        // Outline 설정
        tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.1f);
        tmp.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
    }

    private void CreateItem()
    {
        Vector2Int size = Vector2Int.right * data.blockSize[0] + Vector2Int.up * data.blockSize[1];
        BuildingPlacer.Instance.StartPlacing(data.tag, size);
        UIManager.Instance.Hide<ConstructionPanel>();
        UIManager.Instance.Hide<Main>();
    }
}
