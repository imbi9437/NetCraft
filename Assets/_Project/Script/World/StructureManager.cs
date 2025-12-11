using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

namespace _Project.Script.World
{
    /// <summary>
    /// 구조물 관리 전담 클래스
    /// 구조물 생성, 파괴, 소유권 관리
    /// </summary>
    public class StructureManager
    {
        private WorldDataManager worldDataManager;
        private string[] structurePrefabNames;

        public StructureManager(WorldDataManager dataManager, string[] prefabNames)
        {
            worldDataManager = dataManager;
            structurePrefabNames = prefabNames;
        }

        #region 구조물 생성/파괴

        /// <summary>
        /// 구조물 건설 (네트워크)
        /// </summary>
        public void BuildStructure(Vector3 position, Quaternion rotation, StructureType structureType, int actorNumber, PhotonView photonView)
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                photonView.RPC("BuildStructureRPC", RpcTarget.All,
                    position, rotation, structureType, actorNumber);
            }
        }

        /// <summary>
        /// 구조물 파괴 (네트워크)
        /// </summary>
        public void DestroyStructure(int structureId, PhotonView photonView)
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                photonView.RPC("DestroyStructureRPC", RpcTarget.All,
                    structureId, PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }

        /// <summary>
        /// 구조물 건설 RPC 처리
        /// </summary>
        public int ProcessBuildStructureRPC(Vector3 position, Quaternion rotation, StructureType structureType, int actorNumber, int maxStructures)
        {
            // 플레이어 존재 여부 검증
            var player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            if (player == null)
            {
                Debug.LogWarning($"[StructureManager] 존재하지 않는 플레이어 {actorNumber}의 구조물 건설 시도");
                return -1;
            }

            // 최대 구조물 수 확인
            var stats = worldDataManager.GetWorldStatistics();
            if (stats.structureCount >= maxStructures)
            {
                Debug.LogWarning("[StructureManager] 최대 구조물 수 초과");
                return -1;
            }

            // 위치 검증: 중복 건설 방지
            if (worldDataManager.IsPositionOccupied(position))
            {
                Debug.LogWarning("[StructureManager] 해당 위치에 이미 구조물이 존재");
                return -1;
            }

            // 구조물 추가
            int structureId = worldDataManager.AddStructure(position, rotation, structureType, actorNumber);

            Debug.Log($"[StructureManager] 구조물 건설 - ID: {structureId}, 타입: {structureType}, 위치: {position}");
            return structureId;
        }

        /// <summary>
        /// 구조물 파괴 RPC 처리
        /// </summary>
        public bool ProcessDestroyStructureRPC(int structureId, int actorNumber)
        {
            if (!worldDataManager.CanDestroyStructure(structureId, actorNumber, PhotonNetwork.IsMasterClient))
            {
                var structure = worldDataManager.GetStructure(structureId);
                if (structure.HasValue)
                {
                    Debug.LogWarning($"[StructureManager] 구조물 {structureId}는 플레이어 {structure.Value.ownerActorNumber}의 소유물입니다. 파괴 권한이 없습니다.");
                }
                return false;
            }

            bool success = worldDataManager.RemoveStructure(structureId);
            if (success)
            {
                Debug.Log($"[StructureManager] 구조물 파괴 - ID: {structureId}");
            }
            return success;
        }

        #endregion

        #region 구조물 GameObject 관리

        /// <summary>
        /// 구조물 GameObject 생성 (PUN2 방식)
        /// </summary>
        public GameObject CreateStructureGameObject(int structureId, Vector3 position, Quaternion rotation, StructureType structureType)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // 구조물 타입에 따른 프리팹 이름 가져오기
                string prefabName = GetStructurePrefabName(structureType);

                // PUN2 네트워크 Instantiate (룸 오브젝트)
                GameObject structureObject = PhotonNetwork.InstantiateRoomObject(prefabName, position, rotation);

                if (structureObject != null)
                {
                    // 구조물 ID 설정
                    var identifier = structureObject.GetComponent<StructureIdentifier>();
                    if (identifier != null)
                    {
                        identifier.StructureId = structureId;
                    }

                    // 월드 데이터 매니저에 추가
                    worldDataManager.AddStructureGameObject(structureId, structureObject);

                    Debug.Log($"[StructureManager] 네트워크 구조물 생성: {prefabName} (ID: {structureId})");
                }
                else
                {
                    Debug.LogError($"[StructureManager] 구조물 생성 실패: {prefabName}");
                }

                return structureObject;
            }
            return null;
        }

        /// <summary>
        /// 구조물 GameObject 파괴
        /// </summary>
        public void DestroyStructureGameObject(int structureId)
        {
            GameObject structureObject = worldDataManager.GetStructureGameObject(structureId);
            if (structureObject != null)
            {
                // PUN2 네트워크 파괴
                PhotonNetwork.Destroy(structureObject);
            }

            worldDataManager.RemoveStructureGameObject(structureId);
            Debug.Log($"[StructureManager] 네트워크 구조물 파괴: ID {structureId}");
        }

        /// <summary>
        /// 구조물 타입에 따른 프리팹 이름 가져오기
        /// </summary>
        private string GetStructurePrefabName(StructureType structureType)
        {
            int typeIndex = (int)structureType;
            if (typeIndex >= 0 && typeIndex < structurePrefabNames.Length)
            {
                return structurePrefabNames[typeIndex];
            }

            // 기본값
            return "WallPrefab";
        }

        #endregion

        #region 소유권 관리

        /// <summary>
        /// 구조물 소유권 변경 (MasterClient만 가능)
        /// </summary>
        public void TransferStructureOwnership(int structureId, int newOwnerActorNumber)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[StructureManager] 구조물 소유권 변경은 MasterClient만 가능합니다.");
                return;
            }

            var structure = worldDataManager.GetStructure(structureId);
            if (structure.HasValue)
            {
                // 구조물 소유권 변경 로직 (구현 필요)
                Debug.Log($"[StructureManager] 구조물 {structureId} 소유권 변경: {newOwnerActorNumber}");
            }
        }

        #endregion

        #region 검증 로직

        /// <summary>
        /// 플레이어 권한 검증
        /// </summary>
        public bool ValidatePlayerAction(int actorNumber, string action)
        {
            // 플레이어가 룸에 존재하는지 확인
            var player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            if (player == null)
            {
                Debug.LogWarning($"[StructureManager] 존재하지 않는 플레이어 {actorNumber}의 {action} 시도");
                return false;
            }

            return true;
        }

        #endregion
    }
}
