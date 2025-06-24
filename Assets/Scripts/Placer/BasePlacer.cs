using AdventurersHaven;
using GoogleSheet.Type;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum PlacementState
{
    Idle,
    Placing,
    Preview,
}

// Placer 상태 클래스들이 사용할 최소한의 기능만 정의한 인터페이스
public interface IPlacerContext
{
    void SetPlacementButtonsActive(bool show);  // 확인/취소 버튼 보이기/숨기기
    Tilemap GetTilemap();                       // 현재 사용 중인 Tilemap 반환

    bool RequiresPreview { get; }               // Preview 단계 필요 여부
    void UpdatePlacement();                     // DefaultPlacer 용 위치/색상 갱신
    // 드래그 처리용 메서드
    void OnTouchDragUpdate(); // 터치 드래그 시 호출
    void OnLineDragUpdate();
    void OnInitialDragEnd();
}

// Placer의 상태를 표현하는 State 패턴용 인터페이스
interface IPlacerState
{
    void HandleInput(); // 입력 처리
    void UpdateLogic(); // 상태별 매 프레임 로직
}

/// 건물/도로/제거 배치 공통 로직
/// IPlacerContext 구현체로 상태 클래스에 기능 제공
public abstract class BasePlacer : IPlacerContext
{
    // ----- protected 필드 -----
    protected Construction_Data data;
    protected Camera mainCamera;
    protected Construction previewConstruction;
    protected SpriteRenderer previewRenderer;
    protected BoxCollider2D previewCollider;
    protected Vector2Int buildingSize;
    protected Vector3Int gridPos;

    protected Vector2 checkButtonBound;
    protected Button checkButton;
    protected Button cancelButton;
    protected GameObject notPlaceableIndicator;
    protected Vector2 accumulatedDrag; // 드래그 이동량

    // State 패턴 관리용
    private IPlacerState currentState;
    private readonly Dictionary<PlacementState, IPlacerState> states;

    /// DefaultPlacer: false, Road/RemovePlacer: true
    /// 각 Placer가 Preview 단계를 사용하는지 여부
    public abstract bool RequiresPreview { get; }

    private float swipeThresholdPixels = 50f;

    /// 생성자: 상태 객체 등록 및 초기 상태 세팅
    public BasePlacer(Camera camera, Button check, Button cancel, GameObject notPlaceable)
    {
        mainCamera = camera;
        checkButton = check;
        cancelButton = cancel;
        notPlaceableIndicator = notPlaceable;

        // 상태 딕셔너리 초기화
        states = new Dictionary<PlacementState, IPlacerState>
        {
            { PlacementState.Idle,    new IdleState(this) },
            { PlacementState.Placing, new PlacingState(this) },
            { PlacementState.Preview, new PreviewState(this) }
        };
    }

    public void SetPlacementButtonsActive(bool show)
    {
        checkButton.gameObject.SetActive(show);
        cancelButton.gameObject.SetActive(show);
    }

    public Tilemap GetTilemap()
    {
        return previewConstruction.Type == ConstructionType.Build
            ? MapManager.Instance.BuildingTilemap
            : MapManager.Instance.ElementTilemap;
    }

    public abstract void UpdatePlacement();

    bool IPlacerContext.RequiresPreview => RequiresPreview;

    /// 배치 시작: Pool 반환, preview 초기화, 버튼 활성화
    public virtual void StartPlacing(Construction_Data data, Construction construction)
    {
        this.data = data;
        if (previewConstruction != null)
            PoolManager.Instance.ReturnToPool<Construction>(previewConstruction.Tag, previewConstruction);

        previewConstruction = construction;
        previewConstruction.transform.position = Vector3.zero;
        previewRenderer = previewConstruction.gameObject.GetComponent<SpriteRenderer>();
        previewConstruction.TryGetComponent<BoxCollider2D>(out previewCollider);
        if(previewCollider != null) previewCollider.enabled = false; // Collider 비활성화 (배치 전 충돌 방지)
        buildingSize = construction.Size;

        // 버튼 범위 계산 및 메시 설정
        var checkButtonRect = checkButton.GetComponent<RectTransform>();
        checkButtonBound = checkButtonRect.rect.size * checkButtonRect.localScale;
        notPlaceableIndicator.GetComponent<TextMeshProUGUI>().text = "배치불가!";
        SetPlacementButtonsActive(true);

        Vector2 pos = previewConstruction.transform.position;
        gridPos = Vector3Int.right * Mathf.CeilToInt(pos.x) + Vector3Int.up * Mathf.CeilToInt(pos.y);
        previewConstruction.transform.position = GetSnappedPosition(GetTilemap(), gridPos);
        UpdatePlacement();
        TransitionTo(PlacementState.Idle); // 초기 상태 설정
    }

