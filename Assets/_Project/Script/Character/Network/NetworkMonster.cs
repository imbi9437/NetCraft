using _Project.Script.Character.Network;
using _Project.Script.Interface;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

namespace _Project.Script.Character.Network
{
    /// <summary>
    /// 네트워크 몬스터 프리팹 전용 스크립트
    /// NetworkPvESystem과 연동하여 몬스터 AI 및 동기화
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(Collider))]
    public class NetworkMonster : MonoBehaviour, IPunObservable, IHitAble
    {
        [Header("몬스터 설정")]
        [SerializeField] private int monsterType = 0; // 0: Spider, 1: Hound, 2: Boss
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float damage = 20f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float moveSpeed = 3f;

        [Header("AI 설정")]
        [SerializeField] private float patrolRadius = 5f;
        [SerializeField] private float chaseSpeed = 5f;
        [SerializeField] private float attackSpeed = 1f;

        [Header("프리팹 전용 설정")]
        [SerializeField] private string monsterName = "Monster";
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private string interactionText = "몬스터";

        // 네트워크 동기화용
        private PhotonView photonView;
        private NetworkPvESystem pveManager;

        // AI 상태
        private MonsterState currentState = MonsterState.Idle;
        private Vector3 spawnPosition;
        private Vector3 targetPosition;
        private int targetPlayerId = -1;
        private float lastAttackTime = 0f;
        private float lastStateChangeTime = 0f;

        // 컴포넌트 참조
        private Renderer monsterRenderer;
        private Collider monsterCollider;
        private Rigidbody monsterRigidbody;

        // 플레이어 추적
        private List<GameObject> nearbyPlayers = new List<GameObject>();

        private void Awake()
        {
            photonView = GetComponent<PhotonView>();
            pveManager = NetworkPvESystem.Instance;
            monsterRenderer = GetComponent<Renderer>();
            monsterCollider = GetComponent<Collider>();
            monsterRigidbody = GetComponent<Rigidbody>();

            // 초기 위치 저장
            spawnPosition = transform.position;
        }

        private void Start()
        {
            // 몬스터 정보를 PvE 매니저에 등록
            if (pveManager != null && photonView.IsMine)
            {
                RegisterToPvEManager();
            }

            // 몬스터 시각적 설정
            SetupMonsterVisual();
        }

        private void Update()
        {
            // AI 업데이트 (로컬 플레이어만)
            if (photonView.IsMine)
            {
                UpdateAI();
            }
        }

        private void OnDestroy()
        {
            // 몬스터 정보를 PvE 매니저에서 제거
            if (pveManager != null && photonView.IsMine)
            {
                UnregisterFromPvEManager();
            }
        }

        #region PUN2 네트워크 동기화

        /// <summary>
        /// PUN2 네트워크 동기화 (몬스터 데이터)
        /// </summary>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // 데이터 전송 (로컬 플레이어)
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
                stream.SendNext(currentHealth);
                stream.SendNext((int)currentState);
                stream.SendNext(targetPlayerId);
            }
            else
            {
                // 데이터 수신 (원격 플레이어)
                transform.position = (Vector3)stream.ReceiveNext();
                transform.rotation = (Quaternion)stream.ReceiveNext();
                currentHealth = (float)stream.ReceiveNext();
                currentState = (MonsterState)stream.ReceiveNext();
                targetPlayerId = (int)stream.ReceiveNext();

                // 몬스터 상태 업데이트
                UpdateMonsterVisual();
            }
        }

        #endregion

        #region PvE 매니저 연동

        /// <summary>
        /// PvE 매니저에 몬스터 등록
        /// </summary>
        private void RegisterToPvEManager()
        {
            if (pveManager != null)
            {
                // 몬스터 데이터 생성
                var monsterData = new NetworkPvESystem.MonsterData
                {
                    id = photonView.ViewID,
                    type = (NetworkPvESystem.MonsterType)monsterType,
                    position = transform.position,
                    health = currentHealth,
                    maxHealth = maxHealth,
                    damage = damage,
                    attackCooldown = attackCooldown,
                    isAlive = true,
                    targetPlayerId = -1,
                    lastAttackTime = 0f
                };

                // 몬스터 등록 (나중에 구현)
                Debug.Log($"[NetworkMonster] 몬스터 등록: {monsterType}");
            }
        }

        /// <summary>
        /// PvE 매니저에서 몬스터 제거
        /// </summary>
        private void UnregisterFromPvEManager()
        {
            if (pveManager != null)
            {
                // 몬스터 제거 (나중에 구현)
                Debug.Log($"[NetworkMonster] 몬스터 제거: {monsterType}");
            }
        }

        #endregion

        #region 몬스터 AI

        /// <summary>
        /// AI 업데이트
        /// </summary>
        private void UpdateAI()
        {
            // 플레이어 감지
            DetectPlayers();

            // 상태별 행동
            switch (currentState)
            {
                case MonsterState.Idle:
                    UpdateIdleState();
                    break;
                case MonsterState.Patrol:
                    UpdatePatrolState();
                    break;
                case MonsterState.Chase:
                    UpdateChaseState();
                    break;
                case MonsterState.Attack:
                    UpdateAttackState();
                    break;
                case MonsterState.Dead:
                    UpdateDeadState();
                    break;
            }
        }

        /// <summary>
        /// 플레이어 감지
        /// </summary>
        private void DetectPlayers()
        {
            nearbyPlayers.Clear();
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange);

            foreach (var collider in colliders)
            {
                var player = collider.GetComponent<Player.PlayerStateMachine>();
                if (player != null)
                {
                    nearbyPlayers.Add(collider.gameObject);
                }
            }
        }

        /// <summary>
        /// Idle 상태 업데이트
        /// </summary>
        private void UpdateIdleState()
        {
            if (nearbyPlayers.Count > 0)
            {
                // 플레이어 발견 시 추적 시작
                targetPlayerId = nearbyPlayers[0].GetComponent<PhotonView>().ViewID;
                ChangeState(MonsterState.Chase);
            }
            else if (Time.time - lastStateChangeTime > 5f)
            {
                // 5초 후 순찰 시작
                ChangeState(MonsterState.Patrol);
            }
        }

        /// <summary>
        /// Patrol 상태 업데이트
        /// </summary>
        private void UpdatePatrolState()
        {
            if (nearbyPlayers.Count > 0)
            {
                // 플레이어 발견 시 추적 시작
                targetPlayerId = nearbyPlayers[0].GetComponent<PhotonView>().ViewID;
                ChangeState(MonsterState.Chase);
                return;
            }

            // 순찰 이동
            if (Vector3.Distance(transform.position, targetPosition) < 1f)
            {
                // 새로운 순찰 지점 설정
                targetPosition = spawnPosition + Random.insideUnitSphere * patrolRadius;
                targetPosition.y = spawnPosition.y;
            }

            MoveTowards(targetPosition);
        }

        /// <summary>
        /// Chase 상태 업데이트
        /// </summary>
        private void UpdateChaseState()
        {
            if (nearbyPlayers.Count == 0)
            {
                // 플레이어가 범위를 벗어나면 Idle로 전환
                targetPlayerId = -1;
                ChangeState(MonsterState.Idle);
                return;
            }

            // 가장 가까운 플레이어 추적
            GameObject closestPlayer = GetClosestPlayer();
            if (closestPlayer != null)
            {
                targetPosition = closestPlayer.transform.position;

                // 공격 범위 내에 있으면 공격
                if (Vector3.Distance(transform.position, targetPosition) <= attackRange)
                {
                    ChangeState(MonsterState.Attack);
                    return;
                }

                // 추적 이동
                MoveTowards(targetPosition);
            }
        }

        /// <summary>
        /// Attack 상태 업데이트
        /// </summary>
        private void UpdateAttackState()
        {
            if (nearbyPlayers.Count == 0)
            {
                // 플레이어가 범위를 벗어나면 Chase로 전환
                ChangeState(MonsterState.Chase);
                return;
            }

            // 공격 쿨다운 확인
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                AttackPlayer();
                lastAttackTime = Time.time;
            }

            // 공격 후 Chase로 전환
            if (Time.time - lastAttackTime > 1f)
            {
                ChangeState(MonsterState.Chase);
            }
        }

        /// <summary>
        /// Dead 상태 업데이트
        /// </summary>
        private void UpdateDeadState()
        {
            // 몬스터 제거 (나중에 구현)
            // 예: 일정 시간 후 제거, 재생성 등
        }

        #endregion

        #region 몬스터 행동

        /// <summary>
        /// 목표 지점으로 이동
        /// </summary>
        private void MoveTowards(Vector3 target)
        {
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0; // Y축 이동 제거

            if (direction.magnitude > 0.1f)
            {
                transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
                transform.rotation = Quaternion.Lerp(transform.rotation,
                    Quaternion.LookRotation(direction), Time.deltaTime * 5f);
            }
        }

        /// <summary>
        /// 플레이어 공격
        /// </summary>
        private void AttackPlayer()
        {
            GameObject targetPlayer = GetClosestPlayer();
            if (targetPlayer != null)
            {
                var player = targetPlayer.GetComponent<_Project.Script.Character.Player.PlayerStateMachine>();
                if (player != null)
                {
                    // 공격 실행
                    GetComponent<PhotonView>().RPC("AttackPlayerRPC", RpcTarget.All,
                        photonView.ViewID, player.GetComponent<PhotonView>().ViewID, damage);
                }
            }
        }

        /// <summary>
        /// 플레이어 공격 RPC
        /// </summary>
        [PunRPC]
        public void AttackPlayerRPC(int monsterId, int playerId, float attackDamage)
        {
            Debug.Log($"[NetworkMonster] 몬스터 {monsterId}가 플레이어 {playerId} 공격: {attackDamage} 데미지");
        }

        /// <summary>
        /// 가장 가까운 플레이어 찾기
        /// </summary>
        private GameObject GetClosestPlayer()
        {
            GameObject closest = null;
            float closestDistance = float.MaxValue;

            foreach (var player in nearbyPlayers)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < closestDistance)
                {
                    closest = player;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        #endregion

        #region 상태 관리

        /// <summary>
        /// 상태 변경
        /// </summary>
        private void ChangeState(MonsterState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            lastStateChangeTime = Time.time;

            Debug.Log($"[NetworkMonster] 상태 변경: {currentState}");
        }

        /// <summary>
        /// 몬스터 시각적 설정
        /// </summary>
        private void SetupMonsterVisual()
        {
            // 몬스터 타입에 따른 시각적 설정 (나중에 구현)
            // 예: 색상, 크기, 모델 등
        }

        /// <summary>
        /// 몬스터 시각적 업데이트
        /// </summary>
        private void UpdateMonsterVisual()
        {
            // 상태에 따른 시각적 변화 (나중에 추가)
            // 예: 추적 중일 때 색상 변경, 공격 시 이펙트 등
        }

        #endregion

        #region IHitAble 인터페이스 구현

        /// <summary>
        /// 데미지 받기
        /// </summary>
        public void TakeDamage(float damage, int attackerActorNumber)
        {
            if (currentState == MonsterState.Dead) return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);

            if (currentHealth <= 0)
            {
                Die();
            }

            Debug.Log($"[NetworkMonster] 몬스터 데미지: {damage}, 남은 체력: {currentHealth}");
        }

        /// <summary>
        /// Hit 메서드 (IHitAble 인터페이스)
        /// </summary>
        public void Hit(float damage)
        {
            TakeDamage(damage, -1);
        }

        /// <summary>
        /// 몬스터 사망
        /// </summary>
        private void Die()
        {
            currentState = MonsterState.Dead;
            currentHealth = 0f;

            // 사망 이펙트 (나중에 추가)
            // Instantiate(deathEffect, transform.position, transform.rotation);

            Debug.Log($"[NetworkMonster] 몬스터 사망: {monsterType}");
        }

        #endregion

        #region 공개 API
        // 사용하지 않는 getter 함수들 제거됨
        #endregion

        #region 디버그

        private void OnDrawGizmosSelected()
        {
            // 감지 범위 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // 공격 범위 표시
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // 순찰 범위 표시
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(spawnPosition, patrolRadius);

            // 상태 표시
            Gizmos.color = currentState == MonsterState.Dead ? Color.red : Color.green;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        }

        #endregion
    }

    /// <summary>
    /// 몬스터 상태 열거형
    /// </summary>
    public enum MonsterState
    {
        Idle,       // 대기
        Patrol,     // 순찰
        Chase,      // 추적
        Attack,     // 공격
        Dead        // 사망
    }
}