using _Project.Script.Generic;
using _Project.Script.Character.Network;
using _Project.Script.Character.Player;
using _Project.Script.Items.Network;
using _Project.Script.World;
using _Project.Script.Game;
using _Project.Script.Interface;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;

namespace _Project.Script.Manager
{
    /// <summary>
    /// 서버 관리자용 통합 매니저
    /// 기존 시스템과 멀티플레이 시스템을 연동
    /// </summary>
    public class ServerManager : MonoSingleton<ServerManager>
    {
        [Header("서버 설정")]
        [SerializeField] private bool isServerMode = true;
        [SerializeField] private bool enableDebugLog = true;

        [Header("중앙화된 최적화 설정")]
        [SerializeField] private NetworkOptimizationSettings optimizationSettings;
        [SerializeField] private bool autoApplyOptimization = true; // 자동 최적화 적용

        [Header("매니저 참조 (통합된 시스템)")]
        private NetworkItemManager itemManager;
        // NetworkPlayerStats는 삭제됨 - PlayerStateMachine에서 직접 관리
        private NetworkWorldManager worldManager;
        private NetworkPlayerInteraction interactionManager;
        private NetworkGameEventManager gameEventManager;
        private NetworkPvESystem pveManager;

        [Header("최적화 매니저 목록")]
        private List<INetworkOptimizable> networkManagers = new List<INetworkOptimizable>();

        [Header("플레이어 데이터")]
        // TODO: Firebase 연동 시 제거 예정 - 현재는 임시 메모리 저장
        private Dictionary<int, Generic.PlayerData> serverPlayerData = new Dictionary<int, Generic.PlayerData>();

        [Header("퍼포먼스 최적화 데이터")]
        // 1. 서버 권한 모델 (MasterClient 전담)
        private Dictionary<int, MonsterData> serverMonsterData = new Dictionary<int, MonsterData>();
        private Dictionary<int, Vector3> lastPlayerPositions = new Dictionary<int, Vector3>();
        private Dictionary<int, float> lastSyncTimes = new Dictionary<int, float>();

        // 네트워크 품질 자동 조절 쿨다운
        private float lastQualityAdjustTime = 0f;
        private float qualityAdjustCooldown = 5f; // 5초마다 한 번만 조절

        // 2. 데이터 전송 최소화
        private Dictionary<int, bool> playerDataChanged = new Dictionary<int, bool>();
        private Dictionary<int, bool> monsterDataChanged = new Dictionary<int, bool>();

        // 3. 패킷 묶기 (Batching)
        private List<MonsterData> pendingMonsterUpdates = new List<MonsterData>();
        private float lastMonsterBatchTime = 0f;

        // 4. LOD & 근처만 업데이트
        private Dictionary<int, List<int>> nearbyMonsters = new Dictionary<int, List<int>>();
        private Dictionary<int, float> playerLastLODUpdate = new Dictionary<int, float>();

        #region 초기화

        protected override void Awake()
        {
            base.Awake();

            if (isServerMode)
            {
                InitializeServerManagers();
                LogServerInfo("서버 관리자 초기화 완료");
            }
        }

        private void InitializeServerManagers()
        {
            // 매니저들 초기화 (통합된 시스템)
            itemManager = NetworkItemManager.Instance;
            // NetworkPlayerStats는 삭제됨 - PlayerStateMachine에서 직접 관리
            worldManager = NetworkWorldManager.Instance;
            interactionManager = NetworkPlayerInteraction.Instance;
            gameEventManager = NetworkGameEventManager.Instance;
            pveManager = NetworkPvESystem.Instance;

            // 최적화 매니저 목록 등록
            RegisterNetworkManagers();

            // 이벤트 구독
            PhotonNetwork.AddCallbackTarget(this);

            // 자동 최적화 적용
            if (autoApplyOptimization && optimizationSettings != null)
            {
                ApplyOptimizationToAllManagers();
            }
        }

