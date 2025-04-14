using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonBase<UIManager>
{
    public static float ScreenWidth { get; private set; } = 1080;
    public static float ScreenHeight { get; private set; } = 1920;

    [SerializeField]
    private Dictionary<string, UIBase> uiDict = new Dictionary<string, UIBase>();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public T Show<T>(params object[] param) where T : UIBase
    {
        string _uiName = typeof(T).ToString();
        if (uiDict.ContainsKey(_uiName)) return default;
        UIBase _go = Resources.Load<UIBase>("UI/" + _uiName);
        var _ui = Load<T>(_go, _uiName);
        uiDict[_uiName] = _ui;
        _ui.Opened(param);
        return (T)_ui;
    }

    private T Load<T>(UIBase prefab, string uiName) where T : UIBase
    {
        GameObject _newCanvasObject = new GameObject(uiName + " Canvas");

        var _canvas = _newCanvasObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var _canvasScaler = _newCanvasObject.AddComponent<CanvasScaler>();
        _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _canvasScaler.referenceResolution = Vector2.right * ScreenWidth + Vector2.up * ScreenHeight;

        _newCanvasObject.AddComponent<GraphicRaycaster>();

        UIBase _ui = Instantiate(prefab, _newCanvasObject.transform);
        _ui.name = _ui.name.Replace("(Clone)", "");
        _ui.Canvas = _canvas;
        _ui.Canvas.sortingOrder = uiDict.Count;
        return (T)_ui;
    }

    public void Hide<T>() where T : UIBase
    {
        string _uiName = typeof(T).ToString();
        Hide(_uiName);
    }

    public void Hide(string uiName)
    {
        UIBase _go = uiDict[uiName];
        uiDict.Remove(uiName);
        Destroy(_go.Canvas.gameObject);
    }

    public bool ContainsData(string uiName)
    {
        return uiDict.ContainsKey(uiName); 
    }
}
