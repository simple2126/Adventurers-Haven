using UnityEngine;

public class BuildingPlacer : SingletonBase<BuildingPlacer>
{
    private Camera mainCamera;
    private GameObject previewObject;
    private Construction previewConstruction;
    private Vector2Int buildingSize;
    private SpriteRenderer previewRenderer;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (previewObject == null || !previewObject.activeSelf) return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPos = MapManager.Instance.BuildingTilemap.WorldToCell(mouseWorld);

        previewObject.transform.position = MapManager.Instance.BuildingTilemap.GetCellCenterWorld(gridPos);

        bool canPlace = MapManager.Instance.CanPlaceBuilding(gridPos, buildingSize);
        ChangeColor(canPlace);

        // 마우스 클릭으로 설치
        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            PoolManager.Instance.ReturnToPool<Construction>(previewObject.name, previewConstruction);
            Construction obj = PoolManager.Instance.SpawnFromPool<Construction>(previewObject.name, previewObject.transform.position, Quaternion.identity);
            MapManager.Instance.SetBuildingArea(gridPos, buildingSize, obj.gameObject);
            gameObject.SetActive(false);
            UIManager.Instance.Show<Main>();
        }
    }

    private void ChangeColor(bool canPlace)
    {
        Color color = previewRenderer.color;
        color.a = canPlace ? 1.0f : 0.5f;
        previewRenderer.color = color;
    }

    public void StartPlacing(string tag, Vector2Int size)
    {
        previewConstruction = PoolManager.Instance.SpawnFromPool<Construction>(tag);
        previewObject = previewConstruction.gameObject;
        previewRenderer = previewObject.GetComponent<SpriteRenderer>();
        buildingSize = size;
        gameObject.SetActive(true);
    }

    public void CancelPlacing()
    {
        gameObject.SetActive(false);
    }
}
