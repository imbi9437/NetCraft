using _Project.Script.Generic;
using _Project.Script.EventStruct;
using _Project.Script.Manager;
using _Project.Script.Items.Network;
using _Project.Script.Interface;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PlayerStateMachine = _Project.Script.Character.Player.PlayerStateMachine;

namespace _Project.Script.Character.Network
{
    /// <summary>
    /// PUN2 기반 플레이어 간 상호작용 관리자
    /// 아이템 공유, 단순한 PvP 데미지 처리를 실시간으로 동기화
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    [DefaultExecutionOrder(-50)]
    public class NetworkPlayerInteraction : MonoSingleton<NetworkPlayerInteraction>, INetworkOptimizable, IPunObservable
    {
        [Header("상호작용 설정")]
        [SerializeField] private float interactionRange = 5f; // 상호작용 범위
        [SerializeField] private float attackRange = 3f; // 공격 범위
        [SerializeField] private float attackDamage = 20f; // 공격 데미지
        [SerializeField] private float attackCooldown = 1f; // 공격 쿨다운

        // 상호작용 상태
        private Dictionary<int, PlayerInteractionData> playerInteractions = new Dictionary<int, PlayerInteractionData>();

        // 로컬 상태
        private float lastAttackTime = 0f;

        // 최적화 관련 변수들
        private bool isOptimized = false;
        private NetworkPerformanceStats performanceStats;

        protected override void Awake()
        {
            base.Awake();

            // 초기화
            InitializeInteraction();
        }

        private void Start()
        {
            // 이벤트 구독
            EventHub.Instance.RegisterEvent<OnPlayerSpawnedEvent>(OnPlayerSpawned);
            EventHub.Instance.RegisterEvent<PunEvents.OnPlayerLeftRoomEvent>(OnPlayerLeftRoom);
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (EventHub.Instance != null)
            {
                EventHub.Instance.UnregisterEvent<OnPlayerSpawnedEvent>(OnPlayerSpawned);
                EventHub.Instance.UnregisterEvent<PunEvents.OnPlayerLeftRoomEvent>(OnPlayerLeftRoom);
            }
        }

        #region 초기화

        /// <summary>
        /// 상호작용 시스템 초기화
        /// </summary>
        private void InitializeInteraction()
        {
            Debug.Log("[NetworkPlayerInteraction] 상호작용 시스템 초기화 완료");
        }

        #endregion

        #region PUN2 동기화

        /// <summary>
        /// PUN2 데이터 동기화
        /// </summary>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // 단순화: 상호작용 데이터는 이벤트 기반으로만 처리
            // 실시간 동기화는 필요하지 않음
        }

        #endregion

        #region 단순 공격 시스템

        /// <summary>
        /// 플레이어 공격 (단순 데미지 처리)
        /// </summary>
        public void AttackPlayer(int targetActorNumber)
        {
            // 쿨다운 확인
            if (Time.time - lastAttackTime < attackCooldown) return;

            // 거리 확인
            var targetPlayer = GetPlayerByActorNumber(targetActorNumber);
            if (targetPlayer == null) return;

            float distance = Vector3.Distance(transform.position, targetPlayer.transform.position);
            if (distance > attackRange) return;

            // 공격 실행
            GetComponent<PhotonView>().RPC("AttackPlayerRPC", RpcTarget.All,
                PhotonNetwork.LocalPlayer.ActorNumber, targetActorNumber, attackDamage);

            lastAttackTime = Time.time;
        }

        /// <summary>
        /// 플레이어 공격 RPC
        /// </summary>
        [PunRPC]
        public void AttackPlayerRPC(int attackerActorNumber, int targetActorNumber, float damage)
        {
            // 데미지 처리
            var targetPlayer = GetPlayerByActorNumber(targetActorNumber);
            if (targetPlayer != null)
            {
                // TODO: 실제 데미지 처리 로직
                Debug.Log($"[NetworkPlayerInteraction] 플레이어 {attackerActorNumber}가 {targetActorNumber}에게 {damage} 데미지");

                // 이벤트 발생
                var damageEvent = new PunEvents.OnPlayerDamagedEvent
                {
                    attackerActorNumber = attackerActorNumber,
                    targetActorNumber = targetActorNumber,
                    damage = damage
                };
                EventHub.Instance.RaiseEvent(damageEvent);
            }
        }

        #endregion

        #region 아이템 상호작용

