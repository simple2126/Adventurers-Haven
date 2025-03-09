using UnityEngine;

public class DataButtons : UIBase
{
    public void Showpopup(int index)
    {

    }

    public void ShowTitle()
    {
        UIManager.Instance.Show<TitlePopupMain>();
        Hide();
    }
}
