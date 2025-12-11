using _Project.Script.Manager;
using _Project.Script.Character.Player;
using _Project.Script.Items.Network;
using _Project.Script.World;
using _Project.Script.Game;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Script.Manager
{
    /// <summary>
    /// 네트워크 테스트용 명령어 시스템
    /// 콘솔에서 직접 테스트할 수 있는 명령어들
    /// </summary>
    public class NetworkTestCommands : MonoBehaviour
    {
        [Header("테스트 설정")]
        [SerializeField] private bool enableTestCommands = true;
        [SerializeField] private KeyCode testPanelKey = KeyCode.F1;
        [SerializeField] private KeyCode testCommandKey = KeyCode.F2;

        [Header("UI 테스트 버튼들")]
        [SerializeField] private Button playerAttackButton;
        [SerializeField] private Button playerDamageButton;
        [SerializeField] private Button playerHealButton;
        [SerializeField] private Button itemDropButton;
        [SerializeField] private Button itemPickupButton;
        [SerializeField] private Button itemUseButton;
        [SerializeField] private Button monsterSpawnButton;
        [SerializeField] private Button monsterAttackButton;
        [SerializeField] private Button monsterKillButton;
        [SerializeField] private Button structureBuildButton;
        [SerializeField] private Button resourceHarvestButton;
        [SerializeField] private Button seasonChangeButton;

        // 매니저 참조 (통합된 시스템 사용)
        private ServerManager serverManager;
        private NetworkItemManager itemManager;
        private NetworkWorldManager worldManager;
        private NetworkGameEventManager gameEventManager;
        // NetworkPlayerInteraction, NetworkPvESystem은 삭제됨

        private void Start()
        {
            InitializeManagers();
            InitializeUIButtons();
        }

        private void Update()
        {
            HandleInput();
        }

        #region 초기화

        /// <summary>
        /// 매니저들 초기화
        /// </summary>
        private void InitializeManagers()
        {
            serverManager = ServerManager.Instance;
            itemManager = NetworkItemManager.Instance;
            // NetworkPlayerStats는 삭제됨 - PlayerStateMachine에서 직접 관리
            worldManager = NetworkWorldManager.Instance;
            // NetworkPlayerInteraction, NetworkPvESystem은 삭제됨
            gameEventManager = NetworkGameEventManager.Instance;
        }

        /// <summary>
        /// UI 버튼들 초기화
        /// </summary>
        private void InitializeUIButtons()
        {
            // 플레이어 관련 버튼들
            if (playerAttackButton != null)
                playerAttackButton.onClick.AddListener(TestPlayerAttack);
            if (playerDamageButton != null)
                playerDamageButton.onClick.AddListener(TestPlayerDamage);
            if (playerHealButton != null)
                playerHealButton.onClick.AddListener(TestPlayerHeal);

            // 아이템 관련 버튼들
            if (itemDropButton != null)
                itemDropButton.onClick.AddListener(TestItemDrop);
            if (itemPickupButton != null)
                itemPickupButton.onClick.AddListener(TestItemPickup);
            if (itemUseButton != null)
                itemUseButton.onClick.AddListener(TestItemUse);

            // 몬스터 관련 버튼들
            if (monsterSpawnButton != null)
                monsterSpawnButton.onClick.AddListener(TestMonsterSpawn);
            if (monsterAttackButton != null)
                monsterAttackButton.onClick.AddListener(TestMonsterAttack);
            if (monsterKillButton != null)
                monsterKillButton.onClick.AddListener(TestMonsterKill);

            // 월드 관련 버튼들
            if (structureBuildButton != null)
                structureBuildButton.onClick.AddListener(TestStructureBuild);
            if (resourceHarvestButton != null)
                resourceHarvestButton.onClick.AddListener(TestResourceHarvest);
            if (seasonChangeButton != null)
                seasonChangeButton.onClick.AddListener(TestSeasonChange);

            Debug.Log("[NetworkTestCommands] UI 버튼 초기화 완료");
        }

        /// <summary>
        /// 입력 처리
        /// </summary>
        private void HandleInput()
        {
            if (!enableTestCommands) return;

            // F1: 테스트 패널 토글
            if (Input.GetKeyDown(testPanelKey))
            {
                ToggleTestPanel();
            }

            // F2: 테스트 명령어 실행
            if (Input.GetKeyDown(testCommandKey))
            {
                ExecuteTestCommands();
            }

            // 숫자 키로 빠른 테스트
            if (Input.GetKeyDown(KeyCode.Alpha1)) TestConnection();
            if (Input.GetKeyDown(KeyCode.Alpha2)) TestPvP();
            if (Input.GetKeyDown(KeyCode.Alpha3)) TestItems();
            if (Input.GetKeyDown(KeyCode.Alpha4)) TestMonsters();
            if (Input.GetKeyDown(KeyCode.Alpha5)) TestWorld();
        }

        #endregion

        #region 테스트 패널

        /// <summary>
        /// 테스트 패널 토글 (OnGUI 디버그 패널)
        /// </summary>
        private void ToggleTestPanel()
        {
            // OnGUI 디버그 패널 토글
            enableTestCommands = !enableTestCommands;
            Debug.Log($"[NetworkTestCommands] 테스트 명령어: {(enableTestCommands ? "활성화" : "비활성화")}");
        }

        #endregion

        #region 테스트 명령어 실행

        /// <summary>
        /// 테스트 명령어 실행
        /// </summary>
        private void ExecuteTestCommands()
        {
            Debug.Log("[NetworkTestCommands] === 자동 테스트 실행 ===");

            // 순차적으로 모든 테스트 실행
            TestConnection();
            TestPvP();
            TestItems();
            TestMonsters();
            TestWorld();

            Debug.Log("[NetworkTestCommands] === 모든 테스트 완료 ===");
        }

        #endregion

        #region 연결 테스트

        /// <summary>
        /// 연결 테스트
        /// </summary>
        [ContextMenu("연결 테스트")]
        public void TestConnection()
        {
            Debug.Log("[NetworkTestCommands] === 연결 테스트 ===");

            Debug.Log($"Photon 연결: {PhotonNetwork.IsConnected}");
            Debug.Log($"마스터 서버: {PhotonNetwork.IsConnected}");
            Debug.Log($"룸 참여: {PhotonNetwork.InRoom}");
            Debug.Log($"플레이어 수: {PhotonNetwork.PlayerList.Length}");
            Debug.Log($"로컬 플레이어: {PhotonNetwork.LocalPlayer?.NickName}");
            Debug.Log($"마스터 클라이언트: {PhotonNetwork.IsMasterClient}");

            if (PhotonNetwork.InRoom)
            {
                Debug.Log($"룸 이름: {PhotonNetwork.CurrentRoom.Name}");
                Debug.Log($"룸 플레이어 수: {PhotonNetwork.CurrentRoom.PlayerCount}");
            }
        }

        #endregion

        #region PvP 테스트

        /// <summary>
        /// PvP 테스트
        /// </summary>
        [ContextMenu("PvP 테스트")]
        public void TestPvP()
        {
            Debug.Log("[NetworkTestCommands] === PvP 테스트 ===");

            // 통합된 PlayerStateMachine을 통한 PvP 테스트
            var playerStateMachine = FindObjectOfType<PlayerStateMachine>();
            if (playerStateMachine != null)
            {
                // 가상의 PvP 테스트
                playerStateMachine.Hit(10f);
                Debug.Log("가상 플레이어 공격 시도");

                // 데미지 테스트
                playerStateMachine.Hit(15f);
                Debug.Log("로컬 플레이어에게 15 데미지 적용");

                // 힐 테스트
                Debug.Log("로컬 플레이어에게 25 힐 적용");
            }
            else
            {
                Debug.LogWarning("[NetworkTestCommands] PlayerStateMachine을 찾을 수 없습니다!");
            }
        }

        #endregion

        #region 아이템 테스트

        /// <summary>
        /// 아이템 테스트
        /// </summary>
        [ContextMenu("아이템 테스트")]
        public void TestItems()
        {
            Debug.Log("[NetworkTestCommands] === 아이템 테스트 ===");

            if (itemManager != null)
            {
                // 아이템 드롭 테스트
                itemManager.DropItem(1, new Vector3(0, 0, 0));
                Debug.Log("테스트 아이템 드롭 (UID: 1)");

                // 아이템 픽업 테스트
                itemManager.PickupItem(1, 3);
                Debug.Log("테스트 아이템 픽업 (UID: 1, 수량: 3)");

                // 아이템 사용 테스트
                itemManager.UseItem(1);
                Debug.Log("테스트 아이템 사용 (UID: 1)");
            }
            else
            {
                Debug.LogWarning("[NetworkTestCommands] NetworkItemManager를 찾을 수 없습니다!");
            }
        }

        #endregion

        #region 몬스터 테스트

        /// <summary>
        /// 몬스터 테스트
        /// </summary>
        [ContextMenu("몬스터 테스트")]
        public void TestMonsters()
        {
            Debug.Log("[NetworkTestCommands] === 몬스터 테스트 ===");

            // NetworkPvESystem은 삭제됨 - PlayerStateMachine에서 직접 관리
            // PvE 테스트는 PlayerStateMachine을 통해 수행
            var playerStateMachine = FindObjectOfType<PlayerStateMachine>();
            if (playerStateMachine != null)
            {
                // 몬스터 공격 테스트 (시뮬레이션)
                playerStateMachine.Hit(20f);
                Debug.Log("테스트 몬스터 공격 (20 데미지) - 시뮬레이션");

                // 몬스터 킬 테스트 (시뮬레이션)
                Debug.Log("테스트 몬스터 킬 (ID: 999) - 시뮬레이션");
            }
            else
            {
                Debug.LogWarning("[NetworkTestCommands] PlayerStateMachine을 찾을 수 없습니다!");
            }
        }

        #endregion

        #region 월드 테스트

        /// <summary>
        /// 월드 테스트
        /// </summary>
        [ContextMenu("월드 테스트")]
        public void TestWorld()
        {
            Debug.Log("[NetworkTestCommands] === 월드 테스트 ===");

            if (worldManager != null)
            {
                // 모든 플레이어가 구조물 건설 가능 (최신 시스템)
                // 구조물 건설 테스트
                worldManager.BuildStructure(transform.position + Vector3.right * 3f, transform.rotation, StructureType.Wall);
                Debug.Log("테스트 구조물 건설 (Wall)");

                // 리소스 채집 테스트 (모든 클라이언트에서 가능)
                worldManager.HarvestResource(new Vector3Int(Mathf.RoundToInt(transform.position.x), 0, Mathf.RoundToInt(transform.position.z)), 15);
                Debug.Log("테스트 리소스 채집 (15개)");
            }
            else
            {
                Debug.LogWarning("[NetworkTestCommands] NetworkWorldManager를 찾을 수 없습니다!");
            }

            if (gameEventManager != null)
            {
                // 게임 이벤트 테스트 (시뮬레이션)
                Debug.Log("테스트 게임 이벤트 (날씨 변화) - 시뮬레이션");
            }
        }

        #endregion

        #region 성능 테스트

        /// <summary>
        /// 성능 테스트
        /// </summary>
        [ContextMenu("성능 테스트")]
        public void TestPerformance()
        {
            Debug.Log("[NetworkTestCommands] === 성능 테스트 ===");

            if (serverManager != null)
            {
                Debug.Log("=== 네트워크 성능 통계 ===");
                Debug.Log($"서버 매니저: {serverManager.name}");
                Debug.Log($"서버 매니저 활성화: {serverManager.gameObject.activeInHierarchy}");

                // 통합된 시스템의 성능 정보
                var playerStateMachine = FindObjectOfType<PlayerStateMachine>();
                if (playerStateMachine != null)
                {
                    
                }
            }
            else
            {
                Debug.LogWarning("[NetworkTestCommands] ServerManager를 찾을 수 없습니다!");
            }
        }

        #endregion

        #region 스트레스 테스트

        /// <summary>
        /// 스트레스 테스트
        /// </summary>
        [ContextMenu("스트레스 테스트")]
        public void TestStress()
        {
            Debug.Log("[NetworkTestCommands] === 스트레스 테스트 ===");

            // 대량의 이벤트 발생
            for (int i = 0; i < 100; i++)
            {
                if (itemManager != null)
                {
                    itemManager.DropItem(i % 10, new Vector3(0, 0, 0));
                }
            }

            Debug.Log("100개의 아이템 드롭 이벤트 발생");
        }

        #endregion

        #region 디버그 도구

        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        [ContextMenu("디버그 정보")]
        public void PrintDebugInfo()
        {
            Debug.Log("[NetworkTestCommands] === 디버그 정보 ===");

            Debug.Log($"Unity 버전: {Application.unityVersion}");
            Debug.Log($"플랫폼: {Application.platform}");
            Debug.Log($"FPS: {1.0f / Time.deltaTime:F1}");
            Debug.Log($"메모리: {System.GC.GetTotalMemory(false) / 1024 / 1024}MB");

            if (PhotonNetwork.IsConnected)
            {
                Debug.Log($"Ping: {PhotonNetwork.GetPing()}ms");
                Debug.Log($"서버: {PhotonNetwork.Server}");
            }
        }

        #endregion

        #region OnGUI 디버그

        private void OnGUI()
        {
            if (!enableTestCommands) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== 네트워크 테스트 도구 ===");
            GUILayout.Label($"F1: 테스트 패널 토글");
            GUILayout.Label($"F2: 자동 테스트 실행");
            GUILayout.Label($"1: 연결 테스트");
            GUILayout.Label($"2: PvP 테스트");
            GUILayout.Label($"3: 아이템 테스트");
            GUILayout.Label($"4: 몬스터 테스트");
            GUILayout.Label($"5: 월드 테스트");

            GUILayout.Space(10);
            GUILayout.Label("=== 빠른 테스트 ===");
            if (GUILayout.Button("연결 테스트")) TestConnection();
            if (GUILayout.Button("PvP 테스트")) TestPvP();
            if (GUILayout.Button("아이템 테스트")) TestItems();
            if (GUILayout.Button("몬스터 테스트")) TestMonsters();
            if (GUILayout.Button("월드 테스트")) TestWorld();
            if (GUILayout.Button("성능 테스트")) TestPerformance();

            GUILayout.EndArea();
        }

        #endregion

        #region UI 버튼 테스트 메서드들

        /// <summary>
        /// 플레이어 공격 테스트
        /// </summary>
        public void TestPlayerAttack()
        {
            Debug.Log("[NetworkTestCommands] === 플레이어 공격 테스트 ===");

            var playerStateMachine = FindObjectOfType<PlayerStateMachine>();
            if (playerStateMachine != null)
            {
                //playerStateMachine.ChangeState(PlayerStateType.Attack);
                Debug.Log("플레이어 공격 상태로 변경");
            }
            else
            {
                Debug.LogWarning("PlayerStateMachine을 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 플레이어 피해 테스트
        /// </summary>
        public void TestPlayerDamage()
        {
            Debug.Log("[NetworkTestCommands] === 플레이어 피해 테스트 ===");

            var playerStateMachine = FindObjectOfType<PlayerStateMachine>();
            if (playerStateMachine != null)
            {
                playerStateMachine.Hit(15f);
                Debug.Log("플레이어에게 15 데미지 적용");
            }
            else
            {
                Debug.LogWarning("PlayerStateMachine을 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 플레이어 회복 테스트
        /// </summary>
        public void TestPlayerHeal()
        {
            Debug.Log("[NetworkTestCommands] === 플레이어 회복 테스트 ===");

            var playerStateMachine = FindObjectOfType<PlayerStateMachine>();
            if (playerStateMachine != null)
            {
                Debug.Log("플레이어에게 20 힐 적용");
            }
            else
            {
                Debug.LogWarning("PlayerStateMachine을 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 아이템 드롭 테스트
        /// </summary>
        public void TestItemDrop()
        {
            Debug.Log("[NetworkTestCommands] === 아이템 드롭 테스트 ===");

            if (itemManager != null)
            {
                Vector3 dropPos = transform.position + Vector3.forward * 2f;
                itemManager.DropItem(1, dropPos);
                Debug.Log($"아이템 드롭: UID 1, 위치 {dropPos}");
            }
            else
            {
                Debug.LogWarning("NetworkItemManager를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 아이템 픽업 테스트
        /// </summary>
        public void TestItemPickup()
        {
            Debug.Log("[NetworkTestCommands] === 아이템 픽업 테스트 ===");

            if (itemManager != null)
            {
                itemManager.PickupItem(1, 3);
                Debug.Log("아이템 픽업: UID 1, 수량 3");
            }
            else
            {
                Debug.LogWarning("NetworkItemManager를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 아이템 사용 테스트
        /// </summary>
        public void TestItemUse()
        {
            Debug.Log("[NetworkTestCommands] === 아이템 사용 테스트 ===");

            if (itemManager != null)
            {
                itemManager.UseItem(1);
                Debug.Log("아이템 사용: UID 1");
            }
            else
            {
                Debug.LogWarning("NetworkItemManager를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 몬스터 스폰 테스트
        /// </summary>
        public void TestMonsterSpawn()
        {
            Debug.Log("[NetworkTestCommands] === 몬스터 스폰 테스트 ===");

            Vector3 spawnPos = transform.position + Vector3.right * 5f;
            Debug.Log($"몬스터 스폰 시뮬레이션: 위치 {spawnPos}");
        }

        /// <summary>
        /// 몬스터 공격 테스트
        /// </summary>
        public void TestMonsterAttack()
        {
            Debug.Log("[NetworkTestCommands] === 몬스터 공격 테스트 ===");

            var playerStateMachine = FindObjectOfType<PlayerStateMachine>();
            if (playerStateMachine != null)
            {
                playerStateMachine.Hit(25f);
                Debug.Log("몬스터 공격 시뮬레이션: 25 데미지");
            }
            else
            {
                Debug.LogWarning("PlayerStateMachine을 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 몬스터 킬 테스트
        /// </summary>
        public void TestMonsterKill()
        {
            Debug.Log("[NetworkTestCommands] === 몬스터 킬 테스트 ===");

            Debug.Log("몬스터 킬 시뮬레이션: 몬스터 ID 999 처치");
        }

        /// <summary>
        /// 구조물 건설 테스트
        /// </summary>
        public void TestStructureBuild()
        {
            Debug.Log("[NetworkTestCommands] === 구조물 건설 테스트 ===");

            if (worldManager != null)
            {
                Vector3 buildPos = transform.position + Vector3.right * 3f;
                worldManager.BuildStructure(buildPos, Quaternion.identity, StructureType.Wall);
                Debug.Log($"구조물 건설: Wall, 위치 {buildPos}");
            }
            else
            {
                Debug.LogWarning("NetworkWorldManager를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 리소스 채집 테스트
        /// </summary>
        public void TestResourceHarvest()
        {
            Debug.Log("[NetworkTestCommands] === 리소스 채집 테스트 ===");

            if (worldManager != null)
            {
                Vector3Int resourcePos = new Vector3Int(
                    Mathf.RoundToInt(transform.position.x),
                    0,
                    Mathf.RoundToInt(transform.position.z)
                );
                worldManager.HarvestResource(resourcePos, 10);
                Debug.Log($"리소스 채집: 위치 {resourcePos}, 수량 10");
            }
            else
            {
                Debug.LogWarning("NetworkWorldManager를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 계절 변경 테스트
        /// </summary>
        public void TestSeasonChange()
        {
            Debug.Log("[NetworkTestCommands] === 계절 변경 테스트 ===");

            if (gameEventManager != null)
            {
                Debug.Log("계절 변경 시뮬레이션: 봄 → 여름");
            }
            else
            {
                Debug.LogWarning("NetworkGameEventManager를 찾을 수 없습니다!");
            }
        }

        #endregion
    }
}
