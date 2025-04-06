using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ConstructionPanel : UIBase
{
    [SerializeField] private Button back;
    [SerializeField] private Button buildButton;
    [SerializeField] private Button roadButton;

    [SerializeField] private GameObject Content;
    [SerializeField] private GameObject itemBox;

    private List<GameObject> buildItemList = new List<GameObject>();
    private List<GameObject> roadItemList = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        back.onClick.AddListener(Hide);
        buildButton.onClick.AddListener(ChangeTab);
        roadButton.onClick.AddListener(ChangeTab);
    }

    private void SetBuilItemList()
    {
    }

    private void ChangeTab()
    {

    }
}
