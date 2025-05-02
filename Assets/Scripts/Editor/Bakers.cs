using UnityEditor;
using UnityEngine;

public class Bakers : EditorWindow
{
    private TilemapBaker tilemapBaker = new TilemapBaker();
    private SpritesBaker spritesBaker = new SpritesBaker();

    [MenuItem("Tools/Bakers")]

    private static void ShowWindow()
    {
        GetWindow<Bakers>("Bakers");
    }

    private void OnGUI()
    {
        GUILayout.Space(10); // 아래에 여백 추가
        tilemapBaker.OnGUI();
        GUILayout.Space(20);
        spritesBaker.OnGUI();
    }
}
