using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class DataButtons : UIBase
{
    protected override void Awake()
    {
        base.Awake();

        Button[] buttons = GetComponentsInChildren<Button>();
        foreach(Button btn in buttons)
        {
            btn.onClick.AddListener(() => UIManager.Instance.Show<StartGamePopup>());
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
        UIManager.Instance.Show<Title>();
        Hide();
    }
}
