using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public class UIBase : MonoBehaviour
{
    public Canvas Canvas;
    public RenderMode RenderMode;

    [System.Serializable]
    public class ButtonSfxPair
    {
        public Button Button;
        public SfxType SfxType;
    }

    [SerializeField]
    private SfxType[] sfxTypeArr;

    public List<ButtonSfxPair> ButtonSfxPairList = new List<ButtonSfxPair>();

    protected virtual void Awake()
    {
        Button[] _allButtons = GetComponentsInChildren<Button>(true);

        int _minCount = Mathf.Min(_allButtons.Length, sfxTypeArr.Length);

        for (int i = 0; i < _allButtons.Length; i++)
        {
            SfxType _sfxType = (i < _minCount) ? sfxTypeArr[i] : SfxType.Click;
            Console.WriteLine($"sfxType == {_sfxType}");
            
            ButtonSfxPairList.Add(new ButtonSfxPair { Button = _allButtons[i], SfxType = _sfxType });

            ClickHandler clickHandler = _allButtons[i].gameObject.AddComponent<ClickHandler>();
            clickHandler.SetSfxType(_sfxType);
        }
    }

    public virtual void Opened(params object[] param)
    {
        Canvas.renderMode = RenderMode;
    }

    public void Hide()
    {
        UIManager.Instance.Hide(gameObject.name);
    }
}
