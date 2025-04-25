using AdventurersHaven;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PlacerManager : SingletonBase<PlacerManager>  // PlacerManager로 수정
{
    private Camera mainCamera;
    private BasePlacer currentPlacer;

    [SerializeField] private Button check;
    [SerializeField] private Button cancle;
    [SerializeField] private GameObject notPlaceable;

    // 별도의 Placer 인스턴스들
    private DefaultPlacer defaultPlacer;
    private RoadPlacer roadPlacer;

    protected override void Awake()
    {
        base.Awake();
        check.onClick.AddListener(() => OnPlacementButtonClicked(true));
        cancle.onClick.AddListener(() => OnPlacementButtonClicked(false));
    }

    private void Start()
    {
        mainCamera = Camera.main;

        defaultPlacer = new DefaultPlacer(mainCamera, check, cancle, notPlaceable);
        roadPlacer = new RoadPlacer(mainCamera, check, cancle, notPlaceable);
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
        if (currentPlacer != null)
        {
            if (isCheck)
                currentPlacer.OnConfirm();
            else
                currentPlacer.OnCancel();
        }

        ExitPlacing();
        UIManager.Instance.Show<Main>();
    }

    // 데이터만으로 타입 판단하는 오버로드 메서드 추가
    public void StartPlacing(Construction_Data data, Vector2Int size)
    {
        bool isRoad = false;
        if (data.constructionType == ConstructionType.Element &&
            Enum.TryParse(data.subType, out ElementType elementType))
        {
            isRoad = elementType == ElementType.Road;
        }

        // 적절한 Placer 선택
        currentPlacer = isRoad ? roadPlacer : defaultPlacer;
        currentPlacer.StartPlacing(data, size);

        gameObject.SetActive(true);
    }

    private void ExitPlacing()
    {
        gameObject.SetActive(false);
        currentPlacer = null;
    }
}