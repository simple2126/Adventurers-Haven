using UnityEngine;

public class TitleScene : MonoBehaviour
{
    void Start()
    {
        UIManager.Instance.Show<TitlePopupMain>();
    }
}
