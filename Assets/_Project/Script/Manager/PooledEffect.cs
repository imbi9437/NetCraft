using System.Collections;
using UnityEngine;

/// <summary>
/// 풀링된 이펙트 래퍼
/// 
/// [책임]
/// - 이펙트 프리팹 재생 관리
/// - 파티클 시간 + startLifetime 후 자동으로 풀에 반환
/// - 생명주기 자체 관리
/// 
/// [사용]
/// EffectManager가 Get() 후 Setup()만 호출
/// 이후 자동으로 재생 → 대기 → 반환
/// </summary> 
// IPooledObject 인터페이스로 추상화 할지 말지 고민
public class PooledEffect : MonoBehaviour
{
    private GameObject currentEffect;
    private Coroutine returnCoroutine;

    /// <summary>
    /// 이펙트 설정 및 재생 시작
    /// </summary>
    public void Setup(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent,
                      float duration, bool worldSpace, Vector3 scale)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[PooledEffect] Prefab이 null입니다. 반환합니다.");
            ReturnToPool();
            return;
        }

        // 기존 코루틴 중지
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        // 기존 이펙트 정리
        ClearCurrentEffect();

        // 프리팹 인스턴스화
        currentEffect = Instantiate(prefab, transform);
        currentEffect.transform.localPosition = Vector3.zero;
        currentEffect.transform.localRotation = Quaternion.identity;
        currentEffect.transform.localScale = Vector3.one;

        // Transform 설정
        if (parent != null)
        {
            transform.SetParent(parent, worldSpace);
            if (worldSpace)
            {
                transform.position = position;
                transform.rotation = rotation;
            }
            else
            {
                transform.localPosition = position;
                transform.localRotation = rotation;
            }
        }
        else
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        // 스케일 설정
        if (scale != Vector3.zero)
        {
            transform.localScale = scale;
        }

        // ParticleSystem 자동 재생 및 최대 재생 시간 계산
        var particles = currentEffect.GetComponentsInChildren<ParticleSystem>();
        float maxParticleDuration = 0f;

        foreach (var ps in particles)
        {
            ps.Play();

            // ParticleSystem의 실제 재생 시간 계산
            float particleDuration = ps.main.duration;
            if (ps.main.loop == false)  // 루프가 아닐 때만 시간 계산
            {
                // duration + startLifetime으로 완전히 사라질 때까지 시간 계산
                float totalDuration = particleDuration + ps.main.startLifetime.constantMax;
                if (totalDuration > maxParticleDuration)
                    maxParticleDuration = totalDuration;
            }
        }

        // duration 결정: 사용자 지정 > 파티클 시간 > 기본값
        float actualDuration;
        if (duration > 0f)
        {
            actualDuration = duration;  // 사용자가 명시적으로 지정
        }
        else if (maxParticleDuration > 0f)
        {
            actualDuration = maxParticleDuration + 0.1f;  // 파티클 시간 + 여유
        }
        else
        {
            actualDuration = 1f;  // 기본값
        }

        returnCoroutine = StartCoroutine(ReturnAfterDelay(actualDuration));
    }

    /// <summary>
    /// 지연 후 풀에 반환
    /// </summary>
    private IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
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

        // ParticleSystem 정지
        if (currentEffect != null)
        {
            var particles = currentEffect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
            {
                ps.Stop();
            }
        }

        ReturnToPool();
    }

    /// <summary>
    /// 현재 이펙트 정리
    /// </summary>
    private void ClearCurrentEffect()
    {
        if (currentEffect != null)
        {
            Destroy(currentEffect);
            currentEffect = null;
        }
    }

    /// <summary>
    /// 풀에 반환
    /// </summary>
    private void ReturnToPool()
    {
        // 이펙트 정리
        ClearCurrentEffect();

        // 코루틴 정리
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        // 부모 초기화 (풀 부모로)
        // transform.SetParent는 EffectManager의 poolParent로 설정됨

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