        private void Update()
        {
            if (!isServerMode) return;

            // 퍼포먼스 최적화 시스템 실행
            UpdateServerAuthority();
            UpdateDataMinimization();
            UpdateLODSystem();
            UpdateInterpolation();

            // 자동 네트워크 품질 조절 (쿨다운 적용)
            if (optimizationSettings != null && optimizationSettings.enableAutoQuality)
            {
                if (Time.time - lastQualityAdjustTime >= qualityAdjustCooldown)
                {
                    AutoAdjustNetworkQuality();
                    lastQualityAdjustTime = Time.time;
                }
            }
        }

        #endregion

        #region 중앙화된 최적화 시스템

        /// <summary>
        /// 네트워크 매니저들 등록
        /// </summary>
        private void RegisterNetworkManagers()
        {
            networkManagers.Clear();

            // 모든 네트워크 매니저를 INetworkOptimizable로 캐스팅하여 등록
            if (itemManager is INetworkOptimizable itemOpt) networkManagers.Add(itemOpt);
            // NetworkPlayerStats는 삭제됨 - PlayerStateMachine에서 직접 관리
            if (worldManager is INetworkOptimizable worldOpt) networkManagers.Add(worldOpt);
            if (interactionManager is INetworkOptimizable interactionOpt) networkManagers.Add(interactionOpt);
            if (gameEventManager is INetworkOptimizable gameEventOpt) networkManagers.Add(gameEventOpt);
            if (pveManager is INetworkOptimizable pveOpt) networkManagers.Add(pveOpt);

            LogServerInfo($"등록된 네트워크 매니저: {networkManagers.Count}개");
        }

        /// <summary>
        /// 모든 네트워크 매니저에 최적화 적용
        /// </summary>
        public void ApplyOptimizationToAllManagers()
        {
            if (optimizationSettings == null)
            {
                LogServerWarning("최적화 설정이 없습니다!");
                return;
            }

            LogServerInfo("=== 중앙화된 최적화 적용 시작 ===");

            foreach (var manager in networkManagers)
            {
                try
                {
                    manager.ApplyOptimizationSettings(optimizationSettings);
                    LogServerInfo($"최적화 적용 완료: {manager.GetType().Name}");
                }
                catch (System.Exception e)
                {
                    LogServerError($"최적화 적용 실패: {manager.GetType().Name} - {e.Message}");
                }
            }

            LogServerInfo("=== 중앙화된 최적화 적용 완료 ===");
        }

        /// <summary>
        /// 특정 매니저에 최적화 적용
        /// </summary>
        public void ApplyOptimizationToManager(INetworkOptimizable manager)
        {
            if (optimizationSettings == null) return;

            manager.ApplyOptimizationSettings(optimizationSettings);
            LogServerInfo($"개별 최적화 적용: {manager.GetType().Name}");
        }

        /// <summary>
        /// 최적화 설정 업데이트
        /// </summary>
        public void UpdateOptimizationSettings(NetworkOptimizationSettings newSettings)
        {
            optimizationSettings = newSettings;
            ApplyOptimizationToAllManagers();
            LogServerInfo("최적화 설정 업데이트 완료");
        }

        /// <summary>
        /// 모든 매니저의 퍼포먼스 통계 수집
        /// </summary>
        public Dictionary<string, NetworkPerformanceStats> GetAllPerformanceStats()
        {
            var stats = new Dictionary<string, NetworkPerformanceStats>();

            foreach (var manager in networkManagers)
            {
                var managerStats = manager.GetPerformanceStats();
                stats[manager.GetType().Name] = managerStats;
            }

            return stats;
        }

