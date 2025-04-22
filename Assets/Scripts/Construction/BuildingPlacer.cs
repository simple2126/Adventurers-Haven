using AdventurersHaven;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

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
    private List<Construction> previewRoadList = new List<Construction>(); // 프리뷰 도로 리스트

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
        if (isConfirmingPlacement && roadState != RoadPlacementState.Confirm) return;

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
            if (IsRoad()) ReturnRoadList();
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
        Vector3Int step = GetStep(end - start);
        int stepCount = GetStepCount(step, start, end);
        if (stepCount == 0) return;

        Vector3Int current = start;
        for (int i = 0; i <= stepCount; i++)
        {
            current = start + step * i;
            var pos = GetSnappedPosition(MapManager.Instance.ElementTilemap, current);
            var obj = previewRoadList[i];
            obj.SetData(data);
            MapManager.Instance.SetBuildingArea(current, Vector2Int.one, obj.gameObject, ConstructionType.Element);
        }
    }

    private Vector3Int GetStep(Vector3Int direction)
    {
        bool isHorizontal = Mathf.Abs(direction.x) >= Mathf.Abs(direction.y);

        if (isHorizontal)
        {
            int stepX = Mathf.Clamp(direction.x, -1, 1);
            return Vector3Int.right * stepX * buildingSize.x;
        }
        else
        {
            int stepY = Mathf.Clamp(direction.y, -1, 1);
            return Vector3Int.up * stepY * buildingSize.y;
        }
    }

    private int GetStepCount(Vector3Int step, Vector3Int start, Vector3Int end)
    {
        if (step == Vector3Int.zero) return 0;

        if (step.x != 0)
            return Mathf.Abs(end.x - start.x) / Mathf.Abs(step.x);
        else
            return Mathf.Abs(end.y - start.y) / Mathf.Abs(step.y);
    }

    private void ChangePreviewObjPos()
    {
        if (IsRoad() && roadState == RoadPlacementState.Confirm) return;

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
                    roadState = RoadPlacementState.Confirm;
                    isConfirmingPlacement = true;
                    SetPlacementButtonsActive(true);
                }
                break;

            case RoadPlacementState.Confirm:
                break;
        }
    }

    private void PreviewRoadLine(Vector3Int start, Vector3Int end)
    {
        Vector3Int step = GetStep(end - start);
        int stepCount = GetStepCount(step, start, end);
        if (stepCount == 0)
        {
            // 프리뷰 늘렸다가 다시 원점으로 돌아왔을 때
            int previewCount = previewRoadList.Count;
            ReturnRoadList();
            previewRoadList.Clear();
            if (previewCount > 0) roadState = RoadPlacementState.None;
            return;
        }

        Vector3Int current = start;
        int index = 0;
        
        for (int i = 0; i <= stepCount; i++)
        {
            current = start + step * i;
            if (index >= previewRoadList.Count)
            {
                var obj = PoolManager.Instance.SpawnFromPool<Construction>(data.tag);
                obj.SetData(data);
                previewRoadList.Add(obj);
            }

            var preview = previewRoadList[index];
            preview.transform.position = GetSnappedPosition(MapManager.Instance.ElementTilemap, current);
            preview.gameObject.SetActive(true);
            index++;
        }

        if (previewRoadList.Count > 0) 
        {
            previewConstruction.transform.position = previewRoadList[previewRoadList.Count - 1].transform.position;
        }
        
        // 남은 프리뷰 비활성화
        for (int i = index; i < previewRoadList.Count; i++)
        {
            var preview = previewRoadList[i];
            PoolManager.Instance.ReturnToPool<Construction>(data.tag, preview);
            previewRoadList.RemoveAt(i);
        }
    }

    private void ExitPlacing()
    {
        gameObject.SetActive(false);
        SetPlacementButtonsActive(false);
        isConfirmingPlacement = false;
        roadState = RoadPlacementState.None;
        previewConstruction = null;
        previewRoadList.Clear();
    }

    private void ReturnRoadList()
    {
        if (previewRoadList.Count == 0) return;

        foreach (var obj in previewRoadList)
            PoolManager.Instance.ReturnToPool<Construction>(data.tag, obj);
    }

    private void SetPlacementButtonsActive(bool isActive)
    {
        check.gameObject.SetActive(isActive);
        cancle.gameObject.SetActive(isActive);
    }
}
