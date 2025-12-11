using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

namespace _Project.Script.World
{
    /// <summary>
    /// 월드 네트워크 동기화 전담 클래스
    /// PUN2 동기화, RPC 처리, Custom Properties 관리
    /// </summary>
    public class WorldNetworkSync
    {
        private WorldDataManager worldDataManager;
        private PhotonView photonView;

        public WorldNetworkSync(WorldDataManager dataManager, PhotonView view)
        {
            worldDataManager = dataManager;
            photonView = view;
        }

        #region PUN2 동기화

        /// <summary>
        /// 월드 데이터 전송 (실시간 데이터만)
        /// </summary>
        public void SendWorldData(PhotonStream stream)
        {
            var structures = worldDataManager.GetAllStructures();
            var resources = worldDataManager.GetAllResourceNodes();

            // 구조물 실시간 데이터만 전송 (위치, 회전)
            stream.SendNext(structures.Count);
            foreach (var structure in structures.Values)
            {
                stream.SendNext(structure.id);
                stream.SendNext(structure.position);
                stream.SendNext(structure.rotation);
                // structureType, health, isDestroyed는 CustomProperties로 이동
            }

            // 리소스 노드 실시간 데이터만 전송 (위치)
            stream.SendNext(resources.Count);
            foreach (var resource in resources.Values)
            {
                stream.SendNext(resource.position);
                // resourceType, amount, isDepleted는 CustomProperties로 이동
            }
        }

        /// <summary>
        /// 월드 데이터 수신
        /// </summary>
        public void ReceiveWorldData(PhotonStream stream)
        {
            // 구조물 데이터 수신
            int structureCount = (int)stream.ReceiveNext();
            for (int i = 0; i < structureCount; i++)
            {
                int id = (int)stream.ReceiveNext();
                Vector3 position = (Vector3)stream.ReceiveNext();
                Quaternion rotation = (Quaternion)stream.ReceiveNext();
                StructureType structureType = (StructureType)stream.ReceiveNext();
                float health = (float)stream.ReceiveNext();
                bool isDestroyed = (bool)stream.ReceiveNext();

                NetworkStructure structure = new NetworkStructure
                {
                    id = id,
                    position = position,
                    rotation = rotation,
                    structureType = structureType,
                    health = health,
                    isDestroyed = isDestroyed
                };

                // 월드 데이터 매니저에 추가 (임시)
                // 실제로는 월드 데이터 매니저의 메서드를 호출해야 함
            }

            // 리소스 노드 데이터 수신
            int resourceCount = (int)stream.ReceiveNext();
            for (int i = 0; i < resourceCount; i++)
            {
                Vector3Int position = (Vector3Int)stream.ReceiveNext();
                ResourceType resourceType = (ResourceType)stream.ReceiveNext();
                int amount = (int)stream.ReceiveNext();
                bool isDepleted = (bool)stream.ReceiveNext();

                ResourceNode resource = new ResourceNode
                {
                    position = position,
                    resourceType = resourceType,
                    amount = amount,
                    isDepleted = isDepleted
                };

                // 월드 데이터 매니저에 추가 (임시)
                // 실제로는 월드 데이터 매니저의 메서드를 호출해야 함
            }
        }

        #endregion

        #region RPC 처리

        /// <summary>
        /// 구조물 건설 RPC
        /// </summary>
        [PunRPC]
        public void BuildStructureRPC(Vector3 position, Quaternion rotation, StructureType structureType, int actorNumber)
        {
            // 이 메서드는 NetworkWorldManager에서 호출됨
            Debug.Log($"[WorldNetworkSync] 구조물 건설 RPC - 위치: {position}, 타입: {structureType}");
        }

        /// <summary>
        /// 구조물 파괴 RPC
        /// </summary>
        [PunRPC]
        public void DestroyStructureRPC(int structureId, int actorNumber)
        {
            // 이 메서드는 NetworkWorldManager에서 호출됨
            Debug.Log($"[WorldNetworkSync] 구조물 파괴 RPC - ID: {structureId}");
        }

        /// <summary>
        /// 리소스 채집 RPC
        /// </summary>
        [PunRPC]
        public void HarvestResourceRPC(Vector3 position, int amount, int actorNumber)
        {
            // 이 메서드는 NetworkWorldManager에서 호출됨
            Debug.Log($"[WorldNetworkSync] 리소스 채집 RPC - 위치: {position}, 수량: {amount}");
        }

