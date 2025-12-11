using _Project.Script.Generic;
using _Project.Script.EventStruct;
using _Project.Script.Manager;
using _Project.Script.Interface;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;


namespace _Project.Script.Character.Network
{
    /// <summary>
    /// PUN2 기반 PvE (Player vs Environment) 시스템
    /// 몬스터 사냥, 보스 전투, 생존 요소를 실시간으로 동기화
    /// 돈스타브 핵심: 몬스터 사냥 + 생존 + 보스 전투
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    [DefaultExecutionOrder(-45)]
    public class NetworkPvESystem : MonoSingleton<NetworkPvESystem>, INetworkOptimizable, IPunObservable
    {
        [Header("PvE 설정")]
        [SerializeField] private float attackRange = 2f; // 공격 범위
        [SerializeField] private float detectionRange = 10f; // 몬스터 감지 범위
        [SerializeField] private float respawnTime = 30f; // 몬스터 리스폰 시간
        [SerializeField] private bool enablePvE = true; // PvE 활성화

        [Header("몬스터 프리팹")]
        [SerializeField] private GameObject[] monsterPrefabs; // 몬스터 프리팹들
        [SerializeField] private GameObject[] bossPrefabs; // 보스 프리팹들

        [Header("몬스터 설정")]
        [SerializeField] private float monsterBaseHealth = 50f; // 몬스터 기본 체력
        [SerializeField] private float monsterBaseDamage = 15f; // 몬스터 기본 데미지
        [SerializeField] private float monsterAttackCooldown = 2f; // 몬스터 공격 쿨다운
        [SerializeField] private float monsterMoveSpeed = 3f; // 몬스터 이동 속도

        [Header("보스 설정")]
        [SerializeField] private float bossBaseHealth = 500f; // 보스 기본 체력
        [SerializeField] private float bossBaseDamage = 30f; // 보스 기본 데미지
        [SerializeField] private float bossAttackCooldown = 3f; // 보스 공격 쿨다운
        [SerializeField] private float bossMoveSpeed = 2f; // 보스 이동 속도

        [Header("PvE 생존 요소")]
        [SerializeField] private float environmentalDamageRate = 0.1f; // 환경 피해율
        [SerializeField] private float monsterThreatLevel = 1.0f; // 몬스터 위험도
        [SerializeField] private float bossThreatLevel = 2.0f; // 보스 위험도

        // PvE 데이터 관리
        private Dictionary<int, MonsterData> activeMonsters = new Dictionary<int, MonsterData>();
        private Dictionary<int, BossData> activeBosses = new Dictionary<int, BossData>();
        private Dictionary<int, PlayerPvEData> playerPvEData = new Dictionary<int, PlayerPvEData>();

        // 최적화 관련 변수들
        private bool isOptimized = false;
        private NetworkPerformanceStats performanceStats;
        private Dictionary<int, GameObject> monsterGameObjects = new Dictionary<int, GameObject>();
        private Dictionary<int, GameObject> bossGameObjects = new Dictionary<int, GameObject>();
        private int nextMonsterId = 1;
        private int nextBossId = 1;

        // 이벤트 구독
        private void OnEnable()
        {
            EventHub.Instance.RegisterEvent<OnPlayerSpawnedEvent>(OnPlayerSpawned);
            EventHub.Instance.RegisterEvent<PunEvents.OnPlayerLeftRoomEvent>(OnPlayerLeftRoom);
        }

        private void OnDisable()
        {
            EventHub.Instance.UnregisterEvent<OnPlayerSpawnedEvent>(OnPlayerSpawned);
            EventHub.Instance.UnregisterEvent<PunEvents.OnPlayerLeftRoomEvent>(OnPlayerLeftRoom);
        }

        #region PvE 데이터 구조체

        /// <summary>
        /// 몬스터 데이터
        /// </summary>
        [System.Serializable]
        public struct MonsterData
        {
            public int id;
            public MonsterType type;
            public Vector3 position;
            public float health;
            public float maxHealth;
            public float damage;
            public float attackCooldown;
            public bool isAlive;
            public int targetPlayerId; // 추적 중인 플레이어
            public float lastAttackTime;
        }

        /// <summary>
        /// 보스 데이터
        /// </summary>
        [System.Serializable]
        public struct BossData
        {
            public int id;
            public BossType type;
            public Vector3 position;
            public float health;
            public float maxHealth;
            public float damage;
            public float attackCooldown;
            public bool isAlive;
            public int targetPlayerId; // 추적 중인 플레이어
            public float lastAttackTime;
            public BossPhase currentPhase; // 보스 페이즈
        }

