using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionPanel : UIBase
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
