using UnityEngine;
using UnityEngine.UI;

public class Construction : UIBase
{
    [SerializeField] private Button back;

    protected override void Awake()
    {
        base.Awake();
        back.onClick.AddListener(Hide);
    }
}
