using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class SpritesBaker : EditorWindow
{
    [MenuItem("Tools/Sprite Renderer Baker")]
    static void ShowWindow() => GetWindow<SpritesBaker>("Sprite Renderer Baker");

    private void OnGUI()
    {
        GUILayout.Label("Sprite Renderer Baker", EditorStyles.boldLabel);

        if (GUILayout.Button("선택된 오브젝트 렌더링"))
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("오류", "객체를 선택해 주세요.", "확인");
                return;
            }

            SpriteRenderer[] renderers = selected.GetComponentsInChildren<SpriteRenderer>(true);
            if (renderers.Length == 0)
            {
                EditorUtility.DisplayDialog("오류", "SpriteRenderer가 없습니다.", "확인");
                return;
            }

            BakeSpritesToTexture(selected, renderers);
        }
    }

    static void BakeSpritesToTexture(GameObject root, SpriteRenderer[] renderers)
    {
        int renderLayer = LayerMask.NameToLayer("RenderOnly");
        if (renderLayer == -1)
        {
            Debug.LogError("'RenderOnly' 레이어가 존재하지 않습니다. 프로젝트 설정에서 추가해 주세요.");
            return;
        }

        var originalLayers = SetLayerRecursively(root.transform, renderLayer);

        Camera cam = CreateRenderCamera(renderLayer);
        Bounds bounds = CalculateBounds(renderers);
        SetupCamera(cam, bounds);

        RenderTexture rt = CreateRenderTexture(bounds);
        cam.targetTexture = rt;
        cam.Render();

        Texture2D tex = ReadRenderTexture(rt);
        string savedPath = SaveTexture(tex, root.name);

        if (!string.IsNullOrEmpty(savedPath))
            ImportAsSprite(savedPath);

        RestoreOriginalLayers(originalLayers);
        Cleanup(cam.gameObject, rt, tex);
    }

    static Dictionary<Transform, int> SetLayerRecursively(Transform root, int newLayer)
    {
        var map = new Dictionary<Transform, int>();
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            map[t] = t.gameObject.layer;
            t.gameObject.layer = newLayer;
        }
        return map;
    }

    static void RestoreOriginalLayers(Dictionary<Transform, int> map)
    {
        foreach (var kvp in map)
            if (kvp.Key != null) kvp.Key.gameObject.layer = kvp.Value;
    }

    static Camera CreateRenderCamera(int cullingLayer)
    {
        var camObj = new GameObject("TempCamera");
        var cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear;
        cam.cullingMask = 1 << cullingLayer;
        return cam;
    }

    static void SetupCamera(Camera cam, Bounds bounds)
    {
        cam.orthographicSize = bounds.size.y / 2f;
        cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);
    }

    static RenderTexture CreateRenderTexture(Bounds bounds)
    {
        int width = Mathf.CeilToInt(bounds.size.x * 100);
        int height = Mathf.CeilToInt(bounds.size.y * 100);
        return new RenderTexture(width, height, 24);
    }

    static Texture2D ReadRenderTexture(RenderTexture rt)
    {
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        RenderTexture.active = prev;
        return tex;
    }

    static string SaveTexture(Texture2D tex, string name)
    {
        string path = EditorUtility.SaveFilePanel("스프라이트 저장", "Assets", name, "png");
        if (string.IsNullOrEmpty(path)) return string.Empty;

        File.WriteAllBytes(path, tex.EncodeToPNG());
        Debug.Log("스프라이트 저장 완료: " + path);
        return path;
    }

    static void ImportAsSprite(string path)
    {
        if (!path.StartsWith(Application.dataPath))
        {
            Debug.LogWarning("Assets 폴더 내부에 저장해야 자동 임포트됩니다.");
            return;
        }

        string assetPath = "Assets" + path.Substring(Application.dataPath.Length);
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(assetPath);

        EditorApplication.delayCall += () =>
        {
            if (AssetImporter.GetAtPath(assetPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
                Debug.Log("임포트 완료: " + assetPath);
            }
        };
    }

    static Bounds CalculateBounds(SpriteRenderer[] renderers)
    {
        Bounds bounds = new Bounds(renderers[0].bounds.center, Vector3.zero);
        foreach (var renderer in renderers)
            bounds.Encapsulate(renderer.bounds);
        bounds.Expand(0.1f); // 여유
        return bounds;
    }

    static void Cleanup(GameObject camObj, RenderTexture rt, Texture2D tex)
    {
        RenderTexture.active = null;
        Object.DestroyImmediate(camObj);
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
    }
}
