using DG.Tweening;
using UnityEngine;

public class TitlePopupMain : UIBase
{
    private CanvasGroup canvasGroup;
    public float FadeDuration;

    private void Awake()
    {
        base.Awake();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        canvasGroup.DOFade(1f, FadeDuration).SetLink(gameObject);
    }

    public void Showpopup(int index)
    {
        switch (index)
        {
            case 0: UIManager.Instance.Show<DataButtons>();
                break;
            case 1: UIManager.Instance.Show<OptionPanel>();
                break;
        }
    }
}