        /// <summary>
        /// 플레이어 PvE 데이터
        /// </summary>
        [System.Serializable]
        public struct PlayerPvEData
        {
            public int actorNumber;
            public float lastAttackTime;
            public int targetMonsterId;
            public int targetBossId;
            public float survivalTime; // 생존 시간
            public int monstersKilled; // 처치한 몬스터 수
            public int bossesKilled; // 처치한 보스 수
        }

        /// <summary>
        /// 몬스터 타입
        /// TODO: ScriptableObject 또는 JSON 데이터로 전환 예정
        /// </summary>
        public enum MonsterType
        {
            Hound,      // 하운드 (돈스타브 기본 몬스터)
            Spider,     // 거미
            Pig,        // 돼지
            Rabbit,     // 토끼
            Bird,       // 새
            Fish,       // 물고기
            Bee,        // 벌
            Butterfly   // 나비
        }

        /// <summary>
        /// 보스 타입
        /// TODO: ScriptableObject 또는 JSON 데이터로 전환 예정
        /// </summary>
        public enum BossType
        {
            Deerclops,  // 디어클롭스 (겨울 보스)
            Bearger,    // 베어거 (가을 보스)
            Dragonfly,  // 드래곤플라이 (여름 보스)
            MooseGoose, // 무스구스 (봄 보스)
            AncientGuardian // 고대 수호자 (던전 보스)
        }

        /// <summary>
        /// 보스 페이즈
        /// TODO: ScriptableObject 또는 JSON 데이터로 전환 예정
        /// </summary>
        public enum BossPhase
        {
            Phase1,     // 1페이즈 (100-75%)
            Phase2,     // 2페이즈 (75-50%)
            Phase3,     // 3페이즈 (50-25%)
            Phase4      // 4페이즈 (25-0%)
        }

        #endregion

        #region PvE 시스템 초기화

        /// <summary>
        /// PvE 시스템 초기화
        /// </summary>
        public void InitializePvESystem()
        {
            if (!enablePvE) return;

            // 기본 몬스터 스폰
            SpawnDefaultMonsters();

            // 보스 스폰 (특정 조건에서)
            SpawnBossesByCondition();

            LogPvEInfo("PvE 시스템 초기화 완료");
        }

        /// <summary>
        /// 기본 몬스터들 스폰
        /// </summary>
        private void SpawnDefaultMonsters()
        {
            // 돈스타브 기본 몬스터들 스폰
            SpawnMonster(MonsterType.Rabbit, new Vector3(5, 0, 5));
            SpawnMonster(MonsterType.Rabbit, new Vector3(-5, 0, 5));
            SpawnMonster(MonsterType.Pig, new Vector3(10, 0, 0));
            SpawnMonster(MonsterType.Spider, new Vector3(-10, 0, -5));
        }

        /// <summary>
        /// 조건에 따른 보스 스폰
        /// </summary>
        private void SpawnBossesByCondition()
        {
            // 게임 진행 상황에 따라 보스 스폰
            // 예: 특정 날짜, 계절, 이벤트 발생 시
        }

        #endregion

        #region 몬스터 시스템

        /// <summary>
        /// 몬스터 스폰
        /// </summary>
        public void SpawnMonster(MonsterType type, Vector3 position)
        {
            if (!enablePvE) return;

            var monsterData = new MonsterData
            {
                id = nextMonsterId++,
                type = type,
                position = position,
                health = GetMonsterHealth(type),
                maxHealth = GetMonsterHealth(type),
                damage = GetMonsterDamage(type),
                attackCooldown = monsterAttackCooldown,
                isAlive = true,
                targetPlayerId = -1,
                lastAttackTime = 0f
            };

            activeMonsters[monsterData.id] = monsterData;

            // 몬스터 GameObject 생성
            CreateMonsterGameObject(monsterData);

            // 네트워크로 몬스터 스폰 알림
            GetComponent<PhotonView>().RPC("SpawnMonsterRPC", RpcTarget.All,
                monsterData.id, (int)monsterData.type, monsterData.position,
                monsterData.health, monsterData.maxHealth);

            LogPvEInfo($"몬스터 스폰: {type} at {position}");
        }

