using UnityEngine;
using UnityEngine.EventSystems;

public class SFXHandler : MonoBehaviour, IPointerClickHandler
{
    public SfxType type;

    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySFX(type);
    }
}
