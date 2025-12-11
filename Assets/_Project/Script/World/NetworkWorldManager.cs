using _Project.Script.Generic;
using _Project.Script.EventStruct;
using _Project.Script.Manager;
using _Project.Script.Interface;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace _Project.Script.World
{
    /// <summary>
    /// PUN2 기반 월드 상태 동기화 관리자 (리팩토링됨)
    /// 각 기능별 매니저 클래스들을 조율하는 중앙 관리자
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    [DefaultExecutionOrder(-60)]
    public class NetworkWorldManager : MonoBehaviourPunCallbacks, IPunObservable, INetworkOptimizable
    {
        // 싱글톤 인스턴스
        public static NetworkWorldManager Instance { get; private set; }

        [Header("월드 설정")]
        [SerializeField] private int worldSize = 1000; // 월드 크기
        [SerializeField] private float syncInterval = 0.5f; // 동기화 간격
        [SerializeField] private bool enableWorldSync = true;

        [Header("구조물 설정")]
        [SerializeField] private int maxStructures = 1000; // 최대 구조물 수
        [SerializeField] private float structureSyncRadius = 50f; // 구조물 동기화 반경

        [Header("구조물 프리팹")]
        [SerializeField]
        private string[] structurePrefabNames = {
            "WallPrefab", "DoorPrefab", "WindowPrefab", "FloorPrefab",
            "RoofPrefab", "ChestPrefab", "WorkbenchPrefab", "FurnacePrefab"
        };

        // 기능별 매니저 클래스들
        private WorldDataManager worldDataManager;
        private StructureManager structureManager;
        private ResourceManager resourceManager;
        private WorldNetworkSync networkSync;

        // 동기화 상태
        private bool isInitialized = false;
        private float lastSyncTime = 0f;

        // 최적화 관련 변수들
        private bool isOptimized = false;
        private NetworkPerformanceStats performanceStats;

        private void Awake()
        {
            // 싱글톤 초기화
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 매니저 클래스들 초기화
            InitializeManagers();
        }

        /// <summary>
        /// 매니저 클래스들 초기화
        /// </summary>
        private void InitializeManagers()
        {
            // 월드 데이터 매니저 초기화
            worldDataManager = new WorldDataManager();
            worldDataManager.InitializeWorld(worldSize);

            // 구조물 매니저 초기화
            structureManager = new StructureManager(worldDataManager, structurePrefabNames);

            // 리소스 매니저 초기화
            resourceManager = new ResourceManager(worldDataManager);

            // 네트워크 동기화 매니저 초기화
            networkSync = new WorldNetworkSync(worldDataManager, GetComponent<PhotonView>());

            Debug.Log("[NetworkWorldManager] 모든 매니저 클래스 초기화 완료");
        }

        private void Start()
        {
            // 이벤트 구독
            EventHub.Instance.RegisterEvent<OnPlayerSpawnedEvent>(OnPlayerSpawned);
            EventHub.Instance.RegisterEvent<PunEvents.OnPlayerLeftRoomEvent>(OnPlayerLeftRoom);
        }

        private void Update()
        {
            if (!isInitialized || !enableWorldSync) return;

            // 주기적으로 월드 상태 동기화
            if (Time.time - lastSyncTime >= syncInterval)
            {
                SyncWorldState();
                lastSyncTime = Time.time;
            }
        }

        private void OnDestroy()
        {
            // 싱글톤 정리
            if (Instance == this)
            {
                Instance = null;
            }

            // 이벤트 구독 해제
            if (EventHub.Instance != null)
            {
                EventHub.Instance.UnregisterEvent<OnPlayerSpawnedEvent>(OnPlayerSpawned);
                EventHub.Instance.UnregisterEvent<PunEvents.OnPlayerLeftRoomEvent>(OnPlayerLeftRoom);
            }
        }

        /// <summary>
        /// MasterClient 교체 처리
        /// </summary>
        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            Debug.Log($"[NetworkWorldManager] MasterClient 교체 - 새로운 마스터: {newMasterClient.NickName}");

            // 새로운 MasterClient가 월드 상태를 이어받도록 스냅샷 전송
            if (PhotonNetwork.IsMasterClient)
            {
                networkSync.SyncWorldStateToAllClients();
            }
        }

        /// <summary>
        /// 모든 클라이언트에게 월드 상태 스냅샷 전송
        /// </summary>
        private void SyncWorldStateToAllClients()
        {
            var stats = worldDataManager.GetWorldStatistics();

            // 월드 상태를 새로운 MasterClient가 모든 클라이언트에게 전송
            GetComponent<PhotonView>().RPC("SyncWorldStateRPC", RpcTarget.All,
                stats.tileCount, stats.structureCount, stats.resourceCount);
        }

        /// <summary>
        /// 월드 상태 동기화 RPC
        /// </summary>
        [PunRPC]
        public void SyncWorldStateRPC(int tileCount, int structureCount, int resourceCount)
        {
            Debug.Log($"[NetworkWorldManager] 월드 상태 동기화 - 타일: {tileCount}, 구조물: {structureCount}, 리소스: {resourceCount}");

            // 월드 상태 동기화 이벤트 발생
            EventHub.Instance.RaiseEvent(new PunEvents.OnWorldSyncEvent
            {
                structureCount = structureCount,
                resourceCount = resourceCount
            });
        }


        #region PUN2 동기화

        /// <summary>
        /// PUN2 네트워크 동기화 (월드 데이터)
        /// </summary>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // 데이터 전송 (로컬 플레이어)
                networkSync.SendWorldData(stream);
            }
            else
            {
                // 데이터 수신 (원격 플레이어)
                networkSync.ReceiveWorldData(stream);
            }
        }


        #endregion

        #region RPC 기반 월드 액션

        /// <summary>
        /// 구조물 건설 (RPC)
        /// </summary>
        [PunRPC]
        public void BuildStructureRPC(Vector3 position, Quaternion rotation, StructureType structureType, int actorNumber)
        {
            int structureId = structureManager.ProcessBuildStructureRPC(position, rotation, structureType, actorNumber, maxStructures);

            if (structureId > 0)
            {
                // 실제 구조물 GameObject 생성
                GameObject structureObject = structureManager.CreateStructureGameObject(structureId, position, rotation, structureType);

                // 이벤트 발생
                EventHub.Instance.RaiseEvent(new PunEvents.OnStructureBuiltEvent
                {
                    structureId = structureId,
                    position = position,
                    structureType = structureType,
                    builderActorNumber = actorNumber
                });
            }
        }

        /// <summary>
        /// 구조물 파괴 (RPC)
        /// </summary>
        [PunRPC]
        public void DestroyStructureRPC(int structureId, int actorNumber)
        {
            bool success = structureManager.ProcessDestroyStructureRPC(structureId, actorNumber);

            if (success)
            {
                // 실제 구조물 GameObject 파괴
                structureManager.DestroyStructureGameObject(structureId);

                // 구조물 정보 가져오기
                var structure = worldDataManager.GetStructure(structureId);
                if (structure.HasValue)
                {
                    // 이벤트 발생
                    EventHub.Instance.RaiseEvent(new PunEvents.OnStructureDestroyedEvent
                    {
                        structureId = structureId,
                        position = structure.Value.position,
                        destroyerActorNumber = actorNumber
                    });
                }
            }
        }

        /// <summary>
        /// 리소스 채집 (RPC)
        /// </summary>
        [PunRPC]
        public void HarvestResourceRPC(Vector3 position, int amount, int actorNumber)
        {
            bool success = resourceManager.ProcessHarvestResourceRPC(position, amount, actorNumber);

            if (success)
            {
                // 리소스 정보 가져오기
                Vector3Int pos = new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));
                var resource = worldDataManager.GetResourceNode(pos);

                if (resource.HasValue)
                {
                    // 이벤트 발생
                    EventHub.Instance.RaiseEvent(new PunEvents.OnResourceHarvestedEvent
                    {
                        position = pos,
                        resourceType = resource.Value.resourceType,
                        amount = amount,
                        remainingAmount = resource.Value.amount,
                        harvesterActorNumber = actorNumber
                    });
                }
            }
        }

        /// <summary>
        /// 리소스 재생성 (RPC)
        /// </summary>
        [PunRPC]
        public void RegenerateResourceRPC(Vector3Int position, ResourceType resourceType, int amount)
        {
            resourceManager.ProcessRegenerateResourceRPC(position, resourceType, amount);

            // 이벤트 발생
            EventHub.Instance.RaiseEvent(new PunEvents.OnResourceRegeneratedEvent
            {
                position = position,
                resourceType = resourceType,
                amount = amount
            });
        }

        #endregion


        #region 구조물 소유권 관리

        /// <summary>
        /// 구조물 소유권 확인
        /// </summary>
        public bool CanDestroyStructure(int structureId, int actorNumber)
        {
            return worldDataManager.CanDestroyStructure(structureId, actorNumber, PhotonNetwork.IsMasterClient);
        }

        /// <summary>
        /// 플레이어의 구조물 목록 가져오기
        /// </summary>
        public List<int> GetPlayerStructures(int actorNumber)
        {
            return worldDataManager.GetPlayerStructures(actorNumber);
        }

        /// <summary>
        /// 구조물 소유자 변경 (MasterClient만 가능)
        /// </summary>
        public void TransferStructureOwnership(int structureId, int newOwnerActorNumber)
        {
            structureManager.TransferStructureOwnership(structureId, newOwnerActorNumber);
        }

        #endregion

        #region 이벤트 처리

        private void OnPlayerSpawned(OnPlayerSpawnedEvent spawnEvent)
        {
            if (spawnEvent.isMine)
            {
                isInitialized = true;
                Debug.Log("[NetworkWorldManager] 로컬 플레이어 스폰 완료 - 월드 동기화 활성화");
            }
        }

        private void OnPlayerLeftRoom(PunEvents.OnPlayerLeftRoomEvent leftEvent)
        {
            // 플레이어 관련 월드 데이터 정리 (필요시)
            Debug.Log($"[NetworkWorldManager] 플레이어 {leftEvent.playerName} 퇴장 - 월드 데이터 정리");
        }

        #endregion

        #region 월드 상태 동기화

        /// <summary>
        /// 월드 상태 동기화
        /// </summary>
        private void SyncWorldState()
        {
            // 로컬 플레이어 주변의 월드 상태만 동기화
            if (PhotonNetwork.LocalPlayer != null)
            {
                // 플레이어 위치 기반 동기화 (나중에 구현)
                // 현재는 전체 월드 동기화
            }
        }

        #endregion

        #region 검증 로직 (권장사항 적용)

        /// <summary>
        /// 위치 점유 여부 확인 (중복 건설 방지)
        /// </summary>
        public bool IsPositionOccupied(Vector3 position)
        {
            return worldDataManager.IsPositionOccupied(position);
        }

        /// <summary>
        /// 리소스 가용성 확인 (중복 채집 방지)
        /// </summary>
        public bool IsResourceAvailable(Vector3Int position, int amount)
        {
            return worldDataManager.IsResourceAvailable(position, amount);
        }

        /// <summary>
        /// 플레이어 권한 검증
        /// </summary>
        public bool ValidatePlayerAction(int actorNumber, string action)
        {
            return structureManager.ValidatePlayerAction(actorNumber, action);
        }

        #endregion

        #region 공개 API

        /// <summary>
        /// 구조물 건설 (네트워크) - 모든 플레이어 허용
        /// </summary>
        public void BuildStructure(Vector3 position, Quaternion rotation, StructureType structureType)
        {
            structureManager.BuildStructure(position, rotation, structureType, PhotonNetwork.LocalPlayer.ActorNumber, GetComponent<PhotonView>());
        }

        /// <summary>
        /// 구조물 파괴 (네트워크)
        /// </summary>
        public void DestroyStructure(int structureId)
        {
            structureManager.DestroyStructure(structureId, GetComponent<PhotonView>());
        }

        /// <summary>
        /// 리소스 채집 (네트워크)
        /// </summary>
        public void HarvestResource(Vector3Int position, int amount)
        {
            resourceManager.HarvestResource(position, amount, GetComponent<PhotonView>());
        }

        /// <summary>
        /// 리소스 재생성 (네트워크)
        /// </summary>
        public void RegenerateResource(Vector3Int position, ResourceType resourceType, int amount)
        {
            resourceManager.RegenerateResource(position, resourceType, amount, GetComponent<PhotonView>());
        }

        /// <summary>
        /// 구조물 정보 가져오기
        /// </summary>
        public NetworkStructure? GetStructure(int structureId)
        {
            return worldDataManager.GetStructure(structureId);
        }

        /// <summary>
        /// 리소스 노드 정보 가져오기
        /// </summary>
        public ResourceNode? GetResourceNode(Vector3Int position)
        {
            return worldDataManager.GetResourceNode(position);
        }

        /// <summary>
        /// 월드 타일 정보 가져오기
        /// </summary>
        public WorldTileData? GetWorldTile(Vector3Int position)
        {
            return worldDataManager.GetWorldTile(position);
        }


        #endregion

        #region Photon 콜백

        /// <summary>
        /// 룸 프로퍼티 변경 시 호출
        /// </summary>
        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            networkSync.OnRoomPropertiesUpdate(propertiesThatChanged);
        }

        #endregion

        #region INetworkOptimizable 구현

        /// <summary>
        /// 최적화 설정 적용
        /// </summary>
        public void ApplyOptimizationSettings(NetworkOptimizationSettings settings)
        {
            if (settings == null) return;

            syncInterval = settings.worldSyncInterval;
            isOptimized = true;

            performanceStats = new NetworkPerformanceStats
            {
                managerName = "NetworkWorldManager",
                syncInterval = syncInterval,
                rpcCount = 0,
                batchCount = 0,
                networkTraffic = 0f,
                isOptimized = true,
                lastUpdateTime = Time.time
            };

            Debug.Log($"[NetworkWorldManager] 최적화 설정 적용: 동기화 주기 {syncInterval}초");
        }

        /// <summary>
        /// 동기화 주기 설정
        /// </summary>
        public void SetSyncInterval(float interval)
        {
            syncInterval = Mathf.Max(0.5f, interval);
            performanceStats.syncInterval = syncInterval;
        }

        /// <summary>
        /// 보간 처리 설정
        /// </summary>
        public void SetInterpolation(bool enable, float speed = 6f)
        {
            // NetworkWorldManager는 보간이 필요하지 않음
        }

        /// <summary>
        /// LOD 설정
        /// </summary>
        public void SetLOD(bool enable, float radius = 50f)
        {
            // NetworkWorldManager는 LOD가 필요하지 않음
        }

        /// <summary>
        /// 배치 크기 설정
        /// </summary>
        public void SetBatchSize(int batchSize)
        {
            // NetworkWorldManager는 배치가 필요하지 않음
        }

        /// <summary>
        /// 네트워크 품질 설정
        /// </summary>
        public void SetNetworkQuality(NetworkQuality quality)
        {
            switch (quality)
            {
                case NetworkQuality.Low:
                    syncInterval = 3.0f;
                    break;
                case NetworkQuality.Medium:
                    syncInterval = 1.5f;
                    break;
                case NetworkQuality.High:
                    syncInterval = 0.5f;
                    break;
            }

            performanceStats.syncInterval = syncInterval;
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

        #region CustomProperties 최적화

        /// <summary>
        /// 구조물 상태 정보를 CustomProperties로 업데이트
        /// </summary>
        public void UpdateStructureProperties(int structureId, StructureType type, float health, bool isDestroyed)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                var props = new ExitGames.Client.Photon.Hashtable();
                props[$"Structure_{structureId}_Type"] = (int)type;
                props[$"Structure_{structureId}_Health"] = health;
                props[$"Structure_{structureId}_Destroyed"] = isDestroyed;

                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }
        }

        /// <summary>
        /// 리소스 노드 상태 정보를 CustomProperties로 업데이트
        /// </summary>
        public void UpdateResourceProperties(int resourceId, ResourceType type, float amount, bool isDepleted)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                var props = new ExitGames.Client.Photon.Hashtable();
                props[$"Resource_{resourceId}_Type"] = (int)type;
                props[$"Resource_{resourceId}_Amount"] = amount;
                props[$"Resource_{resourceId}_Depleted"] = isDepleted;

                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }
        }


        #endregion
    }

    #region 데이터 구조체 및 열거형

    /// <summary>
    /// 월드 타일 데이터 구조체
    /// </summary>
    [System.Serializable]
    public struct WorldTileData
    {
        public Vector3Int position;
        public TileType tileType;
        public bool isOccupied;
        public int structureId;
    }

    /// <summary>
    /// 네트워크 구조물 데이터 구조체
    /// </summary>
    [System.Serializable]
    public struct NetworkStructure
    {
        public int id;
        public Vector3 position;
        public Quaternion rotation;
        public StructureType structureType;
        public float health;
        public bool isDestroyed;
        public int ownerActorNumber;  // 구조물 소유자
    }

    /// <summary>
    /// 리소스 노드 데이터 구조체
    /// </summary>
    [System.Serializable]
    public struct ResourceNode
    {
        public Vector3Int position;
        public ResourceType resourceType;
        public int amount;
        public bool isDepleted;
    }

    /// <summary>
    /// 타일 타입 열거형
    /// TODO: ScriptableObject 또는 JSON 데이터로 전환 예정
    /// </summary>
    public enum TileType
    {
        Grass,
        Stone,
        Water,
        Sand,
        Forest
    }

    /// <summary>
    /// 구조물 타입 열거형
    /// TODO: ScriptableObject 또는 JSON 데이터로 전환 예정
    /// </summary>
    public enum StructureType
    {
        Wall,
        Door,
        Window,
        Floor,
        Roof,
        Chest,
        Workbench,
        Furnace
    }

    /// <summary>
    /// 리소스 타입 열거형 (돈스타브 특화)
    /// TODO: ScriptableObject 또는 JSON 데이터로 전환 예정
    /// </summary>
    public enum ResourceType
    {
        // 기본 자원
        Wood,       // 나무 (베기)
        Stone,      // 돌 (채굴)
        Grass,      // 풀 (채집)
        Twigs,      // 나뭇가지 (채집)
        Flint,      // 부싯돌 (채굴)

        // 음식 자원
        Berries,    // 베리 (채집)
        Carrots,    // 당근 (채집)
        Seeds,      // 씨앗 (채집)

        // 특수 자원
        Gold,       // 금 (채굴)
        Gems,       // 보석 (채굴)
        Silk,       // 실크 (거미에서)
        HoundTeeth, // 사냥개 이빨 (사냥개에서)

        // 계절별 자원
        Ice,        // 얼음 (겨울)
        Flowers,    // 꽃 (봄)
        Pinecones   // 솔방울 (가을)
    }
    #endregion
    #endregion
}
