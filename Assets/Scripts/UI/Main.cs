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
            // ❌ 클로저 문제 없음 -> index 없이 i 사용 가능
            childBtns[i].AddComponent<CanvasGroup>();
            CanvasGroup group = childBtns[i].GetComponent<CanvasGroup>();
            group.alpha = 0f;
            canvasGroups[i] = group;

            // ✅ 클로저 문제 발생 가능 -> index 사용
            int index = i;
            childBtns[index].onClick.AddListener(() => ClickChildBtn(index));
        }
    }

    public void Showpopup(int index)
    {
        switch (index)
        {
            case 0:
                UIManager.Instance.Show<OptionPanel>();
                break;
            case 1:
                UIManager.Instance.Show<OptionPanel>();
                break;
        }
    }

    private void ShowMenu()
    {
        isVisible = !isVisible;


        for (int i = 0; i < childBtns.Length; i++)
        {
            // ❌ 클로저 문제 없음 -> index 없이 i 사용 가능
            DG.Tweening.Sequence seq = DOTween.Sequence();
            Vector3 targetPos = childBtns[i].transform.localPosition + ((isVisible ? Vector3.up : Vector3.down) * 10);

            seq.Append(childBtns[i].transform.DOLocalMove(targetPos, 1))
                .Join(canvasGroups[i].DOFade(isVisible ? 1 : 0, 1f))
                .SetAutoKill(true)
                .SetLink(childBtns[i].gameObject);

            // ✅ 클로저 문제 발생 가능 -> index 사용
            int index = i;
            seq.OnComplete(() => childBtns[index].gameObject.SetActive(isVisible));
        }
    }

    private void ClickChildBtn(int index)
    {
        switch (index)
        {
            case 0:
                UIManager.Instance.Show<Construction>();
                break;
        }
    }
}
