using UnityEngine;

public class UIBase : MonoBehaviour
{
    public Canvas canvas;
    public RenderMode renderMode;

    public virtual void Opened(params object[] param)
    {
        canvas.renderMode = renderMode;
    }

    public void Hide()
    {
        UIManager.Instance.Hide(gameObject.name);
    }
}
