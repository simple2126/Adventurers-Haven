using AdventurersHaven;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoadPlacer : BasePlacer
{
    private Vector3Int roadStart;
    private Vector3Int roadEnd;
    private List<Construction> previewRoadList = new List<Construction>();

    public override bool RequiresPreview => true;

    public RoadPlacer(Camera camera, Button check, Button cancel, GameObject notPlaceable)
        : base(camera, check, cancel, notPlaceable)
    {
    }

    public override void StartPlacing(Construction_Data data, Construction construction)
    {
        base.StartPlacing(data, construction);
        previewRoadList.Clear();
        roadStart = Vector3Int.zero;
        roadEnd = Vector3Int.zero;
        SetPlacementButtonsActive(true);
    }

    // 첫 드래그 끝났을 때
    public override void OnInitialDragEnd()
    {
        roadStart = gridPos;
    }

    // PreviewState.HandleInput()에서 드래그 중 매 프레임 호출
    public override void OnLineDragUpdate()
    {
        roadEnd = gridPos;                     // ← 반드시 기록
        PreviewRoadLine(roadStart, roadEnd);

        // 마지막 프리뷰 위치로 previewConstruction 이동
        if (previewRoadList.Count > 0)
            previewConstruction.transform.position = previewRoadList[^1].transform.position;
    }

    public override void UpdatePlacement()
    {
        // 단순 색상/indicator 갱신
        bool can = MapManager.Instance.CanPlaceBuilding(gridPos, buildingSize, previewConstruction);
        ChangeColor(can);
        notPlaceableIndicator.SetActive(!can);
    }

    public override void OnConfirm()
    {
        PlaceRoadLine(roadStart, roadEnd);
        ReturnRoadList();
        PoolManager.Instance.ReturnToPool<Construction>(previewConstruction.Tag, previewConstruction);
        notPlaceableIndicator.SetActive(false);
        Exit();
    }

    public override void OnCancel()
    {
        PoolManager.Instance.ReturnToPool<Construction>(previewConstruction.Tag, previewConstruction);
        ReturnRoadList();
        notPlaceableIndicator.SetActive(false);
        Exit();
    }

    private void PlaceRoadLine(Vector3Int start, Vector3Int end)
    {
        // 단일 도로 배치
        if (previewRoadList.Count == 0)
        {
            if(!MapManager.Instance.CanPlaceBuilding(roadStart, buildingSize, previewConstruction))
            {
                PoolManager.Instance.ReturnToPool<Construction>(previewConstruction.Tag, previewConstruction);
                return;
            }
            MapManager.Instance.SetBuildingArea(roadStart, buildingSize, previewConstruction);
            return;
        }

        for (int i = 0; i < previewRoadList.Count; i++)
        {
            var current = previewRoadList[i].transform.position;
            current = MapManager.Instance.ElementTilemap.WorldToCell(current);
            Vector3Int vecInt = Vector3Int.right * Mathf.CeilToInt(current.x) + Vector3Int.up * Mathf.CeilToInt(current.y);

            Debug.Log($"[DEBUG] vecInt={vecInt}, IsSameRoadData={MapManager.Instance.IsSameRoadData(vecInt, buildingSize, previewRoadList[i].Tag)}, CanPlace={MapManager.Instance.CanPlaceBuilding(vecInt, buildingSize, previewConstruction)}");

            if (!MapManager.Instance.IsSameRoadData(vecInt, buildingSize, previewRoadList[i].Tag)
                && MapManager.Instance.CanPlaceBuilding(vecInt, buildingSize, previewConstruction))
            {
                MapManager.Instance.SetBuildingArea(vecInt, buildingSize, previewRoadList[i]);
            }
            else
            {
                PoolManager.Instance.ReturnToPool<Construction>(previewConstruction.Tag, previewConstruction);
            }
            previewRoadList.RemoveAt(i);
            i--;
        }
    }

    private bool PreviewRoadLine(Vector3Int start, Vector3Int end)
    {
        Vector3Int step = GetStep(end - start);
        int stepCount = GetStepCount(step, start, end);
        if (stepCount == 0)
        {
            ReturnRoadList();
            previewRoadList.Clear();
            return false;
        }

        Vector3Int current = start;
        int index = 0;

        for (int i = 0; i <= stepCount; i++)
        {
            current = start + step * i;
            if (!MapManager.Instance.CanPlaceBuilding(current, buildingSize, previewConstruction))
            {
                break;
            }

            if (index >= previewRoadList.Count)
            {
                var con = PoolManager.Instance.SpawnFromPool<Construction>(previewConstruction.Tag);
                con.Init(data.constructionType, data.subTypeID);
                previewRoadList.Add(con);
            }

            var preview = previewRoadList[index];
            preview.transform.position = GetSnappedPosition(MapManager.Instance.ElementTilemap, current);
            index++;
        }

        // 남은 프리뷰 제거
        for (int i = index; i < previewRoadList.Count; i++)
        {
            PoolManager.Instance.ReturnToPool<Construction>(previewConstruction.Tag, previewRoadList[i]);
            previewRoadList.RemoveAt(i);
            i--;
        }

        return index > 0;
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

    private void ReturnRoadList()
    {
        if (previewRoadList.Count == 0) return;

        foreach (var obj in previewRoadList)
            PoolManager.Instance.ReturnToPool<Construction>(previewConstruction.Tag, obj);

        previewRoadList.Clear();
    }
}