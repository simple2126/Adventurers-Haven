using AdventurersHaven;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class BuildingPlacer : SingletonBase<BuildingPlacer>
{
    private Camera mainCamera;
    private SpriteRenderer previewRenderer;

    private Construction_Data data;
    private Construction previewConstruction;
    private Vector2Int buildingSize;
    private Vector3Int gridPos;

    private bool isConfirmingPlacement; // 현재 배치 기능인지 여부
    private Vector3Int roadStartPos;
    private Vector3Int roadEndPos;

    private RoadPlacementState roadState = RoadPlacementState.None;

    [SerializeField] private Button check;
    [SerializeField] private Button cancle;
    [SerializeField] private GameObject notPlaceable;

    protected override void Awake()
    {
        base.Awake();
        check.onClick.AddListener(() => OnPlacementButtonClicked(true));
        cancle.onClick.AddListener(() => OnPlacementButtonClicked(false));
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (previewConstruction == null || !previewConstruction.gameObject.activeSelf) return;
        if (isConfirmingPlacement && roadState != RoadPlacementState.ReadyToConfirm) return;

        ChangePreviewObjPos();
        ChangeChildPlace();

        if (!IsRoad())
        {
            UpdateDefaultPlacement();
        }
        else
        {
            UpdateRoadPlacement();
        }
    }

    private void OnPlacementButtonClicked(bool isCheck)
    {
        var previewObj = previewConstruction.gameObject;
        if (isCheck)
        {
            if (IsRoad())
                PlaceRoadLine(roadStartPos, roadEndPos);
            else
                MapManager.Instance.SetBuildingArea(gridPos, buildingSize, previewObj, previewConstruction.Type);
        }
        else
        {
            PoolManager.Instance.ReturnToPool<Construction>(data.tag, previewConstruction);
        }

        ExitPlacing();
        UIManager.Instance.Show<Main>();
    }

    public void StartPlacing(Construction_Data data, Vector2Int size)
    {
        this.data = data;
        previewConstruction = PoolManager.Instance.SpawnFromPool<Construction>(data.tag);
        previewConstruction.SetData(data);
        previewRenderer = previewConstruction.gameObject.GetComponent<SpriteRenderer>();
        buildingSize = size;
        gameObject.SetActive(true);
        roadState = RoadPlacementState.None;
    }

    private bool IsRoad()
    {
        return previewConstruction.Type == ConstructionType.Element &&
               previewConstruction.ElementType == ElementType.Road;
    }

    // 라인에 여러 도로 배치
    private void PlaceRoadLine(Vector3Int start, Vector3Int end)
    {
        Vector3Int direction = end - start;

        if (Mathf.Abs(direction.x) > 0 && Mathf.Abs(direction.y) > 0) return;

        int stepX = Mathf.Clamp(direction.x, -1, 1);
        int stepY = Mathf.Clamp(direction.y, -1, 1);

        Vector3Int step = Vector3Int.right * stepX * buildingSize.x +
                          Vector3Int.up * stepY * buildingSize.y;

        Vector3Int current = start;
        PoolManager.Instance.ReturnToPool<Construction>(data.tag, previewConstruction);

        while (current != end + step)
        {
            var pos = GetSnappedPosition(MapManager.Instance.ElementTilemap, current);
            var obj = PoolManager.Instance.SpawnFromPool<Construction>(data.tag, pos, Quaternion.identity);
            obj.SetData(data);
            MapManager.Instance.SetBuildingArea(current, Vector2Int.one, obj.gameObject, ConstructionType.Element);
            current += step;
        }
    }

    private void ChangePreviewObjPos()
    {
        if (IsRoad() && roadState == RoadPlacementState.ReadyToConfirm)
            return;

        var previewObj = previewConstruction.gameObject;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        if (previewConstruction.Type == ConstructionType.Build)
        {
            gridPos = MapManager.Instance.BuildingTilemap.WorldToCell(mouseWorld);
            previewObj.transform.position = GetSnappedPosition(MapManager.Instance.BuildingTilemap);
        }
        else if (previewConstruction.Type == ConstructionType.Element)
        {
            gridPos = MapManager.Instance.ElementTilemap.WorldToCell(mouseWorld);
            previewObj.transform.position = GetSnappedPosition(MapManager.Instance.ElementTilemap);
        }
    }

    // 배치 중일 때 그리드 벗어나지 않도록 위치 계산
    private Vector3 GetSnappedPosition(Tilemap tilemap)
    {
        return GetSnappedPosition(tilemap, gridPos);
    }


    private Vector3 GetSnappedPosition(Tilemap tilemap, Vector3Int pos)
    {
        Vector3 cellCenter = tilemap.GetCellCenterWorld(pos);
        Vector3 cellSize = tilemap.cellSize;

        float offsetX = (buildingSize.x % 2 == 0) ? 0.5f : 0f;
        float offsetY = (buildingSize.y % 2 == 0) ? 0.5f : 0f;

        Vector3 offset = Vector3.right * offsetX * cellSize.x + Vector3.up * offsetY * cellSize.y;
        return cellCenter - offset;
    }

    private void ChangeChildPlace()
    {
        Vector2 pos = previewConstruction.gameObject.transform.position;
        Vector3 bound = previewRenderer.bounds.size;
        float width = bound.x / 2, height = bound.y / 2;
        Vector2 upOrDown = transform.position.y > 0 ? Vector2.down : Vector2.up;

        check.transform.position = pos + height * upOrDown * 0.9f + width * Vector2.right * 0.4f;
        cancle.transform.position = pos + height * upOrDown * 0.9f + width * Vector2.left * 0.4f;
        notPlaceable.transform.position = pos + height * Vector2.up * 0.9f;
    }

    private void UpdateDefaultPlacement()
    {
        bool canPlace = MapManager.Instance.CanPlaceBuilding(gridPos, buildingSize, previewConstruction.Type);
        ChangeColor(canPlace);
        notPlaceable.SetActive(!canPlace);

        if (Input.GetMouseButton(0) && canPlace)
        {
            isConfirmingPlacement = true;
            SetPlacementButtonsActive(true);
        }
    }

    private void ChangeColor(bool canPlace)
    {
        Color color = previewRenderer.color;
        color.a = canPlace ? 1.0f : 0.5f;
        previewRenderer.color = color;
    }

    private void UpdateRoadPlacement()
    {
        bool canPlace = MapManager.Instance.CanPlaceBuilding(gridPos, buildingSize, previewConstruction.Type);
        ChangeColor(canPlace);
        notPlaceable.SetActive(!canPlace);

        switch (roadState)
        {
            case RoadPlacementState.None:
                if (Input.GetMouseButtonDown(0))
                {
                    roadStartPos = gridPos;
                    roadState = RoadPlacementState.Dragging;
                }
                break;

            case RoadPlacementState.Dragging:
                roadEndPos = gridPos;
                PreviewRoadLine(roadStartPos, roadEndPos);

                if (Input.GetMouseButtonDown(0))
                {
                    roadState = RoadPlacementState.ReadyToConfirm;
                    isConfirmingPlacement = true;
                    SetPlacementButtonsActive(true);
                }
                break;

            case RoadPlacementState.ReadyToConfirm:
                // 대기 중 (버튼 클릭)
                break;
        }
    }

    private void PreviewRoadLine(Vector3Int start, Vector3Int end)
    {
        // 시각적 프리뷰 추가 가능 (예: LineRenderer 등)
    }

    private void ExitPlacing()
    {
        gameObject.SetActive(false);
        SetPlacementButtonsActive(false);
        isConfirmingPlacement = false;
        roadState = RoadPlacementState.None;
    }

    private void SetPlacementButtonsActive(bool isActive)
    {
        check.gameObject.SetActive(isActive);
        cancle.gameObject.SetActive(isActive);
    }
}
