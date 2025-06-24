using UnityEngine;

public class NotRoadSign : UIBase
{
    protected override void Awake()
    {
        base.Awake();
        Canvas = GetComponent<Canvas>();
        gameObject.SetActive(false);
    }
}
