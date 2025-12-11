using _Project.Script.Generic;
using _Project.Script.EventStruct;
using _Project.Script.Manager;
using _Project.Script.Interface;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace _Project.Script.Items.Network
{
    /// <summary>
    /// PUN2 기반 아이템 동기화 관리자
    /// 모든 플레이어의 아이템 상태를 실시간으로 동기화
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    [DefaultExecutionOrder(-80)]
    public class NetworkItemManager : MonoSingleton<NetworkItemManager>, IPunObservable, INetworkOptimizable
    {
        [Header("아이템 동기화 설정")]
        [SerializeField] private float syncInterval = 0.1f;
        [SerializeField] private bool enableItemSync = true;

        // 네트워크 동기화용 데이터
        private Dictionary<int, ItemInstance[]> playerInventories = new Dictionary<int, ItemInstance[]>();
        private Dictionary<int, ItemInstance[]> playerEquippedItems = new Dictionary<int, ItemInstance[]>();

        // 로컬 플레이어 데이터
        private ItemInstance[] localInventory;
        private ItemInstance[] localEquippedItems;

        // 동기화 상태
        private bool isInitialized = false;
        private float lastSyncTime = 0f;


        protected override void Awake()
        {
            base.Awake();

            // 인벤토리 초기화 (15슬롯)
            localInventory = new ItemInstance[15];
            localEquippedItems = new ItemInstance[5]; // 장착 아이템 슬롯
        }

        private void Start()
        {
            // 이벤트 구독
            EventHub.Instance.RegisterEvent<OnPlayerSpawnedEvent>(OnPlayerSpawned);
            EventHub.Instance.RegisterEvent<PunEvents.OnPlayerLeftRoomEvent>(OnPlayerLeftRoom);
        }

        private void Update()
        {
            if (!isInitialized || !enableItemSync) return;

            // 주기적으로 로컬 데이터 동기화
            if (Time.time - lastSyncTime >= syncInterval)
            {
                SyncLocalInventory();
                lastSyncTime = Time.time;
            }
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

        #region PUN2 동기화

        /// <summary>
        /// PUN2 네트워크 동기화 (아이템 데이터)
        /// </summary>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // 데이터 전송 (로컬 플레이어)
                SendInventoryData(stream);
                SendEquippedItemsData(stream);
            }
            else
            {
                // 데이터 수신 (원격 플레이어)
                ReceiveInventoryData(stream, info.Sender.ActorNumber);
                ReceiveEquippedItemsData(stream, info.Sender.ActorNumber);
            }
        }

        /// <summary>
        /// 인벤토리 데이터 전송 (실시간 데이터만)
        /// </summary>
        private void SendInventoryData(PhotonStream stream)
        {
            // 인벤토리 크기만 전송 (변경되지 않는 데이터)
            stream.SendNext(localInventory.Length);

            // 실시간으로 변하는 아이템 데이터만 전송
            for (int i = 0; i < localInventory.Length; i++)
            {
                var item = localInventory[i];
                if (item != null && item.itemData != null)
                {
                    stream.SendNext(true); // 아이템 존재
                    stream.SendNext(item.itemData.uid);
                    stream.SendNext(item.count);
                    // lastUseTime은 CustomProperties로 이동 (자주 변하지 않음)
                }
                else
                {
                    stream.SendNext(false); // 빈 슬롯
                }
            }
        }

        /// <summary>
        /// 인벤토리 데이터 수신
        /// </summary>
        private void ReceiveInventoryData(PhotonStream stream, int actorNumber)
        {
            int inventorySize = (int)stream.ReceiveNext();
            ItemInstance[] receivedInventory = new ItemInstance[inventorySize];

            for (int i = 0; i < inventorySize; i++)
            {
                bool hasItem = (bool)stream.ReceiveNext();
                if (hasItem)
                {
                    int itemUID = (int)stream.ReceiveNext();
                    int count = (int)stream.ReceiveNext();
                    float lastUseTime = (float)stream.ReceiveNext();

                    // ItemData 찾기 (UID로)
                    ItemData itemData = FindItemDataByUID(itemUID);
                    if (itemData != null)
                    {
                        receivedInventory[i] = new ItemInstance
                        {
                            itemData = itemData,
                            count = count,
                            lastUseTime = lastUseTime
                        };
                    }
                }
            }

            // 플레이어 인벤토리 업데이트
            playerInventories[actorNumber] = receivedInventory;

            // 이벤트 발생
            EventHub.Instance.RaiseEvent(new OnInventorySyncEvent
            {
                actorNumber = actorNumber,
                inventory = receivedInventory,
                isLocalPlayer = actorNumber == PhotonNetwork.LocalPlayer.ActorNumber
            });
        }

        /// <summary>
        /// 장착 아이템 데이터 전송
        /// </summary>
        private void SendEquippedItemsData(PhotonStream stream)
        {
            stream.SendNext(localEquippedItems.Length);

            for (int i = 0; i < localEquippedItems.Length; i++)
            {
                var item = localEquippedItems[i];
                if (item != null && item.itemData != null)
                {
                    stream.SendNext(true);
                    stream.SendNext(item.itemData.uid);
                    stream.SendNext(item.count);
                }
                else
                {
                    stream.SendNext(false);
                }
            }
        }

        /// <summary>
        /// 장착 아이템 데이터 수신
        /// </summary>
        private void ReceiveEquippedItemsData(PhotonStream stream, int actorNumber)
        {
            int equippedSize = (int)stream.ReceiveNext();
            ItemInstance[] receivedEquipped = new ItemInstance[equippedSize];

            for (int i = 0; i < equippedSize; i++)
            {
                bool hasItem = (bool)stream.ReceiveNext();
                if (hasItem)
                {
                    int itemUID = (int)stream.ReceiveNext();
                    int count = (int)stream.ReceiveNext();

                    ItemData itemData = FindItemDataByUID(itemUID);
                    if (itemData != null)
                    {
                        receivedEquipped[i] = new ItemInstance
                        {
                            itemData = itemData,
                            count = count
                        };
                    }
                }
            }

            playerEquippedItems[actorNumber] = receivedEquipped;

            EventHub.Instance.RaiseEvent(new OnEquippedItemsSyncEvent
            {
                actorNumber = actorNumber,
                equippedItems = receivedEquipped
            });
        }

        #endregion

        #region RPC 기반 아이템 액션

        /// <summary>
        /// 아이템 사용 (RPC)
        /// </summary>
        [PunRPC]
        public void UseItemRPC(int slotIndex, int actorNumber)
        {
            Debug.Log($"[NetworkItemManager] 아이템 사용 RPC - 슬롯: {slotIndex}, 플레이어: {actorNumber}");

            if (playerInventories.TryGetValue(actorNumber, out var inventory))
            {
                if (slotIndex >= 0 && slotIndex < inventory.Length && inventory[slotIndex] != null)
                {
                    inventory[slotIndex].TryUse();

                    // 이벤트 발생
                    EventHub.Instance.RaiseEvent(new OnItemUsedEvent
                    {
                        actorNumber = actorNumber,
                        slotIndex = slotIndex,
                        item = inventory[slotIndex]
                    });
                }
            }
        }

        /// <summary>
        /// 아이템 드롭 (RPC)
        /// </summary>
        [PunRPC]
        public void DropItemRPC(int slotIndex, Vector3 dropPosition, int actorNumber)
        {
            Debug.Log($"[NetworkItemManager] 아이템 드롭 RPC - 슬롯: {slotIndex}, 위치: {dropPosition}");

            if (playerInventories.TryGetValue(actorNumber, out var inventory))
            {
                if (slotIndex >= 0 && slotIndex < inventory.Length && inventory[slotIndex] != null)
                {
                    var droppedItem = inventory[slotIndex];
                    inventory[slotIndex] = null;

                    // 월드에 아이템 오브젝트 생성 (나중에 구현)
                    // CreateWorldItem(droppedItem, dropPosition);

                    EventHub.Instance.RaiseEvent(new OnItemDroppedEvent
                    {
                        actorNumber = actorNumber,
                        slotIndex = slotIndex,
                        item = droppedItem,
                        dropPosition = dropPosition
                    });
                }
            }
        }

        /// <summary>
        /// 아이템 획득 (RPC)
        /// </summary>
        [PunRPC]
        public void PickupItemRPC(int itemUID, int count, int actorNumber)
        {
            Debug.Log($"[NetworkItemManager] 아이템 획득 RPC - UID: {itemUID}, 수량: {count}");

            if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                // 로컬 플레이어만 아이템 추가
                AddItemToLocalInventory(itemUID, count);
            }
        }

        #endregion

        #region 로컬 인벤토리 관리

        /// <summary>
        /// 로컬 인벤토리에 아이템 추가
        /// </summary>
        public bool AddItemToLocalInventory(int itemUID, int count)
        {
            ItemData itemData = FindItemDataByUID(itemUID);
            if (itemData == null) return false;

            ItemInstance newItem = new ItemInstance
            {
                itemData = itemData,
                count = count
            };

            // 기존 DataManager의 AddItem 로직 활용
            // DataManager.Instance.AddItem(newItem, out bool isDestroyed);

            // 임시로 직접 처리
            for (int i = 0; i < localInventory.Length; i++)
            {
                if (localInventory[i] == null)
                {
                    localInventory[i] = newItem;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 로컬 인벤토리 동기화
        /// </summary>
        private void SyncLocalInventory()
        {
            // 로컬 플레이어 데이터를 네트워크에 동기화
            int localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            playerInventories[localActorNumber] = localInventory;
            playerEquippedItems[localActorNumber] = localEquippedItems;
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// UID로 ItemData 찾기
        /// </summary>
        private ItemData FindItemDataByUID(int uid)
        {
            // Resources 폴더에서 모든 ItemData 찾기
            ItemData[] allItems = Resources.LoadAll<ItemData>("");
            return allItems.FirstOrDefault(item => item.uid == uid);
        }

        #endregion

        #region 이벤트 처리

        private void OnPlayerSpawned(OnPlayerSpawnedEvent spawnEvent)
        {
            if (spawnEvent.isMine)
            {
                isInitialized = true;
                Debug.Log("[NetworkItemManager] 로컬 플레이어 스폰 완료 - 아이템 동기화 활성화");
            }
        }

        private void OnPlayerLeftRoom(PunEvents.OnPlayerLeftRoomEvent leftEvent)
        {
            // 플레이어 데이터 정리
            if (playerInventories.ContainsKey(leftEvent.actorNumber))
                playerInventories.Remove(leftEvent.actorNumber);

            if (playerEquippedItems.ContainsKey(leftEvent.actorNumber))
                playerEquippedItems.Remove(leftEvent.actorNumber);
        }

        #endregion

        #region 공개 API

        /// <summary>
        /// 플레이어 인벤토리 가져오기
        /// </summary>
        public ItemInstance[] GetPlayerInventory(int actorNumber)
        {
            return playerInventories.TryGetValue(actorNumber, out var inventory) ? inventory : null;
        }

        /// <summary>
        /// 플레이어 장착 아이템 가져오기
        /// </summary>
        public ItemInstance[] GetPlayerEquippedItems(int actorNumber)
        {
            return playerEquippedItems.TryGetValue(actorNumber, out var equipped) ? equipped : null;
        }

        /// <summary>
        /// 아이템 사용 (네트워크)
        /// </summary>
        public void UseItem(int slotIndex)
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                GetComponent<PhotonView>().RPC("UseItemRPC", RpcTarget.All, slotIndex, PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }

        /// <summary>
        /// 아이템 드롭 (네트워크)
        /// </summary>
        public void DropItem(int slotIndex, Vector3 dropPosition)
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                GetComponent<PhotonView>().RPC("DropItemRPC", RpcTarget.All, slotIndex, dropPosition, PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }

        /// <summary>
        /// 아이템 획득 (네트워크)
        /// </summary>
        public void PickupItem(int itemUID, int count)
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                GetComponent<PhotonView>().RPC("PickupItemRPC", RpcTarget.All, itemUID, count, PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }

        #endregion

        #region INetworkOptimizable 구현

        private bool isOptimized = false;
        private NetworkPerformanceStats performanceStats;

        public void ApplyOptimizationSettings(NetworkOptimizationSettings settings)
        {
            if (settings == null) return;

            syncInterval = settings.statsSyncInterval;
            isOptimized = true;

            performanceStats = new NetworkPerformanceStats
            {
                managerName = "NetworkItemManager",
                syncInterval = syncInterval,
                rpcCount = 0,
                batchCount = 0,
                networkTraffic = 0f,
                isOptimized = true,
                lastUpdateTime = Time.time
            };
        }

        public void SetSyncInterval(float interval)
        {
            syncInterval = Mathf.Max(0.1f, interval);
        }

        public void SetInterpolation(bool enable, float speed = 6f)
        {
            // NetworkItemManager는 보간이 필요하지 않음
        }

        public void SetLOD(bool enable, float radius = 50f)
        {
            // NetworkItemManager는 LOD가 필요하지 않음
        }

        public void SetBatchSize(int batchSize)
        {
            // NetworkItemManager는 배치가 필요하지 않음
        }

        public void SetNetworkQuality(NetworkQuality quality)
        {
            switch (quality)
            {
                case NetworkQuality.Low:
                    syncInterval = 2.0f;
                    break;
                case NetworkQuality.Medium:
                    syncInterval = 1.0f;
                    break;
                case NetworkQuality.High:
                    syncInterval = 0.5f;
                    break;
            }
        }

        public bool IsOptimized()
        {
            return isOptimized;
        }

        public NetworkPerformanceStats GetPerformanceStats()
        {
            performanceStats.lastUpdateTime = Time.time;
            return performanceStats;
        }


        #endregion
    }
}
