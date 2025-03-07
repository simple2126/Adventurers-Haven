using UnityEngine;
using UnityEngine.EventSystems;

public class ClickHandler : MonoBehaviour, IPointerClickHandler
{
    public SfxType type;

    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySFX(type);
    }
}