        /// <summary>
        /// 몬스터 공격
        /// </summary>
        public void AttackMonster(int monsterId, int playerActorNumber)
        {
            if (!activeMonsters.ContainsKey(monsterId)) return;

            var monster = activeMonsters[monsterId];
            if (!monster.isAlive) return;

            // 플레이어 공격력 계산
            float playerDamage = CalculatePlayerDamage(playerActorNumber);

            // 몬스터 체력 감소
            monster.health -= playerDamage;
            monster.health = Mathf.Max(0, monster.health);

            // 몬스터가 죽었는지 확인
            if (monster.health <= 0)
            {
                monster.isAlive = false;
                KillMonster(monsterId, playerActorNumber);
            }

            activeMonsters[monsterId] = monster;

            // 네트워크로 몬스터 피해 알림
            GetComponent<PhotonView>().RPC("MonsterTakeDamageRPC", RpcTarget.All,
                monsterId, monster.health, monster.maxHealth);

            LogPvEInfo($"몬스터 {monsterId} 피해: {playerDamage}, 남은 체력: {monster.health}");
        }

        /// <summary>
        /// 몬스터 처치
        /// </summary>
        private void KillMonster(int monsterId, int playerActorNumber)
        {
            if (!activeMonsters.ContainsKey(monsterId)) return;

            var monster = activeMonsters[monsterId];

            // 플레이어 처치 수 증가
            if (playerPvEData.ContainsKey(playerActorNumber))
            {
                var playerData = playerPvEData[playerActorNumber];
                playerData.monstersKilled++;
                playerPvEData[playerActorNumber] = playerData;
            }

            // 몬스터 드롭 아이템 생성
            CreateMonsterDrops(monster.type, monster.position);

            // 몬스터 제거
            activeMonsters.Remove(monsterId);

            // 네트워크로 몬스터 처치 알림
            GetComponent<PhotonView>().RPC("MonsterKilledRPC", RpcTarget.All,
                monsterId, playerActorNumber);

            LogPvEInfo($"몬스터 {monsterId} 처치 완료 by Player {playerActorNumber}");
        }

        /// <summary>
        /// 몬스터 GameObject 생성 (네트워크 동기화)
        /// </summary>
        private void CreateMonsterGameObject(MonsterData monsterData)
        {
            if (monsterPrefabs == null || monsterPrefabs.Length == 0) return;

            // 몬스터 타입에 따른 프리팹 선택
            GameObject monsterPrefab = GetMonsterPrefab(monsterData.type);
            if (monsterPrefab == null) return;

            // 네트워크 동기화를 위한 PhotonNetwork.Instantiate 사용
            GameObject monsterObj = PhotonNetwork.Instantiate(monsterPrefab.name, monsterData.position, Quaternion.identity);
            monsterObj.name = $"Monster_{monsterData.id}_{monsterData.type}";

            // 몬스터 GameObject 저장
            monsterGameObjects[monsterData.id] = monsterObj;

            // 몬스터 컴포넌트 설정
            var monsterComponent = monsterObj.GetComponent<NetworkMonster>();
            if (monsterComponent == null)
            {
                monsterComponent = monsterObj.AddComponent<NetworkMonster>();
            }
            // NetworkMonster는 자동으로 초기화됨

            LogPvEInfo($"몬스터 GameObject 생성 (네트워크): {monsterData.type} ID:{monsterData.id}");
        }

        /// <summary>
        /// 보스 GameObject 생성 (네트워크 동기화)
        /// </summary>
        private void CreateBossGameObject(BossData bossData)
        {
            if (bossPrefabs == null || bossPrefabs.Length == 0) return;

            // 보스 타입에 따른 프리팹 선택
            GameObject bossPrefab = GetBossPrefab(bossData.type);
            if (bossPrefab == null) return;

            // 네트워크 동기화를 위한 PhotonNetwork.Instantiate 사용
            GameObject bossObj = PhotonNetwork.Instantiate(bossPrefab.name, bossData.position, Quaternion.identity);
            bossObj.name = $"Boss_{bossData.id}_{bossData.type}";

            // 보스 GameObject 저장
            bossGameObjects[bossData.id] = bossObj;

            LogPvEInfo($"보스 GameObject 생성 (네트워크): {bossData.type} ID:{bossData.id}");
        }

        /// <summary>
        /// 몬스터 프리팹 가져오기
        /// </summary>
        private GameObject GetMonsterPrefab(MonsterType type)
        {
            if (monsterPrefabs == null || monsterPrefabs.Length == 0) return null;

            int typeIndex = (int)type;
            if (typeIndex < monsterPrefabs.Length)
            {
                return monsterPrefabs[typeIndex];
            }
            return monsterPrefabs[0]; // 기본 프리팹
        }