        /// <summary>
        /// 아이템 공유 (바닥에 버리기)
        /// </summary>
        public void ShareItem(int slotIndex, Vector3 dropPosition)
        {
            if (NetworkItemManager.Instance != null)
            {
                // 아이템을 바닥에 버리기
                NetworkItemManager.Instance.DropItem(slotIndex, dropPosition);

                Debug.Log($"[NetworkPlayerInteraction] 아이템 공유: 슬롯 {slotIndex}을 {dropPosition}에 버림");
            }
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// ActorNumber로 플레이어 찾기
        /// </summary>
        private PlayerStateMachine GetPlayerByActorNumber(int actorNumber)
        {
            var players = FindObjectsOfType<PlayerStateMachine>();
            return players.FirstOrDefault(p => p.PhotonView.Owner.ActorNumber == actorNumber);
        }

        /// <summary>
        /// 플레이어 간 거리 확인
        /// </summary>
        public bool IsPlayerInRange(int targetActorNumber, float range)
        {
            var targetPlayer = GetPlayerByActorNumber(targetActorNumber);
            if (targetPlayer == null) return false;

            float distance = Vector3.Distance(transform.position, targetPlayer.transform.position);
            return distance <= range;
        }

        #endregion

        #region 이벤트 처리

        /// <summary>
        /// 플레이어 스폰 이벤트 처리
        /// </summary>
        private void OnPlayerSpawned(OnPlayerSpawnedEvent evt)
        {
            // 상호작용 데이터 초기화
            playerInteractions[evt.ownerActorNumber] = new PlayerInteractionData
            {
                actorNumber = evt.ownerActorNumber,
                isInRange = false,
                lastInteractionTime = 0f
            };

            Debug.Log($"[NetworkPlayerInteraction] 플레이어 스폰 - ActorNumber: {evt.ownerActorNumber}");
        }

        /// <summary>
        /// 플레이어 퇴장 이벤트 처리
        /// </summary>
        private void OnPlayerLeftRoom(PunEvents.OnPlayerLeftRoomEvent evt)
        {
            // 상호작용 데이터 정리
            if (playerInteractions.ContainsKey(evt.actorNumber))
            {
                playerInteractions.Remove(evt.actorNumber);
            }

            Debug.Log($"[NetworkPlayerInteraction] 플레이어 퇴장 - ActorNumber: {evt.actorNumber}");
        }

        #endregion

        #region 데이터 구조체

        /// <summary>
        /// 플레이어 상호작용 데이터 구조체
        /// </summary>
        [System.Serializable]
        public struct PlayerInteractionData
        {
            public int actorNumber;
            public bool isInRange;
            public float lastInteractionTime;
        }

        #endregion

        #region INetworkOptimizable 구현

        /// <summary>
        /// 최적화 설정 적용
        /// </summary>
        public void ApplyOptimizationSettings(NetworkOptimizationSettings settings)
        {
            if (settings == null) return;

            isOptimized = true;

            performanceStats = new NetworkPerformanceStats
            {
                managerName = "NetworkPlayerInteraction",
                syncInterval = settings.positionSyncInterval,
                rpcCount = 0,
                batchCount = 0,
                networkTraffic = 0f,
                isOptimized = true,
                lastUpdateTime = Time.time
            };

            Debug.Log($"[NetworkPlayerInteraction] 최적화 설정 적용: 상호작용 최적화 완료");
        }

        /// <summary>
        /// 동기화 주기 설정
        /// </summary>
        public void SetSyncInterval(float interval)
        {
            performanceStats.syncInterval = Mathf.Max(0.1f, interval);
        }

        /// <summary>
        /// 보간 처리 설정
        /// </summary>
        public void SetInterpolation(bool enable, float speed = 6f)
        {
            // NetworkPlayerInteraction는 보간이 필요하지 않음
        }

        /// <summary>
        /// LOD 설정
        /// </summary>
        public void SetLOD(bool enable, float radius = 50f)
        {
            // NetworkPlayerInteraction는 LOD가 필요하지 않음
        }

        /// <summary>
        /// 배치 크기 설정
        /// </summary>
        public void SetBatchSize(int batchSize)
        {
            // NetworkPlayerInteraction는 배치가 필요하지 않음
        }

        /// <summary>
        /// 네트워크 품질 설정
        /// </summary>
        public void SetNetworkQuality(NetworkQuality quality)
        {
            switch (quality)
            {
                case NetworkQuality.Low:
                    performanceStats.syncInterval = 0.3f;
                    break;
                case NetworkQuality.Medium:
                    performanceStats.syncInterval = 0.1f;
                    break;
                case NetworkQuality.High:
                    performanceStats.syncInterval = 0.05f;
                    break;
            }
        }

        /// <summary>
        /// 최적화 상태 확인
        /// </summary>
        public bool IsOptimized()
        {
            return isOptimized;
        }

        /// <summary>
        /// 퍼포먼스 통계 가져오기
        /// </summary>
        public NetworkPerformanceStats GetPerformanceStats()
        {
            performanceStats.lastUpdateTime = Time.time;
            return performanceStats;
        }

        #endregion
    }
}
