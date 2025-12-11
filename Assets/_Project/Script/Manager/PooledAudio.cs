using System.Collections;
using UnityEngine;

/// <summary>
/// 풀링된 오디오 소스 래퍼
/// 
/// [책임]
/// - AudioSource 재생 관리
/// - 재생 완료 후 자동으로 풀에 반환
/// - 생명주기 자체 관리
/// 
/// [사용]
/// SoundManager가 Get() 후 Setup()만 호출
/// 이후 자동으로 재생 → 대기 → 반환
/// </summary>
[RequireComponent(typeof(AudioSource))]
// IPooledObject 인터페이스로 추상화 할지 말지 고민
public class PooledAudio : MonoBehaviour
{
    private AudioSource audioSource;
    private Coroutine returnCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// 오디오 설정 및 재생 시작
    /// </summary>
    public void Setup(AudioClip clip, float volume, float pitch, float spatialBlend, bool loop)
    {
        if (clip == null)
        {
            Debug.LogWarning("[PooledAudio] AudioClip이 null입니다. 반환합니다.");
            ReturnToPool();
            return;
        }

        // 기존 코루틴 중지
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        // AudioSource 설정
        audioSource.clip = clip;
        audioSource.volume = Mathf.Clamp01(volume);
        audioSource.pitch = pitch <= 0f ? 1f : pitch;
        audioSource.spatialBlend = Mathf.Clamp01(spatialBlend);
        audioSource.loop = loop;

        // 재생 시작
        audioSource.Play();

        // 루프가 아니면 자동 반환
        if (!loop)
        {
            float duration = clip.length / Mathf.Max(0.01f, audioSource.pitch);
            returnCoroutine = StartCoroutine(ReturnAfterDelay(duration));
        }
    }

    /// <summary>
    /// 지연 후 풀에 반환
    /// </summary>
    private IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay + 0.02f);
        ReturnToPool();
    }

    /// <summary>
    /// 수동으로 정지하고 풀에 반환
    /// </summary>
    public void StopAndReturn()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        ReturnToPool();
    }

    /// <summary>
    /// 풀에 반환
    /// </summary>
    private void ReturnToPool()
    {
        // AudioSource 정리
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }

        // 코루틴 정리
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        // ObjectPooling으로 반환
        ObjectPooling.Return(gameObject);
    }

    private void OnDisable()
    {
        // 비활성화 시 코루틴 정리
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
    }
}