        /// <summary>
        /// 보스 프리팹 가져오기
        /// </summary>
        private GameObject GetBossPrefab(BossType type)
        {
            if (bossPrefabs == null || bossPrefabs.Length == 0) return null;

            int typeIndex = (int)type;
            if (typeIndex < bossPrefabs.Length)
            {
                return bossPrefabs[typeIndex];
            }
            return bossPrefabs[0]; // 기본 프리팹
        }

        /// <summary>
        /// 몬스터 드롭 아이템 생성
        /// </summary>
        private void CreateMonsterDrops(MonsterType type, Vector3 position)
        {
            // 몬스터 타입에 따른 드롭 아이템 생성
            // 예: 하운드 → 하운드 이빨, 거미 → 거미 실크 등
        }

        #endregion

        #region 보스 시스템

        /// <summary>
        /// 보스 스폰
        /// </summary>
        public void SpawnBoss(BossType type, Vector3 position)
        {
            if (!enablePvE) return;

            var bossData = new BossData
            {
                id = nextBossId++,
                type = type,
                position = position,
                health = GetBossHealth(type),
                maxHealth = GetBossHealth(type),
                damage = GetBossDamage(type),
                attackCooldown = bossAttackCooldown,
                isAlive = true,
                targetPlayerId = -1,
                lastAttackTime = 0f,
                currentPhase = BossPhase.Phase1
            };

            activeBosses[bossData.id] = bossData;

            // 보스 GameObject 생성 (네트워크 동기화)
            CreateBossGameObject(bossData);

            // 네트워크로 보스 스폰 알림
            GetComponent<PhotonView>().RPC("SpawnBossRPC", RpcTarget.All,
                bossData.id, (int)bossData.type, bossData.position,
                bossData.health, bossData.maxHealth);

            LogPvEInfo($"보스 스폰: {type} at {position}");
        }

        /// <summary>
        /// 보스 공격
        /// </summary>
        public void AttackBoss(int bossId, int playerActorNumber)
        {
            if (!activeBosses.ContainsKey(bossId)) return;

            var boss = activeBosses[bossId];
            if (!boss.isAlive) return;

            // 플레이어 공격력 계산
            float playerDamage = CalculatePlayerDamage(playerActorNumber);

            // 보스 체력 감소
            boss.health -= playerDamage;
            boss.health = Mathf.Max(0, boss.health);

            // 보스 페이즈 확인
            boss.currentPhase = GetBossPhase(boss.health, boss.maxHealth);

            // 보스가 죽었는지 확인
            if (boss.health <= 0)
            {
                boss.isAlive = false;
                KillBoss(bossId, playerActorNumber);
            }

            activeBosses[bossId] = boss;

            // 네트워크로 보스 피해 알림
            GetComponent<PhotonView>().RPC("BossTakeDamageRPC", RpcTarget.All,
                bossId, boss.health, boss.maxHealth, (int)boss.currentPhase);

            LogPvEInfo($"보스 {bossId} 피해: {playerDamage}, 남은 체력: {boss.health}, 페이즈: {boss.currentPhase}");
        }

        /// <summary>
        /// 보스 처치
        /// </summary>
        private void KillBoss(int bossId, int playerActorNumber)
        {
            if (!activeBosses.ContainsKey(bossId)) return;

            var boss = activeBosses[bossId];

            // 플레이어 처치 수 증가
            if (playerPvEData.ContainsKey(playerActorNumber))
            {
                var playerData = playerPvEData[playerActorNumber];
                playerData.bossesKilled++;
                playerPvEData[playerActorNumber] = playerData;
            }

            // 보스 드롭 아이템 생성
            CreateBossDrops(boss.type, boss.position);

            // 보스 제거
            activeBosses.Remove(bossId);

            // 네트워크로 보스 처치 알림
            GetComponent<PhotonView>().RPC("BossKilledRPC", RpcTarget.All,
                bossId, playerActorNumber);

            LogPvEInfo($"보스 {bossId} 처치 완료 by Player {playerActorNumber}");
        }

        /// <summary>
        /// 보스 드롭 아이템 생성
        /// </summary>
        private void CreateBossDrops(BossType type, Vector3 position)
        {
            // 보스 타입에 따른 드롭 아이템 생성
            // 예: 디어클롭스 → 거대한 눈, 베어거 → 털 등
        }

        #endregion

        #region 생존 시스템

