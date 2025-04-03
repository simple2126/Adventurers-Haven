using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGamePopup : UIBase
{
    public Button YesBtn;
    public Button NoBtn;

    protected override void Awake()
    {
        base.Awake();

        YesBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.LoadSceneAndShowUI<Main>("MainScene");
        });

        NoBtn.onClick.AddListener(Hide);
    }
}
