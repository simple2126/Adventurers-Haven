using UnityEngine;

public class BuildingPlacer : SingletonBase<BuildingPlacer>
{
    private Camera mainCamera;
    private GameObject previewObject;
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

        Color color = canPlace ? Color.cyan : Color.magenta;
        color.a = 0.5f;
        previewRenderer.color = color;

        // 마우스 클릭으로 설치
        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            Construction obj = PoolManager.Instance.SpawnFromPool<Construction>(previewObject.name, previewObject.transform.position, Quaternion.identity);
            MapManager.Instance.SetBuildingArea(gridPos, buildingSize, obj.gameObject);
            gameObject.SetActive(false);
            UIManager.Instance.Show<Main>();
        }
    }

    public void StartPlacing(string tag, Vector2Int size)
    {
        previewObject = PoolManager.Instance.SpawnFromPool<Construction>(tag).gameObject;
        previewRenderer = previewObject.GetComponent<SpriteRenderer>();
        buildingSize = size;
        gameObject.SetActive(true);
    }

    public void CancelPlacing()
    {
        gameObject.SetActive(false);
    }
}