        /// <summary>
        /// 네트워크 품질 자동 조절
        /// </summary>
        public void AutoAdjustNetworkQuality()
        {
            if (!optimizationSettings.enableAutoQuality) return;

            // 네트워크 지연 시간에 따른 품질 조절
            var latency = PhotonNetwork.GetPing();
            NetworkQuality targetQuality;

            if (latency < 50) targetQuality = NetworkQuality.High;
            else if (latency < 100) targetQuality = NetworkQuality.Medium;
            else targetQuality = NetworkQuality.Low;

            // 현재 품질과 다를 때만 적용
            if (optimizationSettings.networkQuality != targetQuality)
            {
                optimizationSettings.networkQuality = targetQuality;

                // 모든 매니저에 품질 적용
                foreach (var manager in networkManagers)
                {
                    manager.SetNetworkQuality(targetQuality);
                }

                LogServerInfo($"네트워크 품질 자동 조절: {targetQuality} (지연: {latency}ms)");
            }
        }

        #endregion

        #region 퍼포먼스 최적화 시스템

        /// <summary>
        /// 1. 서버 권한 모델 - MasterClient에서만 몬스터 AI 처리
        /// </summary>
        private void UpdateServerAuthority()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // MasterClient에서만 몬스터 AI 처리
            UpdateMonsterAI();

            // 변경된 데이터만 동기화
            SyncChangedData();
        }

        /// <summary>
        /// 2. 데이터 전송 최소화 - 변경된 데이터만 전송
        /// </summary>
        private void UpdateDataMinimization()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // 위치 동기화 주기 확인
            int localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            if (lastSyncTimes.ContainsKey(localActorNumber) &&
                Time.time - lastSyncTimes[localActorNumber] >= optimizationSettings.positionSyncInterval)
            {
                SyncPlayerPositions();
                lastSyncTimes[localActorNumber] = Time.time;
            }
            else if (!lastSyncTimes.ContainsKey(localActorNumber))
            {
                lastSyncTimes[localActorNumber] = Time.time;
            }

