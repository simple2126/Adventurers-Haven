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

    // Update에서 호출
    protected override void UpdatePlacement()
    {
        bool canPlace = CanPlaceAtCurrentPosition();
        ChangeColor(canPlace);
        notPlaceableIndicator.SetActive(!canPlace);

        switch (roadState)
        {
            case RoadPlacementState.None:
                if (InputManager.Instance.IsInputDown() && canPlace)
                {
                    roadStartPos = gridPos;
                    roadState = RoadPlacementState.Dragging;
                }
                break;

            case RoadPlacementState.Dragging:
                roadEndPos = gridPos;
                bool validDrag = PreviewRoadLine(roadStartPos, roadEndPos);

                if (validDrag && previewRoadList.Count > 0)
                {
                    int lastIndex = previewRoadList.Count - 1;
                    previewConstruction.transform.position = previewRoadList[lastIndex].transform.position;
                }

                if (InputManager.Instance.IsInputUp() && validDrag)
                {
                    roadState = RoadPlacementState.Confirm;
                    state = PlacementState.Confirming;
                    SetPlacementButtonsActive(true);
                }
            break;
        }
    }

    // Update에서 호출
    protected override void ChangePreviewObjPos()
    {
        if (roadState == RoadPlacementState.Confirm)
            return; // Confirm 상태에선 움직이지 않음

        Vector3 mouseWorld = InputManager.Instance.GetWorldInputPosition(mainCamera);
        Vector3Int rawGridPos = MapManager.Instance.ElementTilemap.WorldToCell(mouseWorld);

        if (roadState == RoadPlacementState.Dragging)
        {
            gridPos = new Vector3Int(
                Mathf.FloorToInt((float)rawGridPos.x / buildingSize.x) * buildingSize.x,
                Mathf.FloorToInt((float)rawGridPos.y / buildingSize.y) * buildingSize.y,
                0);
        }
        else
        {
            gridPos = rawGridPos;
        }

        // 드래깅 중엔 별도 프리뷰 리스트가 보여지고,
        // NONE 상태일 때만 previewConstruction이 유의미함
        if (roadState == RoadPlacementState.None)
        {
            previewConstruction.transform.position = GetSnappedPosition(MapManager.Instance.ElementTilemap, gridPos);
        }
    }


    public override void OnConfirm()
    {
        PlaceRoadLine(roadStartPos, roadEndPos);
        ReturnRoadList();
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
                var obj = PoolManager.Instance.SpawnFromPool<Construction>(data.tag);
                obj.Init(data);
                previewRoadList.Add(obj);
            }

            var preview = previewRoadList[index];
            preview.transform.position = GetSnappedPosition(MapManager.Instance.ElementTilemap, current);
            index++;
        }

        // 남은 프리뷰 제거
        for (int i = index; i < previewRoadList.Count; i++)
        {
            PoolManager.Instance.ReturnToPool<Construction>(data.tag, previewRoadList[i]);
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
            PoolManager.Instance.ReturnToPool<Construction>(data.tag, obj);

        previewRoadList.Clear();
    }
}