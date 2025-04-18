using AdventurersHaven;
using GoogleSheet.Type;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class BuildingPlacer : SingletonBase<BuildingPlacer>
{
    private Camera mainCamera;
    private SpriteRenderer previewRenderer;
    
    private GameObject previewObject;
    private Construction previewConstruction;
    private Vector2Int buildingSize;
    private Vector3Int gridPos;

    private bool isConfirmingPlacement; // 현재 배치 기능인지 여부
    private bool isCheck;               // Check, Cancle 어떤 것이 클릭됐는지
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
        if (previewObject == null || !previewObject.activeSelf) return;
        if (isConfirmingPlacement) return;
        
        ChangePreviewObjPos();
        ChangeChildPlace();

        bool canPlace = MapManager.Instance.CanPlaceBuilding(gridPos, buildingSize, previewConstruction.Type);
        ChangeColor(canPlace);
        notPlaceable.SetActive(!canPlace);

        // 마우스 클릭으로 설치
        if (Input.GetMouseButton(0) && canPlace)
        {
            isConfirmingPlacement = true;
            SetPlacementButtonsActive(true);
        }
    }

    private void OnPlacementButtonClicked(bool isCheck)
    {
        this.isCheck = isCheck;

        if (isCheck)
        {
            MapManager.Instance.SetBuildingArea(gridPos, buildingSize, previewObject, previewConstruction.Type);
        }
        else
        {
            PoolManager.Instance.ReturnToPool<Construction>(previewObject.name, previewConstruction);
        }

        previewObject = null;
        gameObject.SetActive(false);
        SetPlacementButtonsActive(false);
        isConfirmingPlacement = false;
        UIManager.Instance.Show<Main>();
    }

    private void ChangePreviewObjPos()
    {
        // 스크린 좌표 (픽셀 단위) → 월드 좌표 (3D 공간상 위치) 변환
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        if (previewConstruction.Type == ConstructionType.Build) 
        {
            // 월드 좌표를 타일맵 상 몇 번째 셀인지 변환
            gridPos = MapManager.Instance.BuildingTilemap.WorldToCell(mouseWorld);
            // 셀의 정중앙 월드 위치 반환
            previewObject.transform.position = GetSnappedPosition(MapManager.Instance.BuildingTilemap);
            Debug.Log("Building Position: " + previewObject.transform.position);
        }
        else if (previewConstruction.Type == ConstructionType.Road)
        {
            gridPos = MapManager.Instance.RoadTilemap.WorldToCell(mouseWorld);
            previewObject.transform.position = GetSnappedPosition(MapManager.Instance.RoadTilemap);
            Debug.Log("Road Position: " + previewObject.transform.position);
        }
    }

    // 프리팹 중앙 값 계산하여 위치 조정
    public Vector3 GetSnappedPosition(Tilemap tilemap)
    {
        // 중심 셀의 실제 월드 중심 좌표
        Vector3 cellCenter = tilemap.GetCellCenterWorld(gridPos);

        // 타일 크기 (보통 1x1이지만 다를 수 있음)
        Vector3 cellSize = tilemap.cellSize;

        // 오프셋 계산 (짝수일 때 왼쪽/아래로 한 칸 더 가도록 보정)
        float offsetX = (buildingSize.x % 2 == 0) ? 0.5f : 0f;
        float offsetY = (buildingSize.y % 2 == 0) ? 0.5f : 0f;

        Vector3 offset = Vector3.right * offsetX * cellSize.x + Vector3.up * offsetY * cellSize.y;

        // 셀 중심에서 오프셋 빼서 실제 좌표 계산
        return cellCenter - offset;
    }

    private void SetPlacementButtonsActive(bool isActive)
    {
        check.gameObject.SetActive(isActive);
        cancle.gameObject.SetActive(isActive);
    }

    private void ChangeChildPlace()
    {
        Vector2 pos = previewObject.transform.position;
        Vector3 bound = previewRenderer.bounds.size;
        float width = bound.x / 2, height = bound.y / 2;

        check.transform.position = pos + height * Vector2.down * 0.9f + width * Vector2.right * 0.4f;
        cancle.transform.position = pos + height * Vector2.down * 0.9f + width * Vector2.left * 0.4f;
        notPlaceable.transform.position = pos + height * Vector2.up * 0.9f;
    }

    private void ChangeColor(bool canPlace)
    {
        Color color = previewRenderer.color;
        color.a = canPlace ? 1.0f : 0.5f;
        previewRenderer.color = color;
    }

    // 원하는 건물 클릭 후 건물 배치 시작
    public void StartPlacing(Construction_Data data, Vector2Int size)
    {
        previewConstruction = PoolManager.Instance.SpawnFromPool<Construction>(data.tag);
        previewConstruction.SetData(data);
        previewObject = previewConstruction.gameObject;
        previewRenderer = previewObject.GetComponent<SpriteRenderer>();
        buildingSize = size;
        gameObject.SetActive(true);
    }
}