            // 몬스터 동기화 주기 확인
            if (Time.time - lastMonsterBatchTime >= optimizationSettings.monsterSyncInterval)
            {
                SyncMonsterBatch();
                lastMonsterBatchTime = Time.time;
            }
        }

        /// <summary>
        /// 3. 패킷 묶기 (Batching) - 여러 몬스터를 한 번에 전송
        /// </summary>
        private void SyncMonsterBatch()
        {
            if (pendingMonsterUpdates.Count == 0) return;

            // 최대 배치 크기로 제한
            var batchSize = Mathf.Min(pendingMonsterUpdates.Count, optimizationSettings.maxMonstersPerBatch);
            var batch = pendingMonsterUpdates.GetRange(0, batchSize);

            // RPC로 배치 전송
            GetComponent<PhotonView>().RPC("RPC_UpdateMonsterBatch", RpcTarget.All,
                batch.ToArray());

            // 처리된 몬스터 제거
            pendingMonsterUpdates.RemoveRange(0, batchSize);
        }

        /// <summary>
        /// 4. LOD & 근처만 업데이트 - 플레이어 근처 몬스터만 동기화
        /// </summary>
        private void UpdateLODSystem()
        {
            if (!optimizationSettings.enableLOD) return;

            foreach (var player in PhotonNetwork.PlayerList)
            {
                // 플레이어 근처 몬스터만 업데이트
                var nearbyMonsterIds = GetNearbyMonsters(player.ActorNumber);
                nearbyMonsters[player.ActorNumber] = nearbyMonsterIds;
            }
        }

        /// <summary>
        /// 5. 보간/예측 - 클라이언트에서 자연스러운 움직임 처리
        /// </summary>
        private void UpdateInterpolation()
        {
            if (!optimizationSettings.enableInterpolation) return;

            // 플레이어 위치 보간
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (lastPlayerPositions.ContainsKey(player.ActorNumber))
                {
                    // 보간 처리 로직
                    InterpolatePlayerPosition(player.ActorNumber);
                }
            }
        }

        /// <summary>
        /// 몬스터 AI 업데이트 (MasterClient만)
        /// </summary>
        private void UpdateMonsterAI()
        {
            foreach (var monster in serverMonsterData.Values)
            {
                // 몬스터 AI 로직 처리
                ProcessMonsterAI(monster);

                // 변경된 몬스터를 배치에 추가
                if (monsterDataChanged[monster.id])
                {
                    pendingMonsterUpdates.Add(monster);
                    monsterDataChanged[monster.id] = false;
                }
            }
        }

        /// <summary>
        /// 변경된 데이터만 동기화
        /// </summary>
        private void SyncChangedData()
        {
            // 플레이어 데이터 변경 확인
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (playerDataChanged.ContainsKey(player.ActorNumber) &&
                    playerDataChanged[player.ActorNumber])
                {
                    // 변경된 플레이어 데이터만 전송
                    SyncPlayerData(player.ActorNumber);
                    playerDataChanged[player.ActorNumber] = false;
                }
            }
        }

        /// <summary>
        /// 플레이어 위치 동기화
        /// </summary>
        private void SyncPlayerPositions()
        {
            // 위치가 변경된 플레이어만 동기화
            foreach (var player in PhotonNetwork.PlayerList)
            {
                var currentPos = GetPlayerPosition(player.ActorNumber);
                if (lastPlayerPositions.ContainsKey(player.ActorNumber))
                {
                    var lastPos = lastPlayerPositions[player.ActorNumber];
                    if (Vector3.Distance(currentPos, lastPos) > 0.1f) // 0.1f 이상 이동했을 때만
                    {
                        // 위치 동기화 RPC 호출
                        GetComponent<PhotonView>().RPC("RPC_UpdatePlayerPosition", RpcTarget.All,
                            player.ActorNumber, currentPos);

                        lastPlayerPositions[player.ActorNumber] = currentPos;
                    }
                }
            }
        }

        /// <summary>
        /// 플레이어 근처 몬스터 가져오기
        /// </summary>
        private List<int> GetNearbyMonsters(int playerActorNumber)
        {
            var nearbyMonsters = new List<int>();
            var playerPos = GetPlayerPosition(playerActorNumber);

            foreach (var monster in serverMonsterData.Values)
            {
                var distance = Vector3.Distance(playerPos, monster.position);
                if (distance <= optimizationSettings.playerSyncRadius)
                {
                    nearbyMonsters.Add(monster.id);
                }
            }

            return nearbyMonsters;
        }

        /// <summary>
        /// 플레이어 위치 보간
        /// </summary>
        private void InterpolatePlayerPosition(int actorNumber)
        {
            // 보간 처리 로직
            // 클라이언트에서 자연스러운 움직임을 위해 사용
        }

        /// <summary>
        /// 몬스터 AI 처리
        /// </summary>
        private void ProcessMonsterAI(MonsterData monster)
        {
            // 몬스터 AI 로직
            // 플레이어 추적, 공격, 이동 등
        }

        /// <summary>
        /// 플레이어 위치 가져오기
        /// </summary>
        private Vector3 GetPlayerPosition(int actorNumber)
        {
            // 플레이어 위치 가져오기 로직
            return Vector3.zero; // 임시
        }

        /// <summary>
        /// 플레이어 데이터 동기화
        /// </summary>
        private void SyncPlayerData(int actorNumber)
        {
            // 플레이어 데이터 동기화 로직
        }

        #endregion

        #region 퍼포먼스 최적화 RPC 메서드들

        /// <summary>
        /// 몬스터 배치 업데이트 RPC
        /// </summary>
        [PunRPC]
        public void RPC_UpdateMonsterBatch(MonsterData[] monsters)
        {
            // 모든 클라이언트에서 몬스터 배치 업데이트
            foreach (var monster in monsters)
            {
                serverMonsterData[monster.id] = monster;
            }

            LogServerInfo($"몬스터 배치 업데이트: {monsters.Length}마리");
        }

        /// <summary>
        /// 플레이어 위치 업데이트 RPC
        /// </summary>
        [PunRPC]
        public void RPC_UpdatePlayerPosition(int actorNumber, Vector3 position)
        {
            // 플레이어 위치 업데이트
            lastPlayerPositions[actorNumber] = position;
        }

        /// <summary>
        /// 플레이어 데이터 업데이트 RPC (변경된 데이터만)
        /// </summary>
        [PunRPC]
        public void RPC_UpdatePlayerData(int actorNumber, Generic.PlayerData playerData)
        {
            // 변경된 플레이어 데이터만 업데이트
            serverPlayerData[actorNumber] = playerData;
        }

        #endregion

        #region 서버 관리자용: 플레이어 데이터 관리

        /// <summary>
        /// 서버에서 플레이어 데이터 로드
        /// TODO: Firebase 연동 시 Firebase에서 데이터 로드하도록 변경
        /// TODO: Firebase 팀원이 구현: RoomDataManager.LoadPlayerData()
        /// </summary>
        public void LoadPlayerData(int actorNumber, Generic.PlayerData playerData)
        {
            if (!isServerMode) return;

            // TODO: Firebase 연동 시 제거 - 현재는 임시 메모리 저장
            serverPlayerData[actorNumber] = playerData;

            // NetworkPlayerStats는 삭제됨 - PlayerStateMachine에서 직접 관리
            // 플레이어 데이터는 PlayerStateMachine을 통해 관리

            LogServerInfo($"플레이어 {actorNumber} 데이터 로드 완료");
        }

        /// <summary>
        /// 서버에서 플레이어 데이터 저장
        /// TODO: Firebase 연동 시 Firebase에 데이터 저장하도록 변경
        /// TODO: Firebase 팀원이 구현: PlayerDataManager.SavePlayerData()
        /// </summary>
        public Generic.PlayerData SavePlayerData(int actorNumber)
        {
            if (!isServerMode) return null;

            // NetworkPlayerStats는 삭제됨 - PlayerStateMachine에서 직접 관리
            // 플레이어 데이터는 PlayerStateMachine을 통해 관리
            var playerStateMachine = FindObjectOfType<PlayerStateMachine>();
            if (playerStateMachine != null)
            {
                // PlayerStateMachine에서 데이터 가져오기

                // TODO: Firebase 연동 시 제거 - 현재는 임시 메모리 저장
                serverPlayerData[actorNumber] = new PlayerData(); // 임시 데이터

                // TODO: Firebase 연동 시 추가
                // await playerDataManager.SavePlayerData(currentRoomId, $"player_{actorNumber}", playerData);

                LogServerInfo($"플레이어 {actorNumber} 데이터 저장 완료");
                return new PlayerData(); // 임시 데이터 반환
            }

            return null;
        }

        /// <summary>
        /// 서버에서 모든 플레이어 데이터 저장
        /// TODO: Firebase 연동 시 Firebase에 모든 데이터 저장하도록 변경
        /// TODO: Firebase 팀원이 구현: RoomDataManager.SaveAllPlayerData()
        /// </summary>
        public Dictionary<int, Generic.PlayerData> SaveAllPlayerData()
        {
            if (!isServerMode) return null;

            var allPlayerData = new Dictionary<int, Generic.PlayerData>();

            foreach (var player in PhotonNetwork.PlayerList)
            {
                var playerData = SavePlayerData(player.ActorNumber);
                if (playerData != null)
                {
                    allPlayerData[player.ActorNumber] = playerData;
                }
            }

            // TODO: Firebase 연동 시 추가
            // await roomDataManager.SaveAllPlayerData(currentRoomId, allPlayerData);

            LogServerInfo($"모든 플레이어 데이터 저장 완료: {allPlayerData.Count}명");
            return allPlayerData;
        }

        #endregion

        #region 서버 관리자용: 월드 데이터 관리

        /// <summary>
        /// 서버에서 월드 데이터 로드
        /// TODO: Firebase 연동 시 Firebase에서 월드 데이터 로드하도록 변경
        /// TODO: Firebase 팀원이 구현: RoomDataManager.LoadWorldData()
        /// </summary>
        public void LoadWorldData(WorldDataStruct worldDataStruct)
        {
            if (!isServerMode) return;

            // TODO: Firebase 연동 시 추가
            // var worldData = await roomDataManager.LoadWorldData(currentRoomId);

            // 월드 데이터 로드 로직
            LogServerInfo("월드 데이터 로드 완료");
        }

        /// <summary>
        /// 서버에서 월드 데이터 저장
        /// TODO: Firebase 연동 시 Firebase에 월드 데이터 저장하도록 변경
        /// TODO: Firebase 팀원이 구현: RoomDataManager.SaveWorldData()
        /// </summary>
        public WorldDataStruct? SaveWorldData()
        {
            if (!isServerMode) return null;

            // TODO: Firebase 연동 시 추가
            // await roomDataManager.SaveWorldData(currentRoomId, worldData);

            // 월드 데이터 저장 로직
            LogServerInfo("월드 데이터 저장 완료");
            return new WorldDataStruct();
        }

        #endregion

        #region 서버 관리자용: 게임 상태 관리

        /// <summary>
        /// 서버 상태 확인
        /// </summary>
        public bool IsServerRunning()
        {
            return isServerMode && PhotonNetwork.IsConnected && PhotonNetwork.InRoom;
        }

        /// <summary>
        /// 서버 통계 정보
        /// </summary>
        public void GetServerStats()
        {
            if (!isServerMode) return;

            LogServerInfo("=== 서버 상태 ===");
            LogServerInfo($"연결 상태: {PhotonNetwork.IsConnected}");
            LogServerInfo($"룸 상태: {PhotonNetwork.InRoom}");
            LogServerInfo($"마스터 클라이언트: {PhotonNetwork.IsMasterClient}");
            LogServerInfo($"플레이어 수: {PhotonNetwork.PlayerList.Length}");
            LogServerInfo($"서버 데이터: {serverPlayerData.Count}명");

            LogServerInfo("=== 퍼포먼스 최적화 ===");
            LogServerInfo($"위치 동기화 주기: {optimizationSettings.positionSyncInterval}초");
            LogServerInfo($"몬스터 동기화 주기: {optimizationSettings.monsterSyncInterval}초");
            LogServerInfo($"배치 크기: {optimizationSettings.maxMonstersPerBatch}마리");
            LogServerInfo($"동기화 반경: {optimizationSettings.playerSyncRadius}m");
            LogServerInfo($"LOD 활성화: {optimizationSettings.enableLOD}");
            LogServerInfo($"보간 활성화: {optimizationSettings.enableInterpolation}");
            LogServerInfo($"대기 중인 몬스터 업데이트: {pendingMonsterUpdates.Count}마리");
        }

        #endregion

        #region 서버 관리자용: 디버그 및 로깅

        /// <summary>
        /// 서버 로그 출력
        /// </summary>
        private void LogServerInfo(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[서버관리자] {message}");
            }
        }

        /// <summary>
        /// 서버 경고 로그
        /// </summary>
        private void LogServerWarning(string message)
        {
            if (enableDebugLog)
            {
                Debug.LogWarning($"[서버관리자] {message}");
            }
        }

        /// <summary>
        /// 서버 에러 로그
        /// </summary>
        private void LogServerError(string message)
        {
            Debug.LogError($"[서버관리자] {message}");
        }

        #endregion

        #region Photon 콜백

        /// <summary>
        /// 플레이어 입장 시 서버 처리
        /// </summary>
        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (isServerMode)
            {
                LogServerInfo($"플레이어 {newPlayer.ActorNumber} 입장");

                // 서버에서 플레이어 데이터 로드
                if (serverPlayerData.ContainsKey(newPlayer.ActorNumber))
                {
                    LoadPlayerData(newPlayer.ActorNumber, serverPlayerData[newPlayer.ActorNumber]);
                }
            }
        }

        /// <summary>
        /// 플레이어 퇴장 시 서버 처리
        /// </summary>
        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (isServerMode)
            {
                LogServerInfo($"플레이어 {otherPlayer.ActorNumber} 퇴장");

                // 서버에서 플레이어 데이터 저장
                SavePlayerData(otherPlayer.ActorNumber);
            }
        }

        #endregion

        #region 디버그

        private void OnGUI()
        {
            if (!isServerMode || !enableDebugLog) return;

            GUILayout.BeginArea(new Rect(10, 10, 400, 300));
            GUILayout.Label("=== 서버 관리자 ===");
            GUILayout.Label($"서버 모드: {isServerMode}");
            GUILayout.Label($"연결 상태: {PhotonNetwork.IsConnected}");
            GUILayout.Label($"룸 상태: {PhotonNetwork.InRoom}");
            GUILayout.Label($"플레이어 수: {PhotonNetwork.PlayerList.Length}");
            GUILayout.Label($"서버 데이터: {serverPlayerData.Count}명");

            GUILayout.Space(10);
            GUILayout.Label("=== 중앙화된 최적화 ===");
            GUILayout.Label($"설정 파일: {(optimizationSettings != null ? "적용됨" : "없음")}");
            GUILayout.Label($"자동 적용: {autoApplyOptimization}");
            GUILayout.Label($"등록된 매니저: {networkManagers.Count}개");

            if (optimizationSettings != null)
            {
                GUILayout.Label($"위치 동기화: {optimizationSettings.positionSyncInterval}초");
                GUILayout.Label($"몬스터 동기화: {optimizationSettings.monsterSyncInterval}초");
                GUILayout.Label($"배치 크기: {optimizationSettings.maxMonstersPerBatch}마리");
                GUILayout.Label($"동기화 반경: {optimizationSettings.playerSyncRadius}m");
                GUILayout.Label($"LOD: {optimizationSettings.enableLOD}");
                GUILayout.Label($"보간: {optimizationSettings.enableInterpolation}");
                GUILayout.Label($"네트워크 품질: {optimizationSettings.networkQuality}");
            }

            if (GUILayout.Button("서버 상태 확인"))
            {
                GetServerStats();
            }

            if (GUILayout.Button("모든 데이터 저장"))
            {
                SaveAllPlayerData();
            }

            if (GUILayout.Button("최적화 적용"))
            {
                ApplyOptimizationToAllManagers();
            }

            if (GUILayout.Button("품질 자동 조절"))
            {
                AutoAdjustNetworkQuality();
            }

            if (GUILayout.Button("퍼포먼스 통계"))
            {
                var stats = GetAllPerformanceStats();
                foreach (var stat in stats)
                {
                    LogServerInfo($"{stat.Key}: RPC {stat.Value.rpcCount}, 배치 {stat.Value.batchCount}");
                }
            }

            GUILayout.EndArea();
        }

        #endregion
    }

    #region 퍼포먼스 최적화 데이터 구조체

    /// <summary>
    /// 몬스터 데이터 (퍼포먼스 최적화용)
    /// </summary>
    [System.Serializable]
    public struct MonsterData
    {
        public int id;
        public Vector3 position;
        public Quaternion rotation;
        public float health;
        public float maxHealth;
        public bool isAlive;
        public int monsterType;
        public float lastUpdateTime;
    }

    /// <summary>
    /// 월드 데이터 (퍼포먼스 최적화용)
    /// </summary>
    [System.Serializable]
    public struct WorldDataStruct
    {
        public Dictionary<Vector3Int, WorldTileData> worldTiles;
        public Dictionary<int, NetworkStructure> structures;
        public Dictionary<Vector3Int, ResourceNode> resourceNodes;
        public float lastUpdateTime;
    }

    #endregion
}
