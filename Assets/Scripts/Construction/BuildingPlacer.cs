using UnityEngine;
using UnityEngine.UI;

public class BuildingPlacer : SingletonBase<BuildingPlacer>
{
    private Camera mainCamera;
    private SpriteRenderer previewRenderer;
    
    private GameObject previewObject;
    private Construction previewConstruction;
    private Vector2Int buildingSize;
    private Vector3Int gridPos;

    private bool isConfirmingPlacement;
    private bool isCheck;
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

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        gridPos = MapManager.Instance.BuildingTilemap.WorldToCell(mouseWorld);
        previewObject.transform.position = MapManager.Instance.BuildingTilemap.GetCellCenterWorld(gridPos);
        ChangeChildPlace();

        bool canPlace = MapManager.Instance.CanPlaceBuilding(gridPos, buildingSize);
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
            MapManager.Instance.SetBuildingArea(gridPos, buildingSize, previewObject);
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
