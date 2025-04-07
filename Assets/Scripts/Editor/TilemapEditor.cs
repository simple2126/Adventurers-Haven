using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapBaker : MonoBehaviour
{
    [MenuItem("Tools/Bake Grid to Sprite", false, 1000)]
    static void BakeGridToSprite()
    {
        GameObject selected = Selection.activeObject as GameObject;
        Grid grid = FindGridComponent(selected);

        if (grid == null)
        {
            Debug.LogWarning("Grid 컴포넌트를 찾을 수 없습니다. Grid가 있는 객체를 선택하세요.");
            return;
        }

        Debug.Log("Grid 발견: " + grid.name + ", Cell Size: " + grid.cellSize);
        LogTilemapInfo(grid);

        Camera cam = CreateCamera();
        Bounds bounds = CalculateBounds(grid);
        ConfigureCamera(cam, bounds);

        RenderTexture rt = CreateRenderTexture(bounds);
        cam.targetTexture = rt;

        cam.Render();
        Texture2D tex = CaptureTexture(rt);

        if (SaveTextureAsPNG(tex, grid))
        {
            ImportTextureToAssetDatabase(tex, grid);
        }

        Cleanup(cam.gameObject, rt, tex);
    }

    static Grid FindGridComponent(GameObject selected)
    {
        if (selected == null) return null;

        Grid grid = selected.GetComponent<Grid>() ??
                    selected.GetComponentInParent<Grid>() ??
                    selected.GetComponentInChildren<Grid>();

        return grid;
    }

    static void LogTilemapInfo(Grid grid)
    {
        Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>(true);
        Debug.Log("총 " + tilemaps.Length + "개의 타일맵 발견");

        foreach (var tilemap in tilemaps)
        {
            TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
            string sortingInfo = renderer ?
                "Sorting Layer: " + renderer.sortingLayerName + ", Order: " + renderer.sortingOrder :
                "렌더러 없음";

            Debug.Log("타일맵: " + tilemap.name +
                     ", Position: " + tilemap.transform.position +
                     ", 활성화: " + tilemap.gameObject.activeInHierarchy +
                     ", " + sortingInfo);

            int tileCount = 0;
            foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(pos)) tileCount++;
            }
            Debug.Log("타일맵 '" + tilemap.name + "'의 타일 수: " + tileCount);
        }
    }

    static Camera CreateCamera()
    {
        GameObject camObj = new GameObject("TempCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0);
        return cam;
    }

    static void ConfigureCamera(Camera cam, Bounds bounds)
    {
        cam.orthographicSize = bounds.size.y / 2f;
        Vector3 cameraPos = bounds.center;
        cameraPos.z = -10f;
        cam.transform.position = cameraPos;
        cam.cullingMask = -1;
        Debug.Log("카메라 설정 - 위치: " + cam.transform.position + ", Size: " + cam.orthographicSize);
    }

    static RenderTexture CreateRenderTexture(Bounds bounds)
    {
        int textureWidth = Mathf.CeilToInt(bounds.size.x * 100);
        int textureHeight = Mathf.CeilToInt(bounds.size.y * 100);
        Debug.Log("최종 텍스처 크기: " + textureWidth + "x" + textureHeight +
                 ", Bounds 크기: " + bounds.size + ", 중심: " + bounds.center);

        return new RenderTexture(textureWidth, textureHeight, 24);
    }

    static Texture2D CaptureTexture(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        Color[] pixels = tex.GetPixels();
        bool hasContent = false;
        foreach (Color pixel in pixels)
        {
            if (pixel.a > 0.01f)
            {
                hasContent = true;
                break;
            }
        }
        Debug.Log("텍스처에 내용 있음: " + hasContent);
        return tex;
    }

    static bool SaveTextureAsPNG(Texture2D tex, Grid grid)
    {
        string prefabName = grid.gameObject.name;
        string path = EditorUtility.SaveFilePanel("Save Sprite", "Assets", prefabName, "png");
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("저장이 취소되었습니다.");
            return false;
        }

        File.WriteAllBytes(path, tex.EncodeToPNG());
        Debug.Log("스프라이트가 저장되었습니다: " + path);
        return true;
    }

    static void ImportTextureToAssetDatabase(Texture2D tex, Grid grid)
    {
        string path = AssetDatabase.GetAssetPath(tex);
        if (path.StartsWith(Application.dataPath))
        {
            string assetPath = "Assets" + path.Substring(Application.dataPath.Length);
            AssetDatabase.ImportAsset(assetPath);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
                Debug.Log("스프라이트가 에셋으로 임포트되었습니다: " + assetPath);
            }
            else
            {
                Debug.LogWarning("임포트 중 오류가 발생했습니다.");
            }
        }
        else
        {
            Debug.Log("Assets 폴더 외부에 저장되었습니다. 에셋으로 임포트하려면 Assets 폴더 내에 저장하세요.");
        }
    }

    static Bounds CalculateBounds(Grid grid)
    {
        Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>(true);
        Debug.Log("발견된 타일맵 수: " + tilemaps.Length);

        if (tilemaps.Length == 0)
        {
            Debug.LogWarning("타일맵이 없습니다!");
            return new Bounds(grid.transform.position, Vector3.one * 2f);
        }

        Bounds bounds = new Bounds();
        bool isFirst = true;

        foreach (var tilemap in tilemaps)
        {
            Debug.Log("타일맵 '" + tilemap.name + "' 처리 중, 활성화 상태: " + tilemap.gameObject.activeInHierarchy);

            BoundsInt cellBounds = tilemap.cellBounds;
            List<Vector3Int> usedCells = new List<Vector3Int>();

            foreach (Vector3Int cellPosition in cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(cellPosition))
                {
                    usedCells.Add(cellPosition);
                }
            }

            if (usedCells.Count == 0)
            {
                Debug.Log("타일맵 '" + tilemap.name + "'에 타일이 없습니다. 사용 셀 수: " + usedCells.Count);
                continue;
            }

            Debug.Log("타일맵 '" + tilemap.name + "'에서 " + usedCells.Count + "개의 셀 발견!");

            List<Vector3> worldPositions = new List<Vector3>();
            foreach (Vector3Int cell in usedCells)
            {
                worldPositions.Add(tilemap.CellToWorld(cell));
                worldPositions.Add(tilemap.CellToWorld(cell + new Vector3Int(1, 0, 0)));
                worldPositions.Add(tilemap.CellToWorld(cell + new Vector3Int(0, 1, 0)));
                worldPositions.Add(tilemap.CellToWorld(cell + new Vector3Int(1, 1, 0)));
            }

            Bounds tilemapBounds = new Bounds(worldPositions[0], Vector3.zero);
            foreach (Vector3 pos in worldPositions)
            {
                tilemapBounds.Encapsulate(pos);
            }

            Debug.Log("타일맵 '" + tilemap.name + "' 경계: " + tilemapBounds.center + ", 크기: " + tilemapBounds.size);

            if (isFirst)
            {
                bounds = tilemapBounds;
                isFirst = false;
            }
            else
            {
                bounds.Encapsulate(tilemapBounds);
            }
        }

        if (isFirst)
        {
            Debug.LogWarning("유효한 타일맵이 없습니다. 기본 경계를 사용합니다.");
            return new Bounds(grid.transform.position, Vector3.one * 2f);
        }

        bounds.Expand(grid.cellSize * 2f);
        Debug.Log("최종 경계: " + bounds.center + ", 크기: " + bounds.size);
        return bounds;
    }

    static void Cleanup(GameObject camObj, RenderTexture rt, Texture2D tex)
    {
        RenderTexture.active = null;
        if (camObj != null) DestroyImmediate(camObj);
        if (rt != null) DestroyImmediate(rt);
        if (tex != null) DestroyImmediate(tex);
    }
}