        /// <summary>
        /// PvE 생존 요소 업데이트 (생존 시간, 처치 수 등)
        /// </summary>
        public void UpdatePvESurvivalElements(int playerActorNumber)
        {
            if (!playerPvEData.ContainsKey(playerActorNumber)) return;

            var playerData = playerPvEData[playerActorNumber];
            playerData.survivalTime += Time.deltaTime;
            playerPvEData[playerActorNumber] = playerData;
        }

        /// <summary>
        /// 환경 위험 요소 처리 (몬스터/보스 위험도)
        /// </summary>
        public void HandlePvEEnvironmentalHazards(int playerActorNumber, Vector3 position)
        {
            // 몬스터/보스 근처에서 위험도 증가
            // 환경 피해는 NetworkPlayerStats에서 처리
        }

        #endregion

        #region 유틸리티 메서드들

        /// <summary>
        /// 몬스터 체력 계산
        /// </summary>
        private float GetMonsterHealth(MonsterType type)
        {
            switch (type)
            {
                case MonsterType.Rabbit: return 10f;
                case MonsterType.Pig: return 30f;
                case MonsterType.Spider: return 20f;
                case MonsterType.Hound: return 40f;
                default: return monsterBaseHealth;
            }
        }

        /// <summary>
        /// 몬스터 데미지 계산
        /// </summary>
        private float GetMonsterDamage(MonsterType type)
        {
            switch (type)
            {
                case MonsterType.Rabbit: return 5f;
                case MonsterType.Pig: return 15f;
                case MonsterType.Spider: return 10f;
                case MonsterType.Hound: return 20f;
                default: return monsterBaseDamage;
            }
        }

        /// <summary>
        /// 보스 체력 계산
        /// </summary>
        private float GetBossHealth(BossType type)
        {
            switch (type)
            {
                case BossType.Deerclops: return 2000f;
                case BossType.Bearger: return 3000f;
                case BossType.Dragonfly: return 2500f;
                case BossType.MooseGoose: return 1500f;
                case BossType.AncientGuardian: return 5000f;
                default: return bossBaseHealth;
            }
        }

        /// <summary>
        /// 보스 데미지 계산
        /// </summary>
        private float GetBossDamage(BossType type)
        {
            switch (type)
            {
                case BossType.Deerclops: return 50f;
                case BossType.Bearger: return 60f;
                case BossType.Dragonfly: return 40f;
                case BossType.MooseGoose: return 35f;
                case BossType.AncientGuardian: return 80f;
                default: return bossBaseDamage;
            }
        }

        /// <summary>
        /// 플레이어 데미지 계산
        /// </summary>
        private float CalculatePlayerDamage(int playerActorNumber)
        {
            // 플레이어 스탯, 장비, 버프 등을 고려한 데미지 계산
            return 20f; // 기본 데미지
        }

        /// <summary>
        /// 보스 페이즈 계산
        /// </summary>
        private BossPhase GetBossPhase(float currentHealth, float maxHealth)
        {
            float healthPercentage = currentHealth / maxHealth;

            if (healthPercentage > 0.75f) return BossPhase.Phase1;
            else if (healthPercentage > 0.5f) return BossPhase.Phase2;
            else if (healthPercentage > 0.25f) return BossPhase.Phase3;
            else return BossPhase.Phase4;
        }

        #endregion

        #region RPC 메서드들

        [PunRPC]
        private void SpawnMonsterRPC(int monsterId, int type, Vector3 position, float health, float maxHealth)
        {
            // 몬스터 스폰 처리
            LogPvEInfo($"몬스터 스폰 RPC: {monsterId}");
        }

        [PunRPC]
        private void MonsterTakeDamageRPC(int monsterId, float health, float maxHealth)
        {
            // 몬스터 피해 처리
            LogPvEInfo($"몬스터 피해 RPC: {monsterId}, 체력: {health}/{maxHealth}");
        }

        [PunRPC]
        private void MonsterKilledRPC(int monsterId, int playerActorNumber)
        {
            // 몬스터 처치 처리
            LogPvEInfo($"몬스터 처치 RPC: {monsterId} by Player {playerActorNumber}");
        }

        [PunRPC]
        private void SpawnBossRPC(int bossId, int type, Vector3 position, float health, float maxHealth)
        {
            // 보스 스폰 처리
            LogPvEInfo($"보스 스폰 RPC: {bossId}");
        }

        [PunRPC]
        private void BossTakeDamageRPC(int bossId, float health, float maxHealth, int phase)
        {
            // 보스 피해 처리
            LogPvEInfo($"보스 피해 RPC: {bossId}, 체력: {health}/{maxHealth}, 페이즈: {phase}");
        }

