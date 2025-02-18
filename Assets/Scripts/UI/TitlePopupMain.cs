using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitlePopupMain : UIBase
{
    public void Showpopup(int index)
    {
        switch (index)
        {
            case 0: UIManager.Instance.Show<DataButtons>();
                break;
            case 1: UIManager.Instance.Show<OptionPanel>();
                break;
        }
    }
}
