# 사운드/이펙트 매니저 설계 개선 문서

## 📋 개선 내용 요약

### Before (기존 설계)
```
SoundManager → ObjectPooling.Get() → GameObject + AudioSource
             → Play()
             → StartCoroutine(ReturnAfter) ← 매니저가 생명주기 관리
             → ObjectPooling.Return()
```

### After (개선된 설계)
```
SoundManager → ObjectPooling.Get() → GameObject + AudioSource + PooledAudio
             → PooledAudio.Setup() ← 위임
             
PooledAudio → Play()
            → StartCoroutine(ReturnAfter) ← 자신이 생명주기 관리
            → ObjectPooling.Return()
```

---

## 🎯 핵심 개선 사항

### 1. **책임 분리 (Separation of Concerns)**

#### Before: 매니저가 너무 많은 책임
```csharp
// SoundManager가 담당:
// 1. 사운드 리소스 관리
// 2. AudioMixer 라우팅
// 3. 풀링 관리
// 4. 재생 제어
// 5. 생명주기 관리 (코루틴)
// 6. 반환 처리
```

#### After: 책임 분산
```csharp
// SoundManager: 설정 및 조율
// - 사운드 리소스 관리
// - AudioMixer 라우팅
// - 풀링 요청

// PooledAudio: 생명주기 관리
// - 재생 제어
// - 자동 반환
// - 코루틴 관리
```

---

### 2. **단일 책임 원칙 (Single Responsibility Principle)**

#### PooledAudio.cs
```csharp
/// [책임]
/// - AudioSource 재생 관리
/// - 재생 완료 후 자동으로 풀에 반환
/// - 생명주기 자체 관리

public void Setup(...)  // 설정 및 재생 시작
public void StopAndReturn()  // 수동 중지
private void ReturnToPool()  // 자동 반환
```

#### PooledEffect.cs
```csharp
/// [책임]
/// - 이펙트 프리팹 재생 관리
/// - duration 후 자동으로 풀에 반환
/// - 생명주기 자체 관리

public void Setup(...)  // 설정 및 재생 시작
public void StopAndReturn()  // 수동 중지
private void ReturnToPool()  // 자동 반환
```

---

### 3. **매니저 단순화**

#### SoundManager.Play()
```csharp
// Before: 30+ 줄 (재생 + 코루틴 관리)
public void Play(SoundRequest request)
{
    // ... 설정 ...
    src.Play();
    if (loop == false)
    {
        StartCoroutine(ReturnAfter(src, go, duration));  // 직접 관리
    }
}

// After: 15줄 (설정만)
public void Play(SoundRequest request)
{
    // ... 설정 ...
    pooledAudio.Setup(clip, volume, pitch, spatialBlend, loop);  // 위임!
}
```

---

## 🏗️ 아키텍처 개선

### 계층 구조

```
┌─────────────────────────────────────────────────────┐
│ 사용자 코드 (게임 로직)                               │
│ - EventHub.RaiseEvent()                             │
│ - SoundManager.Instance.Play()                      │
└─────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────┐
│ 매니저 계층 (조율자)                                  │
│ - SoundManager: 리소스 관리, 라우팅, 풀 요청          │
│ - EffectManager: 리소스 관리, 풀 요청                │
└─────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────┐
│ 풀링 객체 계층 (실행자)                               │
│ - PooledAudio: 재생 + 생명주기 관리                  │
│ - PooledEffect: 재생 + 생명주기 관리                 │
└─────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────┐
│ 풀링 인프라 (메모리 관리)                             │
│ - ObjectPooling: Get/Return/Warmup                  │
└─────────────────────────────────────────────────────┘
```

---

## ✅ 장점

### 1. **유지보수성 향상**
- 각 클래스가 명확한 역할 하나만 담당
- 버그 발생 시 책임 소재가 명확
- 코드 수정 시 영향 범위 최소화

### 2. **테스트 용이성**
```csharp
// PooledAudio 단독 테스트 가능
var pooledAudio = gameObject.AddComponent<PooledAudio>();
pooledAudio.Setup(testClip, 1f, 1f, 0f, false);
// 자동 반환 확인
```

