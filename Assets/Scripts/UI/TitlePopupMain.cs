using DG.Tweening;
using UnityEngine;

public class TitlePopupMain : UIBase
{
    private CanvasGroup _canvasGroup;
    public float fadeDuration;

    private void Awake()
    {
        base.Awake();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        _canvasGroup.DOFade(1f, fadeDuration).SetLink(gameObject);
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
