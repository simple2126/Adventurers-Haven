using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public class UIBase : MonoBehaviour
{
    public Canvas canvas;
    public RenderMode renderMode;

    [System.Serializable]
    public class ButtonSfxPair
    {
        public Button button;
        public SfxType sfxType;
    }

    [SerializeField]
    private SfxType[] _sfxTypeArr;

    public List<ButtonSfxPair> buttonSfxPairList = new List<ButtonSfxPair>();

    protected void Awake()
    {
        Button[] allButtons = GetComponentsInChildren<Button>(true);

        int minCount = Mathf.Min(allButtons.Length, _sfxTypeArr.Length);

        for (int i = 0; i < allButtons.Length; i++)
        {
            SfxType sfxType = (i < minCount) ? _sfxTypeArr[i] : SfxType.Click;
            Console.WriteLine($"sfxType == {sfxType}");
            buttonSfxPairList.Add(new ButtonSfxPair { button = allButtons[i], sfxType = sfxType });

            ClickHandler clickHandler = allButtons[i].gameObject.AddComponent<ClickHandler>();
            clickHandler.SetSfxType(sfxType);
        }

        if (allButtons.Length != _sfxTypeArr.Length)
        {
            Debug.LogWarning($"⚠️ Button 개수({allButtons.Length})와 SfxType 개수({_sfxTypeArr.Length})가 다릅니다!");
        }
    }

    public virtual void Opened(params object[] param)
    {
        canvas.renderMode = renderMode;
    }

    public void Hide()
    {
        UIManager.Instance.Hide(gameObject.name);
    }
}
