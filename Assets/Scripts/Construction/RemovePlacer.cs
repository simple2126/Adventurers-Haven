using AdventurersHaven;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RemovePlacer : BasePlacer
{
    private Vector3Int removeSize;

    public RemovePlacer(Camera camera, Button check, Button cancel, GameObject notPlaceable)
        : base(camera, check, cancel, notPlaceable)
    {
    }

    public override void StartPlacing(Construction_Data data, Construction construction, Vector2Int size)
    {
        base.StartPlacing(data, construction, size);
        removeSize = Vector3Int.right * size.x + Vector3Int.up * size.y;
        notPlaceableIndicator.GetComponent<TextMeshProUGUI>().text = "제거불가!";
    }

    protected override void UpdatePlacement()
    {
        bool canPlace = CanPlaceAtCurrentPosition();

        // 현재 범위 내 오브젝트가 같은지 검사
        Vector2Int size = MapManager.Instance.GetConstructionSize(gridPos);
        bool sameObjectInArea = MapManager.Instance.CurrentSizeInOneObject(gridPos, size);

        // 두 조건을 모두 만족해야 함
        bool canRemove = canPlace && sameObjectInArea;

        ChangeColor(canRemove);
        notPlaceableIndicator.SetActive(!canRemove);

        if (canRemove) ChangeScale(size);

        if (Input.GetMouseButtonDown(0) && canRemove)
        {
            state = PlacementState.Confirming;
            SetPlacementButtonsActive(true);
        }
    }

    private void ChangeScale(Vector2Int size)
    {
        buildingSize = Vector2Int.right * removeSize.x + Vector2Int.up * removeSize.y; // 초기화
        float xRatio = (float)size.x / buildingSize.x;
        float yRatio = (float)size.y / buildingSize.y;

        // 프리팹의 새로운 크기 계산
        previewConstruction.transform.localScale = removeSize; // 초기화
        Vector3 newScale = previewConstruction.transform.localScale;
        newScale.x *= xRatio;
        newScale.y *= yRatio;

        // 프리팹 크기 적용
        if (MapManager.Instance.CurrentSizeInOneObject(gridPos, size))
        {
            previewConstruction.transform.localScale = newScale;
            buildingSize = size;
        }
    }

    protected override void Place()
    {
        MapManager.Instance.RemoveBuildingArea(gridPos, buildingSize, previewConstruction);
    }
}
