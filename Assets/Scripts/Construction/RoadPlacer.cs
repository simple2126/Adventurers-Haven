using AdventurersHaven;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoadPlacer : BasePlacer
{
    private RoadPlacementState roadState = RoadPlacementState.None;
    private Vector3Int roadStartPos;
    private Vector3Int roadEndPos;
    private List<Construction> previewRoadList = new List<Construction>();

    public RoadPlacer(Camera camera, Button check, Button cancel, GameObject notPlaceable)
    : base(camera, check, cancel, notPlaceable)
    {
    }

    public override void StartPlacing(Construction_Data data, Construction construction, Vector2Int size)
    {
        base.StartPlacing(data, construction, size);
        roadState = RoadPlacementState.None;
        previewRoadList.Clear();
    }

    protected override void UpdatePlacement()
    {
        bool canPlace = CanPlaceAtCurrentPosition();
        ChangeColor(canPlace);
        notPlaceableIndicator.SetActive(!canPlace);

        switch (roadState)
        {
            case RoadPlacementState.None:
                if (Input.GetMouseButtonDown(0) && canPlace)
                {
                    roadStartPos = gridPos;
                    roadState = RoadPlacementState.Dragging;
                }
                break;

            case RoadPlacementState.Dragging:
                roadEndPos = gridPos;
                PreviewRoadLine(roadStartPos, roadEndPos);

                if (Input.GetMouseButtonDown(0) && canPlace)
                {
                    roadState = RoadPlacementState.Confirm;
                    state = PlacementState.Confirming;
                    SetPlacementButtonsActive(true);
                }
                break;
        }
    }

    protected override void ChangePreviewObjPos()
    {
        if (roadState == RoadPlacementState.Confirm) return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector3Int rawGridPos = MapManager.Instance.ElementTilemap.WorldToCell(mouseWorld);

        if (roadState == RoadPlacementState.Dragging)
        {
            // 도로 드래깅 중일 때 buildingSize 단위 스냅 적용
            gridPos =
                Vector3Int.right * Mathf.FloorToInt((float)rawGridPos.x / buildingSize.x) * buildingSize.x +
                Vector3Int.up * Mathf.FloorToInt((float)rawGridPos.y / buildingSize.y) * buildingSize.y;
        }
        else
        {
            gridPos = rawGridPos;
        }

        previewConstruction.gameObject.transform.position = GetSnappedPosition(MapManager.Instance.ElementTilemap, gridPos);
    }

    public override void OnConfirm()
    {
        PlaceRoadLine(roadStartPos, roadEndPos);
        Exit();
    }

    public override void OnCancel()
    {
        ReturnRoadList();
        PoolManager.Instance.ReturnToPool<Construction>(data.tag, previewConstruction);
        Exit();
    }

    protected override void Exit()
    {
        base.Exit();
        roadState = RoadPlacementState.None;
        previewRoadList.Clear();
    }

    private void PlaceRoadLine(Vector3Int start, Vector3Int end)
    {
        // 단일 도로 배치
        if (previewRoadList.Count == 0)
        {
            Vector3Int vecInt = Vector3Int.right * Mathf.CeilToInt(start.x) + Vector3Int.up * Mathf.CeilToInt(start.y);
            MapManager.Instance.SetBuildingArea(vecInt, buildingSize, previewConstruction);
            return;
        }

        for (int i = 0; i < previewRoadList.Count; i++)
        {
            var current = previewRoadList[i].transform.position;
            current = MapManager.Instance.ElementTilemap.WorldToCell(current);
            Vector3Int vecInt = Vector3Int.right * Mathf.CeilToInt(current.x) + Vector3Int.up * Mathf.CeilToInt(current.y);

            if (MapManager.Instance.IsSameRoadData(vecInt, buildingSize, previewRoadList[i].Tag))
            {
                PoolManager.Instance.ReturnToPool<Construction>(data.tag, previewRoadList[i]);
                previewRoadList.RemoveAt(i);
                i--;
            }
            else
            {
                MapManager.Instance.SetBuildingArea(vecInt, buildingSize, previewRoadList[i]);
            }
        }

        if (previewRoadList.Count > 0)
            PoolManager.Instance.ReturnToPool<Construction>(data.tag, previewConstruction);
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
            var notPlace = !MapManager.Instance.CanPlaceBuilding(current, buildingSize, previewConstruction);
            if (notPlace) break;

            if (index >= previewRoadList.Count)
            {
                var obj = PoolManager.Instance.SpawnFromPool<Construction>(data.tag);
                obj.SetData(data);
                previewRoadList.Add(obj);
            }

            var preview = previewRoadList[index];
            preview.transform.position = GetSnappedPosition(MapManager.Instance.ElementTilemap, current);
            index++;
        }

        if (previewRoadList.Count > 0)
        {
            previewConstruction.transform.position = previewRoadList[index - 1].transform.position;
        }

        // 남은 프리뷰 비활성화
        for (int i = index; i < previewRoadList.Count; i++)
        {
            var preview = previewRoadList[i];
            PoolManager.Instance.ReturnToPool<Construction>(data.tag, preview);
            previewRoadList.RemoveAt(i);
            i--; // 리스트에서 요소를 제거했으므로 인덱스 조정
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

    private void ReturnRoadList()
    {
        if (previewRoadList.Count == 0) return;

        foreach (var obj in previewRoadList)
            PoolManager.Instance.ReturnToPool<Construction>(data.tag, obj);

        previewRoadList.Clear();
    }
}