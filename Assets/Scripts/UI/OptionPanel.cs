using DG.Tweening;
using UnityEngine;

public class OptionPanel : UIBase
{
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
