using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(TilemapPainter))]
public class TilemapPainterEditor : Editor
{
    private Texture2D previewImage;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TilemapPainter painter = (TilemapPainter)target;

        GUILayout.Space(10);
        GUILayout.Label("패턴 미리보기", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 선택된 패턴 가져오기
        TileBase[] pattern = painter.PatternType == PatternType.White
            ? painter.PatternWhiteTiles
            : painter.PatternGrayTiles;

        previewImage = GeneratePreviewImage(pattern);
        if (previewImage != null)
        {
            float size = EditorGUIUtility.currentViewWidth - 40;
            Rect previewRect = GUILayoutUtility.GetRect(size, size);
            GUI.DrawTexture(previewRect, previewImage, ScaleMode.ScaleToFit);
        }
    }

    Texture2D GeneratePreviewImage(TileBase[] pattern)
    {
        int tileSize = 0;
        if (pattern[0] is Tile tile0 && tile0.sprite != null)
        {
            tileSize = (int)tile0.sprite.rect.width;
        }

        Texture2D result = new Texture2D(tileSize * 2, tileSize * 2);
        result.filterMode = FilterMode.Point;

        for (int i = 0; i < 4; i++)
        {
            if (pattern[i] is Tile tile && tile.sprite != null)
            {
                Texture2D tex = SpriteToTexture(tile.sprite);
                int px = (i % 2) * tileSize;
                int py = (i / 2 == 0) ? tileSize : 0;

                result.SetPixels(px, py, tex.width, tex.height, tex.GetPixels());
            }
        }

        result.Apply();
        return result;
    }

    Texture2D SpriteToTexture(Sprite sprite)
    {
        Texture2D tex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        Color[] pixels = sprite.texture.GetPixels(
            (int)sprite.textureRect.x,
            (int)sprite.textureRect.y,
            (int)sprite.textureRect.width,
            (int)sprite.textureRect.height);
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