    /// 매 프레임 호출: 상태별 입력/로직 실행 후 자식 위치 갱신
    public virtual void Update()
    {
        if (previewConstruction == null || !previewConstruction.gameObject.activeSelf) return;

        currentState.HandleInput();
        currentState.UpdateLogic();
        UpdateChildTransform();
    }

    /// 상태 전환 헬퍼
    public void TransitionTo(PlacementState next)
    {
        currentState = states[next];
    }

    // 배치 확정: MapManager에 저장, 새 preview 생성
    public virtual void OnConfirm()
    {
        var subData = DataManager.Instance.Construction.GetDeepConstructionData(data.constructionType, data.subTypeID);
        string conTag = subData.Tag;
        Place();
        Exit();
        // 새 preview 인스턴스 즉시 재시작
        var con = PoolManager.Instance.SpawnFromPool<Construction>(conTag);
        con.Init(data.constructionType, data.subTypeID);
        Debug.Log($"{con.Tag} {con.Type} {con.SubType}");
        StartPlacing(data, con);
    }

    public virtual void OnCancel()
    {
        if (previewConstruction.Type == ConstructionType.Build) previewConstruction.OnCancel?.Invoke();
        PoolManager.Instance.ReturnToPool<Construction>(previewConstruction.Tag, previewConstruction);
        Exit();
    }

    protected virtual void Place()
    {
        if(previewCollider != null) previewCollider.enabled = true; // 배치 확정 시 Collider 활성화
        MapManager.Instance.SetBuildingArea(gridPos, buildingSize, previewConstruction);
        previewConstruction.OnPlace?.Invoke();
    }

    protected virtual void Exit()
    {
        SetPlacementButtonsActive(false);
        previewConstruction = null;
    }

    protected Vector3 GetSnappedPosition(Tilemap tilemap, Vector3Int pos)
    {
        Vector3 cellCenter = tilemap.GetCellCenterWorld(pos);
        Vector3 cellSize = tilemap.cellSize;

        float offsetX = (buildingSize.x % 2 == 0) ? 0.5f : 0f;
        float offsetY = (buildingSize.y % 2 == 0) ? 0.5f : 0f;

        Vector3 offset = Vector3.right * offsetX * cellSize.x + Vector3.up * offsetY * cellSize.y;
        return cellCenter - offset;
    }

    private void UpdateChildTransform()
    {
        Vector2 pos = previewConstruction.transform.position;
        float width = checkButtonBound.x / 2, height = buildingSize.y / 2f;
        Vector2 upOrDown = mainCamera.transform.position.y > 0 ? Vector2.down : Vector2.up;

        checkButton.transform.position = pos + height * upOrDown * 0.4f + width * Vector2.right * 0.4f;
        cancelButton.transform.position = pos + height * upOrDown * 0.4f + width * Vector2.left * 0.4f;
        notPlaceableIndicator.transform.position = pos + height * Vector2.up * 0.4f;
        if(upOrDown == Vector2.up) notPlaceableIndicator.transform.position += Vector3.up * 0.4f;
    }

    // 프리뷰 오브젝트의 색상(alpha)을 변경
    protected void ChangeColor(bool canPlace)
    {
        Color color = previewRenderer.color;
        color.a = canPlace ? 1.0f : 0.5f;
        previewRenderer.color = color;
    }

    public virtual void OnTouchDragUpdate()
    {
        var tilemap = GetTilemap();

        Vector2 dragDeltaPixels = InputManager.Instance.GetDragDirection();
        accumulatedDrag += dragDeltaPixels;

        float worldHeight = mainCamera.orthographicSize * 2f;
        float pixelToWorld = worldHeight / Screen.height;
        float thresholdWorld = swipeThresholdPixels * pixelToWorld;

        Vector3Int offset = Vector3Int.zero;
        if (Mathf.Abs(accumulatedDrag.x) >= swipeThresholdPixels)
        {
            offset.x = (int)Mathf.Sign(accumulatedDrag.x);
            accumulatedDrag.x = 0;
        }
        if (Mathf.Abs(accumulatedDrag.y) >= swipeThresholdPixels)
        {
            offset.y = (int)Mathf.Sign(accumulatedDrag.y);
            accumulatedDrag.y = 0;
        }

        if (offset == Vector3Int.zero) return;
        if (!MapManager.Instance.InBounds(gridPos + offset, buildingSize, previewConstruction))
            return;

        gridPos += offset;
        previewConstruction.transform.position = GetSnappedPosition(tilemap, gridPos);
    }

    public virtual void OnInitialDragEnd() { }

    public virtual void OnLineDragUpdate() { }
}
