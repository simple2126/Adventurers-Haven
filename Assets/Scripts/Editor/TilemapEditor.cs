using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapBaker : EditorWindow
{
    [MenuItem("Tools/Tilemap Baker")]
    static void ShowWindow()
    {
        GetWindow<TilemapBaker>("Tilemap Baker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tilemap Baker", EditorStyles.boldLabel);

        if (GUILayout.Button("선택된 Grid 베이킹"))
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("오류", "객체를 선택해 주세요.", "확인");
                return;
            }

            Grid grid = FindGridComponent(selected);
            if (grid == null)
            {
                EditorUtility.DisplayDialog("오류", "Grid 컴포넌트를 찾을 수 없습니다.", "확인");
                return;
            }

            BakeGrid(grid);
        }
    }

    static void BakeGrid(Grid grid)
    {
        LogTilemapInfo(grid);

        Camera cam = CreateCamera();
        Bounds bounds = CalculateBounds(grid);
        ConfigureCamera(cam, bounds);

        RenderTexture rt = CreateRenderTexture(bounds);
        cam.targetTexture = rt;

        cam.Render();
        Texture2D tex = CaptureTexture(rt);

        string savedPath = SaveTextureAsPNG(tex, grid);
        if (!string.IsNullOrEmpty(savedPath))
        {
            ImportTextureToAssetDatabase(savedPath);
        }

        Cleanup(cam.gameObject, rt, tex);
    }

    static Grid FindGridComponent(GameObject selected)
    {
        if (selected == null) return null;

        return selected.GetComponent<Grid>() ??
               selected.GetComponentInParent<Grid>() ??
               selected.GetComponentInChildren<Grid>();
    }

    static void LogTilemapInfo(Grid grid)
    {
        Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>(true);
        Debug.Log("총 " + tilemaps.Length + "개의 타일맵 발견");

        foreach (var tilemap in tilemaps)
        {
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
        cam.backgroundColor = Color.clear;
        return cam;
    }

    static void ConfigureCamera(Camera cam, Bounds bounds)
    {
        cam.orthographicSize = bounds.size.y / 2f;
        Vector3 cameraPos = bounds.center;
        cameraPos.z = -10f;
        cam.transform.position = cameraPos;
    }

    static RenderTexture CreateRenderTexture(Bounds bounds)
    {
        int textureWidth = Mathf.Max(1, Mathf.CeilToInt(bounds.size.x * 100));
        int textureHeight = Mathf.Max(1, Mathf.CeilToInt(bounds.size.y * 100));
        Debug.Log("텍스처 크기: " + textureWidth + "x" + textureHeight);

        return new RenderTexture(textureWidth, textureHeight, 24);
    }

    static Texture2D CaptureTexture(RenderTexture rt)
    {
        RenderTexture prevActive = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        RenderTexture.active = prevActive;
        return tex;
    }

    static string SaveTextureAsPNG(Texture2D tex, Grid grid)
    {
        string prefabName = grid.gameObject.name;
        string path = EditorUtility.SaveFilePanel("Save Sprite", "Assets", prefabName, "png");
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("저장이 취소되었습니다.");
            return string.Empty;
        }

        byte[] pngData = tex.EncodeToPNG();
        if (pngData != null)
        {
            File.WriteAllBytes(path, pngData);
            Debug.Log("스프라이트가 저장되었습니다: " + path);
            return path;
        }
        else
        {
            Debug.LogError("텍스처를 PNG로 인코딩하는데 실패했습니다.");
            return string.Empty;
        }
    }

    static void ImportTextureToAssetDatabase(string savedPath)
    {
        if (string.IsNullOrEmpty(savedPath)) return;

        string dataPath = Application.dataPath;
        if (!savedPath.StartsWith(dataPath))
        {
            Debug.Log("Assets 폴더 외부에 저장되었습니다. 에셋으로 임포트하려면 Assets 폴더 내에 저장하세요.");
            return;
        }

        string assetPath = "Assets" + savedPath.Substring(dataPath.Length);
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(assetPath);

        EditorApplication.delayCall += () =>
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
                Debug.Log("스프라이트가 에셋으로 임포트되었습니다: " + assetPath);
            }
            else
            {
                Debug.LogWarning("임포트 중 오류가 발생했습니다.");
            }
        };
    }

    static Bounds CalculateBounds(Grid grid)
    {
        Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>(true);

        if (tilemaps.Length == 0)
        {
            Debug.LogWarning("타일맵이 없습니다!");
            return new Bounds(grid.transform.position, Vector3.one * 2f);
        }

        Bounds bounds = new Bounds();
        bool isFirst = true;

        foreach (var tilemap in tilemaps)
        {
            if (!tilemap.gameObject.activeInHierarchy) continue;

            List<Vector3> worldPositions = new List<Vector3>();

            foreach (Vector3Int cellPosition in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(cellPosition))
                {
                    Vector3 worldPos = tilemap.CellToWorld(cellPosition);
                    worldPositions.Add(worldPos);
                    worldPositions.Add(worldPos + new Vector3(grid.cellSize.x, 0, 0));
                    worldPositions.Add(worldPos + new Vector3(0, grid.cellSize.y, 0));
                    worldPositions.Add(worldPos + new Vector3(grid.cellSize.x, grid.cellSize.y, 0));
                }
            }

            if (worldPositions.Count == 0) continue;

            Bounds tilemapBounds = new Bounds(worldPositions[0], Vector3.zero);
            foreach (Vector3 pos in worldPositions)
            {
                tilemapBounds.Encapsulate(pos);
            }

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
        return bounds;
    }

    static void Cleanup(GameObject camObj, RenderTexture rt, Texture2D tex)
    {
        RenderTexture.active = null;
        if (camObj != null) Object.DestroyImmediate(camObj);
        if (rt != null) Object.DestroyImmediate(rt);
        if (tex != null) Object.DestroyImmediate(tex);
    }
}