using UnityEngine;
using _Project.Script.Manager;

namespace _Project.Script.Interface
{
    /// <summary>
    /// 네트워크 최적화 인터페이스
    /// 모든 네트워크 매니저가 구현해야 하는 공통 최적화 패턴
    /// </summary>
    public interface INetworkOptimizable
    {
        /// <summary>
        /// 최적화 설정 적용
        /// </summary>
        /// <param name="settings">최적화 설정</param>
        void ApplyOptimizationSettings(NetworkOptimizationSettings settings);

        /// <summary>
        /// 동기화 주기 설정
        /// </summary>
        /// <param name="interval">동기화 주기 (초)</param>
        void SetSyncInterval(float interval);

        /// <summary>
        /// 네트워크 품질 설정
        /// </summary>
        /// <param name="quality">네트워크 품질</param>
        void SetNetworkQuality(NetworkQuality quality);

        /// <summary>
        /// 최적화 상태 확인
        /// </summary>
        /// <returns>최적화 적용 여부</returns>
        bool IsOptimized();

        /// <summary>
        /// 퍼포먼스 통계 가져오기
        /// </summary>
        /// <returns>퍼포먼스 통계</returns>
        NetworkPerformanceStats GetPerformanceStats();
    }

    /// <summary>
    /// 네트워크 퍼포먼스 통계
    /// </summary>
    [System.Serializable]
    public struct NetworkPerformanceStats
    {
        public string managerName;
        public float syncInterval;
        public int rpcCount;
        public int batchCount;
        public float networkTraffic;
        public bool isOptimized;
        public float lastUpdateTime;
    }
}
