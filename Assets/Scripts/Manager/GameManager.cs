using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonBase<GameManager>
{
    protected override void Awake()
    {
        base.Awake();

        Application.targetFrameRate = 60;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ShowUI<Robby>(BgmType.Robby);
    }

    private void ShowUI<TUI>(BgmType type) where TUI : UIBase
    {
        UIManager.Instance.Show<TUI>();
        SoundManager.Instance.PlayBGM(type);
    }

    public void LoadSceneAndShowUI<T>(string sceneName) where T : UIBase
    {
        SceneManager.sceneLoaded += OnSceneLoaded<T>;
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded<T>(Scene scene, LoadSceneMode mode) where T : UIBase
    {
        UIManager.Instance.Show<T>();
        SceneManager.sceneLoaded -= OnSceneLoaded<T>;
    }
}
