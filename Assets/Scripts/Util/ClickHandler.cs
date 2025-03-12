using UnityEngine;
using UnityEngine.EventSystems;


public class ClickHandler : MonoBehaviour, IPointerClickHandler
{
    public SfxType Type;

    public void SetSfxType(SfxType sfxType)
    {
        Type = sfxType;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySFX(Type);
    }
}