using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonBase<GameManager>
{
    public bool IsFrame30 = false;

    protected override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = IsFrame30 ? 30 : 60;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UIManager.Instance.Show<Robby>();
    }

    public void LoadSceneAndShowUI<T>(string sceneName) where T : UIBase
    {
        Debug.Log($"LoadSceneAndShowUI 실행 - Scene: {sceneName}, UI: {typeof(T).Name}");
        SceneManager.sceneLoaded += OnSceneLoaded<T>;
        SceneManager.LoadSceneAsync(sceneName);
    }

    private void OnSceneLoaded<T>(Scene scene, LoadSceneMode mode) where T : UIBase
    {
        Debug.Log($"OnSceneLoaded 실행됨 - Scene: {scene.name}, UI: {typeof(T).Name}");
        UIManager.Instance.Show<T>();
        SceneManager.sceneLoaded -= OnSceneLoaded<T>;
    }
}
