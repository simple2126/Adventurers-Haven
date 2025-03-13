using DG.Tweening;
using UnityEngine;

public class Title : UIBase
{
    private CanvasGroup canvasGroup;
    public float FadeDuration;

    protected override void Awake()
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
