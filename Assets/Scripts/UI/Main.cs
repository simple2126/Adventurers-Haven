using DG.Tweening;
using UnityEngine;

public class Main : UIBase
{
    public void Showpopup(int index)
    {
        switch (index)
        {
            case 0:
                UIManager.Instance.Show<OptionPanel>();
                break;
            case 1:
                UIManager.Instance.Show<OptionPanel>();
                break;
        }
    }
}
