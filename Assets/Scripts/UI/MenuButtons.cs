using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtons : UIBase
{
    [Header("Menu Child")]
    private bool isVisible = false; // 현재 보여져야 하는지 여부
    [SerializeField] private Button[] childBtns;
    [SerializeField] private CanvasGroup[] canvasGroups;

    protected override void Awake()
    {
        base.Awake();

        childBtns = GetComponentsInChildren<Button>(true).ToArray();
        canvasGroups = new CanvasGroup[childBtns.Length];

        for (int i = 0; i < childBtns.Length; i++)
        {
            SetChildButton(i);
        }
    }

    private void OnEnable()
    {
        ShowOrHideMenu(true);
    }

    private void OnDisable()
    {
        ShowOrHideMenu(false);
    }

    private void SetChildButton(int index)
    {
        int idx = index;
        CanvasGroup group = childBtns[idx].gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        canvasGroups[idx] = group;

        childBtns[idx].onClick.AddListener(() => ClickChildBtn(idx));
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

    private void ShowOrHideMenu(bool isVisible)
    {
        this.isVisible = isVisible;

        for (int i = 0; i < childBtns.Length; i++)
        {
            AnimateChildButton(i);
        }
    }

    private void AnimateChildButton(int index)
    {
        int idx = index;
        childBtns[index].gameObject.SetActive(true);

        DG.Tweening.Sequence seq = DOTween.Sequence();
        Vector3 offsetY = (isVisible ? Vector3.up : Vector3.down) * 10;
        Vector3 targetPos = childBtns[index].transform.localPosition + offsetY;

        canvasGroups[index].alpha = isVisible ? 0 : 1;

        seq.Append(childBtns[idx].transform.DOLocalMove(targetPos, 1))
            .Join(canvasGroups[idx].DOFade(isVisible ? 1 : 0, 1f))
            .SetLink(childBtns[idx].gameObject);

        seq.OnComplete(() =>
        {
            if (!isVisible)
            {
                childBtns[idx].gameObject.SetActive(false);
            }
        });
    }
}