### 3. **확장성**
- 새로운 풀링 타입 추가 시 동일 패턴 적용
```csharp
// 예: PooledParticle, PooledUI 등
public class PooledParticle : MonoBehaviour
{
    public void Setup(...) { }
    private void ReturnToPool() { }
}
```

### 4. **코루틴 관리 간소화**
- 각 풀링 객체가 자신의 코루틴만 관리
- 매니저에서 코루틴 추적 불필요
- OnDisable에서 자동 정리

### 5. **메모리 누수 방지**
```csharp
private void OnDisable()
{
    // 비활성화 시 코루틴 자동 정리
    if (returnCoroutine != null)
    {
        StopCoroutine(returnCoroutine);
        returnCoroutine = null;
    }
}
```

---

## 🎮 사용 예시

### 기존 사용법 유지 (외부 인터페이스 변화 없음)

```csharp
// EventHub 사용
EventHub.Instance.RaiseEvent(new RequestPlaySoundEvent
{
    id = "SFX_Attack",
    position = transform.position,
    volume = 0.8f,
    mixerGroupName = "SFXVolum"
});

// 직접 호출
SoundManager.Instance.Play(new SoundManager.SoundRequest { ... });
```

### 내부 동작 개선

```csharp
// Before: SoundManager가 코루틴 관리
// SoundManager.Play()
// → src.Play()
// → StartCoroutine(ReturnAfter)  ← 매니저 코루틴
// → Wait...
// → ObjectPooling.Return()

// After: PooledAudio가 자율적으로 관리
// SoundManager.Play()
// → pooledAudio.Setup()  ← 위임
//     → src.Play()
//     → StartCoroutine(ReturnAfterDelay)  ← 자신의 코루틴
//     → Wait...
//     → ReturnToPool()
```

---

## 🔧 추가 개선 가능 사항

### 1. **이벤트 기반 반환 알림**
```csharp
public class PooledAudio : MonoBehaviour
{
    public event System.Action OnReturned;
    
    private void ReturnToPool()
    {
        OnReturned?.Invoke();  // 반환 전 알림
        ObjectPooling.Return(gameObject);
    }
}
```

### 2. **상태 추적**
```csharp
public enum PooledAudioState
{
    Idle,
    Playing,
    Stopping,
    Returning
}

public PooledAudioState State { get; private set; }
```

### 3. **디버깅 지원**
```csharp
#if UNITY_EDITOR
[Header("Debug Info")]
[SerializeField, ReadOnly] private float remainingTime;
[SerializeField, ReadOnly] private string currentClipName;
#endif
```

---

## 📊 성능 비교

| 항목 | Before | After | 개선 |
|-----|--------|-------|-----|
| 코루틴 수 | SoundManager × N | PooledAudio × N | 분산 |
| 코드 라인 수 (SoundManager) | ~230 | ~210 | -20줄 |
| 책임 수 (SoundManager) | 6개 | 3개 | -50% |
| 테스트 복잡도 | 높음 | 낮음 | ✅ |
| 확장 용이성 | 중간 | 높음 | ✅ |

---

## 🎯 결론

### 핵심 원칙 준수
- ✅ **단일 책임 원칙 (SRP)**: 각 클래스가 하나의 역할
- ✅ **개방-폐쇄 원칙 (OCP)**: 확장 용이, 수정 최소화
- ✅ **의존성 역전 원칙 (DIP)**: 매니저가 구체적인 생명주기 관리에 의존하지 않음

### 코드 품질 향상
- 가독성: 각 클래스의 목적이 명확
- 유지보수성: 버그 수정 및 기능 추가 용이
- 확장성: 새로운 풀링 타입 쉽게 추가

### 실전 적용
- 기존 외부 인터페이스 유지 (하위 호환성)
- 내부 구조만 개선 (점진적 리팩토링 가능)
- 성능 저하 없음 (오히려 코드 정리로 최적화)

