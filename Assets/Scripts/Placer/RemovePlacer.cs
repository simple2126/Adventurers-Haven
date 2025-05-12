using AdventurersHaven;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RemovePlacer : BasePlacer
{
    private Vector2Int removeSize;
    public override bool RequiresPreview => true;

    public RemovePlacer(Camera camera, Button check, Button cancel, GameObject notPlaceable)
        : base(camera, check, cancel, notPlaceable)
    {
    }

    public override void StartPlacing(Construction_Data data, Construction construction, Vector2Int size)
    {
        base.StartPlacing(data, construction, size);
        removeSize = Vector2Int.right * size.x + Vector2Int.up * size.y;
        notPlaceableIndicator.GetComponent<TextMeshProUGUI>().text = "제거불가!";
    }

    public override void UpdatePlacementInternal()
    {
        bool canPlace = MapManager.Instance.CanPlaceBuilding(gridPos, removeSize, previewConstruction);
        bool sameObjectInArea = MapManager.Instance.CurrentSizeInOneObject(gridPos, removeSize);

        // 두 조건을 모두 만족해야 함
        bool canRemove = canPlace && sameObjectInArea;

        ChangeColor(canRemove);
        notPlaceableIndicator.SetActive(!canRemove);

        if (InputManager.Instance.IsInputDown() && canRemove)
        {
            SetPlacementButtonsActive(true);
        }
    }

    protected override void Place()
    {
        MapManager.Instance.RemoveBuildingArea(gridPos, previewConstruction);
    }
}
