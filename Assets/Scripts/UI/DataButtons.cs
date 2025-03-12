using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DataButtons : UIBase
{
    [Header("DataButtons")]
    public Button[] Buttons; 

    protected void Awake()
    {
        base.Awake();
        
        foreach(Button btn in Buttons)
        {
            btn.onClick.AddListener(() => SceneManager.LoadScene("MainScene"));
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
}
