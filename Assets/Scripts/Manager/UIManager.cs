using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonBase<UIManager>
{
    [SerializeField] private Transform canvas;

    public static float ScreenWidth = 1080;
    public static float ScreenHeight = 1920;
    private Dictionary<string, UIBase> uiDict = new Dictionary<string, UIBase>();

    public T Show<T>(params object[] param) where T : UIBase
    {
        string uiName = typeof(T).ToString();
        if (uiDict.ContainsKey(uiName)) return default;
        UIBase go = Resources.Load<UIBase>("UI/" + uiName);
        var ui = Load<T>(go, uiName);
        //uiList.Add(ui);
        uiDict[uiName] = ui;
        ui.Opened(param);
        return (T)ui;
    }

    private T Load<T>(UIBase prefab, string uiName) where T : UIBase
    {
        GameObject newCanvasObject = new GameObject(uiName + " Canvas");

        var canvas = newCanvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var canvasScaler = newCanvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = Vector2.right * ScreenWidth + Vector2.up * ScreenHeight;

        newCanvasObject.AddComponent<GraphicRaycaster>();

        UIBase ui = Instantiate(prefab, newCanvasObject.transform);
        ui.name = ui.name.Replace("(Clone)", "");
        ui.canvas = canvas;
        ui.canvas.sortingOrder = uiDict.Count;
        return (T)ui;
    }

    public void Hide<T>() where T : UIBase
    {
        string uiName = typeof(T).ToString();
        Hide(uiName);
    }

    public void Hide(string uiName)
    {
        UIBase go = uiDict[uiName];
        uiDict.Remove(uiName);
        Destroy(go.canvas.gameObject);
    }
}
