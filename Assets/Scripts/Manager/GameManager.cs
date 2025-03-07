using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    protected override void Awake()
    {
        base.Awake();

        Application.targetFrameRate = 60;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SoundManager.Instance.PlayBGM(BgmType.Robby);
    }
}
