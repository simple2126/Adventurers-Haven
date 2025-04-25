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
    protected Button cancelButton;
    protected GameObject notPlaceableIndicator;

    public BasePlacer(Camera camera, Button check, Button cancel, GameObject notPlaceable)
    {
        mainCamera = camera;
        checkButton = check;
        cancelButton = cancel;
        notPlaceableIndicator = notPlaceable;
    }

    public virtual void StartPlacing(Construction_Data data, Vector2Int size)
    {
        this.data = data;
        previewConstruction = PoolManager.Instance.SpawnFromPool<Construction>(data.tag);
        previewConstruction.SetData(data);
        previewRenderer = previewConstruction.gameObject.GetComponent<SpriteRenderer>();
        buildingSize = size;
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

    public virtual void OnConfirm()
    {
        Place();
        Exit();
    }

    public virtual void OnCancel()
    {
        PoolManager.Instance.ReturnToPool<Construction>(data.tag, previewConstruction);
        Exit();
    }

    protected virtual void Exit()
    {
        SetPlacementButtonsActive(false);
        previewConstruction = null;
        state = PlacementState.None;
    }

    protected virtual void Place()
    {
        MapManager.Instance.SetBuildingArea(gridPos, buildingSize, previewConstruction);
    }

    protected abstract void UpdatePlacement();

    protected virtual void ChangePreviewObjPos()
    {
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Tilemap targetTilemap = previewConstruction.Type == ConstructionType.Build
            ? MapManager.Instance.BuildingTilemap : MapManager.Instance.ElementTilemap;

        gridPos = targetTilemap.WorldToCell(mouseWorld);

        previewConstruction.gameObject.transform.position = GetSnappedPosition(targetTilemap, gridPos);
    }

    protected virtual Vector3 GetSnappedPosition(Tilemap tilemap, Vector3Int pos)
    {
        Vector3 cellCenter = tilemap.GetCellCenterWorld(pos);
        Vector3 cellSize = tilemap.cellSize;

        float offsetX = (buildingSize.x % 2 == 0) ? 0.5f : 0f;
        float offsetY = (buildingSize.y % 2 == 0) ? 0.5f : 0f;

        Vector3 offset = Vector3.right * offsetX * cellSize.x + Vector3.up * offsetY * cellSize.y;
        return cellCenter - offset;
    }

    protected void ChangeChildPlace()
    {
        Vector2 pos = previewConstruction.gameObject.transform.position;
        Vector3 bound = previewRenderer.bounds.size;
        float width = bound.x / 2, height = bound.y / 2;
        Vector2 upOrDown = mainCamera.transform.position.y > 0 ? Vector2.down : Vector2.up;

        checkButton.transform.position = pos + height * upOrDown * 0.9f + width * Vector2.right * 0.4f;
        cancelButton.transform.position = pos + height * upOrDown * 0.9f + width * Vector2.left * 0.4f;
        notPlaceableIndicator.transform.position = pos + height * Vector2.up * 0.9f;
    }

    protected void ChangeColor(bool canPlace)
    {
        Color color = previewRenderer.color;
        color.a = canPlace ? 1.0f : 0.5f;
        previewRenderer.color = color;
    }

    protected void SetPlacementButtonsActive(bool isActive)
    {
        checkButton.gameObject.SetActive(isActive);
        cancelButton.gameObject.SetActive(isActive);
    }

    protected bool CanPlaceAtCurrentPosition()
    {
        return MapManager.Instance.CanPlaceBuilding(gridPos, buildingSize, previewConstruction.Type);
    }
}