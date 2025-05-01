using UnityEngine;
using UnityEngine.UI;

public class Main : UIBase
{
    // 테스트용
    [SerializeField] private Button robbyBtn;

    [SerializeField] private Button menuBtn;

    private void Start()
    {
        SoundManager.Instance.PlayBGM(BgmType.Main);

        robbyBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.LoadSceneAndShowUI<Robby>(SceneName.RobbyScene);
        });

        menuBtn.onClick.AddListener(() =>
        {
            bool isMenuVisible = UIManager.Instance.IsVisible("MenuButtons");

            if(!isMenuVisible) UIManager.Instance.Show<MenuButtons>();
            else UIManager.Instance.Hide<MenuButtons>();
        });
    }

    public void Showpopup(int index)
    {
        switch (index)
        {
            case 0:
                UIManager.Instance.Show<OptionPanel>();
                break;
        }
    }
}
