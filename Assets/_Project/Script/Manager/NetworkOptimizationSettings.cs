using UnityEngine;

namespace _Project.Script.Manager
{
    /// <summary>
    /// 네트워크 최적화 설정 - 중앙화된 최적화 관리
    /// 모든 네트워크 매니저가 동일한 설정을 사용
    /// </summary>
    [CreateAssetMenu(fileName = "NetworkOptimizationSettings", menuName = "Network/Optimization Settings")]
    public class NetworkOptimizationSettings : ScriptableObject
    {
        [Header("=== 퍼포먼스 최적화 설정 ===")]

        [Header("1. 서버 권한 모델 (MasterClient 전담)")]
        [Tooltip("MasterClient에서만 AI 처리")]
        public bool enableServerAuthority = true;

        [Header("2. 데이터 전송 최소화")]
        [Tooltip("위치 동기화 주기 (초)")]
        [Range(0.05f, 1.0f)]
        public float positionSyncInterval = 0.1f;

        [Tooltip("몬스터 동기화 주기 (초)")]
        [Range(0.1f, 2.0f)]
        public float monsterSyncInterval = 0.2f;

        [Tooltip("플레이어 스탯 동기화 주기 (초)")]
        [Range(0.5f, 5.0f)]
        public float statsSyncInterval = 1.0f;

        [Tooltip("월드 상태 동기화 주기 (초)")]
        [Range(0.5f, 3.0f)]
        public float worldSyncInterval = 1.5f;

        [Header("3. 패킷 묶기 (Batching)")]
        [Tooltip("배치당 최대 몬스터 수")]
        [Range(5, 50)]
        public int maxMonstersPerBatch = 20;

        [Tooltip("배치당 최대 아이템 수")]
        [Range(5, 30)]
        public int maxItemsPerBatch = 15;

        [Tooltip("배치당 최대 플레이어 수")]
        [Range(2, 10)]
        public int maxPlayersPerBatch = 5;

        [Header("4. LOD & 근처만 업데이트")]
        [Tooltip("플레이어 근처 동기화 반경 (미터)")]
        [Range(10f, 100f)]
        public float playerSyncRadius = 50f;

        [Tooltip("LOD 활성화")]
        public bool enableLOD = true;

        [Tooltip("LOD 업데이트 주기 (초)")]
        [Range(0.5f, 5.0f)]
        public float lodUpdateInterval = 2.0f;

        [Header("5. 보간/예측")]
        [Tooltip("보간 처리 활성화")]
        public bool enableInterpolation = true;

        [Tooltip("보간 속도")]
        [Range(1f, 20f)]
        public float interpolationSpeed = 6f;

        [Tooltip("예측 활성화")]
        public bool enablePrediction = true;

        [Header("6. 네트워크 품질 설정")]
        [Tooltip("네트워크 품질 (낮음/보통/높음)")]
        public NetworkQuality networkQuality = NetworkQuality.Medium;

        [Tooltip("자동 품질 조절")]
        public bool enableAutoQuality = true;

        [Header("7. 디버그 및 모니터링")]
        [Tooltip("퍼포먼스 모니터링 활성화")]
        public bool enablePerformanceMonitoring = true;

        [Tooltip("디버그 로그 활성화")]
        public bool enableDebugLog = true;

        [Tooltip("통계 업데이트 주기 (초)")]
        [Range(1f, 10f)]
        public float statsUpdateInterval = 5f;
    }

    /// <summary>
    /// 네트워크 품질 레벨
    /// </summary>
    public enum NetworkQuality
    {
        Low,     // 낮음 - 최대 최적화
        Medium,  // 보통 - 균형
        High     // 높음 - 최고 품질
    }
}
