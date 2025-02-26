using System.Collections;
using UnityEngine;

public class SfxSoundSource : MonoBehaviour
{
    private AudioSource _audioSource;
    private SfxType _type;

    public void Play(AudioClip clip, SfxType sfxType, float soundEffectVolume, float soundEffectPitchVaricance)
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        CancelInvoke();
        _type = sfxType;
        _audioSource.clip = clip;
        _audioSource.volume = soundEffectVolume;
        _audioSource.pitch = 1f + Random.Range(-soundEffectPitchVaricance, soundEffectPitchVaricance); // 다양한 음향 효과
        _audioSource.PlayOneShot(clip);

        StartCoroutine(CoDisable(clip.length));
    }

    // 비활성화 및 풀 반환
    private IEnumerator CoDisable(float length)
    {
        yield return new WaitForSecondsRealtime(length);
        gameObject.SetActive(false);
        PoolManager.Instance.ReturnToPool(_type.ToString(), this);
    }
}