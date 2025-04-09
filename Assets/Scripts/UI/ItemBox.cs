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

    public void SetData(Sprite sprite, Construction_Data data)
    {
        image.sprite = sprite;
        itemName.text = data.name;
        blockSize.text = $"{data.blockSize}칸";
        buildCost.text = $"{data.buildCost}G";
        SetOutline(blockSize);
        SetOutline(buildCost);
    }

    private void SetOutline(TextMeshProUGUI tmp)
    {
        Material newMat = new Material(tmp.fontMaterial);
        tmp.fontMaterial = newMat;

        // Outline 설정
        tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.1f);
        tmp.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
    }
}