public static class TilemapExtensions
{
    public static Bounds CalculateBounds(this Tilemap tilemap)
    {
        bool hasTiles = false;
        BoundsInt cellBounds = tilemap.cellBounds;

        Vector3Int minCell = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
        Vector3Int maxCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

        foreach (Vector3Int pos in cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                hasTiles = true;
                minCell.x = Mathf.Min(minCell.x, pos.x);
                minCell.y = Mathf.Min(minCell.y, pos.y);
                minCell.z = Mathf.Min(minCell.z, pos.z);

                maxCell.x = Mathf.Max(maxCell.x, pos.x);
                maxCell.y = Mathf.Max(maxCell.y, pos.y);
                maxCell.z = Mathf.Max(maxCell.z, pos.z);
            }
        }

        if (!hasTiles)
        {
            Debug.LogWarning("타일맵 '" + tilemap.name + "'에 타일이 없습니다.");
            return new Bounds(tilemap.transform.position, Vector3.one);
        }

        Grid grid = tilemap.layoutGrid;
        float cellSize = grid != null ? grid.cellSize.x : 1.0f;
        Debug.Log("타일맵 '" + tilemap.name + "'의 Cell Size: " + cellSize);

        Vector3 worldMin = tilemap.CellToWorld(minCell);
        Vector3 worldMax = tilemap.CellToWorld(maxCell + Vector3Int.one);

        Vector3 padding = new Vector3(cellSize, cellSize, 0) * 0.5f;
        worldMin -= padding;
        worldMax += padding;

        Vector3 size = worldMax - worldMin;
        Vector3 center = (worldMin + worldMax) * 0.5f;

        float minSize = cellSize * 4;
        size.x = Mathf.Max(minSize, size.x);
        size.y = Mathf.Max(minSize, size.y);

        Debug.Log("타일맵 '" + tilemap.name + "' 계산된 크기: " + size + " (셀 범위: " + (maxCell - minCell + Vector3Int.one) + ")");
        return new Bounds(center, size);
    }
}