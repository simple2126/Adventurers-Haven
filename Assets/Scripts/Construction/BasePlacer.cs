using AdventurersHaven;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public enum PlacementState
{
    None,
    Placing,
    Confirming
}

public abstract class BasePlacer
{
    protected Camera mainCamera;
    protected SpriteRenderer previewRenderer;
    protected Construction previewConstruction;
    protected Vector2Int buildingSize;
    protected Vector3Int gridPos;
    protected Construction_Data data;
    protected PlacementState state = PlacementState.None;

    protected Button checkButton;
    protected Vector2 checkButtonBound;
    protected Button cancelButton;
    protected GameObject notPlaceableIndicator;

    public BasePlacer(Camera camera, Button check, Button cancel, GameObject notPlaceable)
    {
        mainCamera = camera;
        checkButton = check;
        cancelButton = cancel;
        notPlaceableIndicator = notPlaceable;
    }

    // ItemBox를 선택한 후 실행
    public virtual void StartPlacing(Construction_Data data, Construction construction, Vector2Int size)
    {
        this.data = data;
        if(previewConstruction != null)
        {
            PoolManager.Instance.ReturnToPool<Construction>(previewConstruction.Tag, previewConstruction);
        }
        previewConstruction = construction;
        previewConstruction.SetData(data);
        previewRenderer = previewConstruction.gameObject.GetComponent<SpriteRenderer>();
        buildingSize = size;
        var checkButtonRect = checkButton.GetComponent<RectTransform>();
        checkButtonBound = checkButtonRect.rect.size * checkButtonRect.localScale;
        state = PlacementState.Placing;

        SetPlacementButtonsActive(false);
    }

    public virtual void Update()
    {
        if (previewConstruction == null || !previewConstruction.gameObject.activeSelf) return;
        if (state == PlacementState.Confirming) return;

        ChangePreviewObjPos();
        ChangeChildPlace();
        UpdatePlacement();
    }

    // 배치 확정
    public virtual void OnConfirm()
    {
        Place();
        Exit();
    }

    // 배치 취소
    public virtual void OnCancel()
    {
        PoolManager.Instance.ReturnToPool<Construction>(data.tag, previewConstruction);
        Exit();
    }

    // 배치 확정 및 취소 시 실행되는 초기화
    protected virtual void Exit()
    {
        SetPlacementButtonsActive(false);
        previewConstruction = null;
        state = PlacementState.None;
    }

    // 프리뷰 오즈젝트 위치를 MapManager에 저장
    protected virtual void Place()
    {
        MapManager.Instance.SetBuildingArea(gridPos, buildingSize, previewConstruction);
    }

    protected abstract void UpdatePlacement();

    // 현제 프리뷰 오브젝트의 위치 변경
    protected virtual void ChangePreviewObjPos()
    {
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Tilemap targetTilemap = previewConstruction.Type == ConstructionType.Build
            ? MapManager.Instance.BuildingTilemap : MapManager.Instance.ElementTilemap;

        gridPos = targetTilemap.WorldToCell(mouseWorld);

        previewConstruction.gameObject.transform.position = GetSnappedPosition(targetTilemap, gridPos);
    }

    // 사이즈에 맞게 프리뷰 오브젝트의 위치 변경
    protected virtual Vector3 GetSnappedPosition(Tilemap tilemap, Vector3Int pos)
    {
        Vector3 cellCenter = tilemap.GetCellCenterWorld(pos);
        Vector3 cellSize = tilemap.cellSize;

        float offsetX = (buildingSize.x % 2 == 0) ? 0.5f : 0f;
        float offsetY = (buildingSize.y % 2 == 0) ? 0.5f : 0f;

        Vector3 offset = Vector3.right * offsetX * cellSize.x + Vector3.up * offsetY * cellSize.y;
        return cellCenter - offset;
    }

    // 프리뷰 오브젝트의 자식 오브젝트의 위치 변경
    protected void ChangeChildPlace()
    {
        Vector2 pos = previewConstruction.gameObject.transform.position;
        float width = checkButtonBound.x / 2, height = buildingSize.y / 2f;
        Vector2 upOrDown = mainCamera.transform.position.y > 0 ? Vector2.down : Vector2.up;

        checkButton.transform.position = pos + height * upOrDown * 0.4f + width * Vector2.right * 0.4f;
        cancelButton.transform.position = pos + height * upOrDown * 0.4f + width * Vector2.left * 0.4f;
        notPlaceableIndicator.transform.position = pos + height * Vector2.up * 0.4f;
    }

    // 프리뷰 오브젝트의 색상(alpha)을 변경
    protected void ChangeColor(bool canPlace)
    {
        Color color = previewRenderer.color;
        color.a = canPlace ? 1.0f : 0.5f;
        previewRenderer.color = color;
    }

    // 프리뷰 오브젝트의 배치(check, cancel) 활성화/비활성화
    protected void SetPlacementButtonsActive(bool isActive)
    {
        checkButton.gameObject.SetActive(isActive);
        cancelButton.gameObject.SetActive(isActive);
    }

    // 프리뷰 오브젝트의 배치 가능 여부를 판단
    protected bool CanPlaceAtCurrentPosition()
    {
        return MapManager.Instance.CanPlaceBuilding(gridPos, buildingSize, previewConstruction);
    }
}