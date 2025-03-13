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
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadSceneAsync("MainScene");
        });

        NoBtn.onClick.AddListener(Hide);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UIManager.Instance.Show<Main>();

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
