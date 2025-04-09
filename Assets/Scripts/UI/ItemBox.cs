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
        blockSize.text = $"{data.blockSize}칸";
        buildCost.text = $"{data.buildCost}G";
        SetOutline(blockSize);
        SetOutline(buildCost);
        this.data = data;
    }

    private void SetOutline(TextMeshProUGUI tmp)
    {
        Material newMat = new Material(tmp.fontMaterial);
        tmp.fontMaterial = newMat;

        // Outline 설정
        tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.1f);
        tmp.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
    }

    private void CreateItem()
    {
        PoolManager.Instance.SpawnFromPool<Construction>(data.tag);
        UIManager.Instance.Hide<ConstructionPanel>();
    }
}
