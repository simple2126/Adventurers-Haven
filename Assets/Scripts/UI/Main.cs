using DG.Tweening;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Main : UIBase
{
    // 테스트용
    [SerializeField] private Button robbyBtn;

    [SerializeField] private Button menuBtn;

    [Header("Menu Child")]
    private bool isVisible = false;
    [SerializeField] private Button[] childBtns;
    [SerializeField] private CanvasGroup[] canvasGroups;

    private void Start()
    {
        SoundManager.Instance.PlayBGM(BgmType.Main);

        robbyBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.LoadSceneAndShowUI<Robby>("RobbyScene");
        });

        menuBtn.onClick.AddListener(ShowMenu);
        childBtns = menuBtn.GetComponentsInChildren<Button>(true)
                .Where(btn => btn.gameObject != menuBtn.gameObject)
                .ToArray();
        canvasGroups = new CanvasGroup[childBtns.Length];

        for (int i = 0; i < childBtns.Length; i++)
        {
            SetChildButton(i);
        }
    }

    private void SetChildButton(int index)
    {
        CanvasGroup group = childBtns[index].gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        canvasGroups[index] = group;

        childBtns[index].onClick.AddListener(() => ClickChildBtn(index));
    }

    public void Showpopup(int index)
    {
        switch (index)
        {
            case 0:
                UIManager.Instance.Show<OptionPanel>();
                break;
        }
    }

    private void ShowMenu()
    {
        isVisible = !isVisible;

        for (int i = 0; i < childBtns.Length; i++)
        {
            AnimateChildButton(i);
        }
    }

    private void AnimateChildButton(int index)
    {
        childBtns[index].gameObject.SetActive(true);

        DG.Tweening.Sequence seq = DOTween.Sequence();
        Vector3 offsetY = (isVisible ? Vector3.up : Vector3.down) * 10;
        Vector3 targetPos = childBtns[index].transform.localPosition + offsetY;

        canvasGroups[index].alpha = isVisible ? 0 : 1;

        seq.Append(childBtns[index].transform.DOLocalMove(targetPos, 1))
            .Join(canvasGroups[index].DOFade(isVisible ? 1 : 0, 1f))
            .SetLink(childBtns[index].gameObject);

        seq.OnComplete(() =>
        {
            if (!isVisible)
            {
                childBtns[index].gameObject.SetActive(false);
            }
        });
    }
    private void ClickChildBtn(int index)
    {
        switch (index)
        {
            case 0:
                UIManager.Instance.Show<ConstructionPanel>();
                break;
        }
    }
}
