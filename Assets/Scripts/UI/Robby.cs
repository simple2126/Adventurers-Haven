using DG.Tweening;
using UnityEngine;

public class Robby : UIBase
{
    private CanvasGroup canvasGroup;
    public float FadeDuration;

    private void OnEnable()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, FadeDuration).SetLink(gameObject);

        SoundManager.Instance.PlayBGM(BgmType.Robby);
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
