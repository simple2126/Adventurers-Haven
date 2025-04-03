using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Construction : UIBase
{
    [SerializeField] private Button back;

    protected override void Awake()
    {
        back.onClick.AddListener(Hide);
    }
}
