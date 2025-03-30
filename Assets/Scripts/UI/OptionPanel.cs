using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class OptionPanel : UIBase
{
    [SerializeField] private Button toggleBtn;
    [SerializeField] private GameObject[] objs; 
    private bool isFrame30 = false;

    protected override void Awake()
    {
        base.Awake();
        toggleBtn.onClick.AddListener(SwapImage);
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

    private void SwapImage()
    {
        isFrame30 = !isFrame30;
        Application.targetFrameRate = Application.targetFrameRate == 60 ? 30 : 60;
        objs[isFrame30 == true ? 1 : 0].SetActive(true);
        objs[isFrame30 == true ? 0 : 1].SetActive(false);
    }
}
