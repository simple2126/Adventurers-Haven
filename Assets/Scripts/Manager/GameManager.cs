using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonBase<GameManager>
{
    public bool IsFrame30 { get; private set; } = false;

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
        SceneManager.sceneLoaded += OnSceneLoaded<T>;
        SceneManager.LoadSceneAsync(sceneName);
    }

    private void OnSceneLoaded<T>(Scene scene, LoadSceneMode mode) where T : UIBase
    {
        UIManager.Instance.Show<T>();
        SceneManager.sceneLoaded -= OnSceneLoaded<T>;
    }

    public void SetIsFrame30(bool isFrame30)
    {
        IsFrame30 = isFrame30;
    }
}