        /// <summary>
        /// 리소스 재생성 RPC
        /// </summary>
        [PunRPC]
        public void RegenerateResourceRPC(Vector3Int position, ResourceType resourceType, int amount)
        {
            // 이 메서드는 NetworkWorldManager에서 호출됨
            Debug.Log($"[WorldNetworkSync] 리소스 재생성 RPC - 위치: {position}, 타입: {resourceType}");
        }

        /// <summary>
        /// 월드 상태 동기화 RPC
        /// </summary>
        [PunRPC]
        public void SyncWorldStateRPC(int tileCount, int structureCount, int resourceCount)
        {
            Debug.Log($"[WorldNetworkSync] 월드 상태 동기화 - 타일: {tileCount}, 구조물: {structureCount}, 리소스: {resourceCount}");
        }

        #endregion

        #region Custom Properties 관리

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

        /// <summary>
        /// 룸 프로퍼티 변경 시 호출
        /// </summary>
        public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            foreach (var key in propertiesThatChanged.Keys)
            {
                string keyStr = key.ToString();

                if (keyStr.StartsWith("Structure_"))
                {
                    ProcessStructurePropertyUpdate(keyStr, propertiesThatChanged[key]);
                }
                else if (keyStr.StartsWith("Resource_"))
                {
                    ProcessResourcePropertyUpdate(keyStr, propertiesThatChanged[key]);
                }
            }
        }

        /// <summary>
        /// 구조물 프로퍼티 업데이트 처리
        /// </summary>
        private void ProcessStructurePropertyUpdate(string keyStr, object value)
        {
            string[] parts = keyStr.Split('_');
            if (parts.Length >= 3)
            {
                int structureId = int.Parse(parts[1]);
                string property = parts[2];

                var structure = worldDataManager.GetStructure(structureId);
                if (structure.HasValue)
                {
                    var updatedStructure = structure.Value;

                    switch (property)
                    {
                        case "Type":
                            updatedStructure.structureType = (StructureType)value;
                            break;
                        case "Health":
                            updatedStructure.health = (float)value;
                            break;
                        case "Destroyed":
                            updatedStructure.isDestroyed = (bool)value;
                            break;
                    }

                    // 월드 데이터 매니저에 업데이트 (구현 필요)
                    Debug.Log($"[WorldNetworkSync] 구조물 {structureId} 프로퍼티 업데이트: {property} = {value}");
                }
            }
        }

        /// <summary>
        /// 리소스 프로퍼티 업데이트 처리
        /// </summary>
        private void ProcessResourcePropertyUpdate(string keyStr, object value)
        {
            string[] parts = keyStr.Split('_');
            if (parts.Length >= 3)
            {
                int resourceId = int.Parse(parts[1]);
                string property = parts[2];

                // 리소스 ID를 Vector3Int로 변환 (구현 필요)
                Vector3Int position = new Vector3Int(resourceId, 0, 0); // 임시 로직

                var resource = worldDataManager.GetResourceNode(position);
                if (resource.HasValue)
                {
                    var updatedResource = resource.Value;

                    switch (property)
                    {
                        case "Type":
                            updatedResource.resourceType = (ResourceType)value;
                            break;
                        case "Amount":
                            updatedResource.amount = (int)(float)value;
                            break;
                        case "Depleted":
                            updatedResource.isDepleted = (bool)value;
                            break;
                    }

                    // 월드 데이터 매니저에 업데이트 (구현 필요)
                    Debug.Log($"[WorldNetworkSync] 리소스 {resourceId} 프로퍼티 업데이트: {property} = {value}");
                }
            }
        }

        #endregion

        #region 월드 상태 동기화

        /// <summary>
        /// 모든 클라이언트에게 월드 상태 스냅샷 전송
        /// </summary>
        public void SyncWorldStateToAllClients()
        {
            var stats = worldDataManager.GetWorldStatistics();

            // 월드 상태를 새로운 MasterClient가 모든 클라이언트에게 전송
            photonView.RPC("SyncWorldStateRPC", RpcTarget.All,
                stats.tileCount, stats.structureCount, stats.resourceCount);
        }

        #endregion
    }
}
