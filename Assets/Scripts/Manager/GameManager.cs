using System;
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

    public void LoadSceneAndShowUI<T>(SceneName sceneType) where T : UIBase
    {
        SceneManager.sceneLoaded += OnSceneLoaded<T>;
        SceneManager.LoadSceneAsync(sceneType.ToString());
    }

    private void OnSceneLoaded<T>(Scene scene, LoadSceneMode mode) where T : UIBase
    {
        UIManager.Instance.Show<T>();
        if (scene.name == SceneName.MainScene.ToString())
        {
            MapManager.Instance.gameObject?.SetActive(true);
            MapManager.Instance?.ShowOrHideTileDict(true);
        }
        else
        {
            MapManager.Instance.gameObject?.SetActive(false);
            MapManager.Instance?.ShowOrHideTileDict(false);
        }
        SceneManager.sceneLoaded -= OnSceneLoaded<T>;
    }

    public void SetIsFrame30(bool isFrame30)
    {
        IsFrame30 = isFrame30;
    }
}
