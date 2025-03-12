using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System;

public class DataButtons : UIBase
{
    [Header("DataButtons")]
    public Button[] Buttons;

    protected override void Awake()
    {
        base.Awake();
        
        foreach(Button btn in Buttons)
        {
            btn.onClick.AddListener(() =>
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.LoadSceneAsync("MainScene");
            });
        }
    }

    private void Start()
    {
        transform.DOLocalMove(Vector3.up * 10, 1).SetAutoKill(true).SetLink(gameObject);
    }

    public void Showpopup(int index)
    {

    }

    public void ShowTitle()
    {
        UIManager.Instance.Show<TitlePopupMain>();
        Hide();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UIManager.Instance.Show<MainPopupMain>();

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
