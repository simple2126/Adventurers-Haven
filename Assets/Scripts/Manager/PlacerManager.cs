using AdventurersHaven;
using UnityEngine;
using UnityEngine.UI;

public class PlacerManager : SingletonBase<PlacerManager>  // PlacerManager로 수정
{
    private Camera mainCamera;
    private BasePlacer currentPlacer;

    [SerializeField] private Button check;
    [SerializeField] private Button cancel;
    [SerializeField] private GameObject notPlaceable;

    // 별도의 Placer 인스턴스들
    private DefaultPlacer defaultPlacer;
    private RoadPlacer roadPlacer;
    private RemovePlacer removePlacer;

    protected override void Awake()
    {
        base.Awake();
        check.onClick.AddListener(() => OnPlacementButtonClicked(true));
        cancel.onClick.AddListener(() => OnPlacementButtonClicked(false));
    }

    private void Start()
    {
        mainCamera = Camera.main;

        defaultPlacer = new DefaultPlacer(mainCamera, check, cancel, notPlaceable);
        roadPlacer = new RoadPlacer(mainCamera, check, cancel, notPlaceable);
        removePlacer = new RemovePlacer(mainCamera, check, cancel, notPlaceable);
    }

    private void Update()
    {
        if (currentPlacer != null)
        {
            currentPlacer.Update();
        }
    }

    private void OnPlacementButtonClicked(bool isCheck)
    {
        // 먼저 버튼 클릭 처리를 완료
        if (currentPlacer != null)
        {
            if (isCheck)
            {
                currentPlacer.OnConfirm(); // 배치 확인
            }
            else
            {
                currentPlacer.OnCancel(); // 배치 취소
                ExitPlacing(); // 배치 취소 후, 처리
            }
        }
    }


    // 데이터만으로 타입 판단하는 오버로드 메서드 추가
    public void StartPlacing(Construction_Data data, Vector2Int size)
    {
        var prefab = PoolManager.Instance.SpawnFromPool<Construction>(data.tag);
        prefab.Init(data);
        // 적절한 Placer 선택

        // Element && Demolish
        if (prefab.IsDemolish())
        {
            currentPlacer = removePlacer;
        }
        // Element && Road
        else if (prefab.IsRoad())
        {
            currentPlacer = roadPlacer;
        }
        else
        {
            currentPlacer = defaultPlacer;
        }
        
        currentPlacer.StartPlacing(data, prefab);
        gameObject.SetActive(true);
    }

    private void ExitPlacing()
    {
        gameObject.SetActive(false);
        currentPlacer = null;
        UIManager.Instance.Show<Main>();
    }
}