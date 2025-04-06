using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class DataButtons : UIBase
{
    protected override void Awake()
    {
        base.Awake();

        List<Button> buttonList = GetComponentsInChildren<Button>().ToList<Button>();
        // 닫기 버튼 제거
        buttonList.RemoveAt(buttonList.Count - 1);
        foreach(Button btn in buttonList)
        {
            btn.onClick.AddListener(() => UIManager.Instance.Show<StartGamePopup>());
        }
    }

    private void Start()
    {
        transform.DOLocalMove(Vector3.up * 10, 1).SetLink(gameObject);
    }
}