        [PunRPC]
        private void BossKilledRPC(int bossId, int playerActorNumber)
        {
            // 보스 처치 처리
            LogPvEInfo($"보스 처치 RPC: {bossId} by Player {playerActorNumber}");
        }

        #endregion

        #region 이벤트 처리

        private void OnPlayerSpawned(OnPlayerSpawnedEvent evt)
        {
            // 플레이어 PvE 데이터 초기화
            playerPvEData[evt.ownerActorNumber] = new PlayerPvEData
            {
                actorNumber = evt.ownerActorNumber,
                lastAttackTime = 0f,
                targetMonsterId = -1,
                targetBossId = -1,
                survivalTime = 0f,
                monstersKilled = 0,
                bossesKilled = 0
            };

            LogPvEInfo($"플레이어 {evt.ownerActorNumber} PvE 데이터 초기화");
        }

        private void OnPlayerLeftRoom(PunEvents.OnPlayerLeftRoomEvent evt)
        {
            // 플레이어 PvE 데이터 제거
            if (playerPvEData.ContainsKey(evt.actorNumber))
            {
                playerPvEData.Remove(evt.actorNumber);
                LogPvEInfo($"플레이어 {evt.actorNumber} PvE 데이터 제거");
            }
        }

        #endregion

        #region IPunObservable 구현

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // 데이터 전송
                stream.SendNext(activeMonsters.Count);
                stream.SendNext(activeBosses.Count);
                stream.SendNext(playerPvEData.Count);
            }
            else
            {
                // 데이터 수신
                int monsterCount = (int)stream.ReceiveNext();
                int bossCount = (int)stream.ReceiveNext();
                int playerCount = (int)stream.ReceiveNext();
            }
        }

        #endregion

        #region 디버그 및 로깅

        /// <summary>
        /// PvE 로그 출력
        /// </summary>
        private void LogPvEInfo(string message)
        {
            Debug.Log($"[PvE시스템] {message}");
        }

        /// <summary>
        /// PvE 경고 로그
        /// </summary>
        private void LogPvEWarning(string message)
        {
            Debug.LogWarning($"[PvE시스템] {message}");
        }

        /// <summary>
        /// PvE 에러 로그
        /// </summary>
        private void LogPvEError(string message)
        {
            Debug.LogError($"[PvE시스템] {message}");
        }

        #endregion

        #region 디버그 UI

        private void OnGUI()
        {
            if (!enablePvE) return;

            GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 200));
            GUILayout.Label("=== PvE 시스템 ===");
            GUILayout.Label($"활성 몬스터: {activeMonsters.Count}");
            GUILayout.Label($"활성 보스: {activeBosses.Count}");
            GUILayout.Label($"플레이어 수: {playerPvEData.Count}");

            if (GUILayout.Button("몬스터 스폰"))
            {
                SpawnMonster(MonsterType.Hound, new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10)));
            }

            if (GUILayout.Button("보스 스폰"))
            {
                SpawnBoss(BossType.Deerclops, new Vector3(Random.Range(-20, 20), 0, Random.Range(-20, 20)));
            }

            GUILayout.EndArea();
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
                managerName = "NetworkPvESystem",
                syncInterval = settings.monsterSyncInterval,
                rpcCount = 0,
                batchCount = 0,
                networkTraffic = 0f,
                isOptimized = true,
                lastUpdateTime = Time.time
            };

            Debug.Log($"[NetworkPvESystem] 최적화 설정 적용: 몬스터 동기화 주기 {settings.monsterSyncInterval}초");
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
            // NetworkPvESystem는 보간이 필요하지 않음
        }

        /// <summary>
        /// LOD 설정
        /// </summary>
        public void SetLOD(bool enable, float radius = 50f)
        {
            // NetworkPvESystem는 LOD가 필요하지 않음
        }

        /// <summary>
        /// 배치 크기 설정
        /// </summary>
        public void SetBatchSize(int batchSize)
        {
            // NetworkPvESystem는 배치가 필요하지 않음
        }

        /// <summary>
        /// 네트워크 품질 설정
        /// </summary>
        public void SetNetworkQuality(NetworkQuality quality)
        {
            switch (quality)
            {
                case NetworkQuality.Low:
                    performanceStats.syncInterval = 0.5f;
                    break;
                case NetworkQuality.Medium:
                    performanceStats.syncInterval = 0.2f;
                    break;
                case NetworkQuality.High:
                    performanceStats.syncInterval = 0.1f;
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
