using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

namespace _Project.Script.World
{
    /// <summary>
    /// 리소스 관리 전담 클래스
    /// 리소스 채집, 재생성, 상태 관리
    /// </summary>
    public class ResourceManager
    {
        private WorldDataManager worldDataManager;

        public ResourceManager(WorldDataManager dataManager)
        {
            worldDataManager = dataManager;
        }

        #region 리소스 채집/재생성

        /// <summary>
        /// 리소스 채집 (네트워크)
        /// </summary>
        public void HarvestResource(Vector3Int position, int amount, PhotonView photonView)
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                // Vector3Int를 Vector3로 변환하여 전송
                Vector3 pos = new Vector3(position.x, position.y, position.z);
                photonView.RPC("HarvestResourceRPC", RpcTarget.All,
                    pos, amount, PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }

        /// <summary>
        /// 리소스 재생성 (네트워크)
        /// </summary>
        public void RegenerateResource(Vector3Int position, ResourceType resourceType, int amount, PhotonView photonView)
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                photonView.RPC("RegenerateResourceRPC", RpcTarget.All,
                    position, resourceType, amount);
            }
        }

        /// <summary>
        /// 리소스 채집 RPC 처리
        /// </summary>
        public bool ProcessHarvestResourceRPC(Vector3 position, int amount, int actorNumber)
        {
            // 플레이어 존재 여부 검증
            var player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            if (player == null)
            {
                Debug.LogWarning($"[ResourceManager] 존재하지 않는 플레이어 {actorNumber}의 리소스 채집 시도");
                return false;
            }

            // 리소스 가용성 확인
            Vector3Int pos = new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));
            if (!worldDataManager.IsResourceAvailable(pos, amount))
            {
                Debug.LogWarning($"[ResourceManager] 리소스 부족 또는 고갈: 위치 {position}, 요청량 {amount}");
                return false;
            }

            // 리소스 채집
            bool success = worldDataManager.HarvestResource(position, amount);
            if (success)
            {
                Debug.Log($"[ResourceManager] 리소스 채집 - 위치: {position}, 수량: {amount}");
            }

            return success;
        }

        /// <summary>
        /// 리소스 재생성 RPC 처리
        /// </summary>
        public void ProcessRegenerateResourceRPC(Vector3Int position, ResourceType resourceType, int amount)
        {
            worldDataManager.RegenerateResource(position, resourceType, amount);
            Debug.Log($"[ResourceManager] 리소스 재생성 - 위치: {position}, 타입: {resourceType}, 수량: {amount}");
        }

        #endregion

        #region 리소스 정보 조회

        /// <summary>
        /// 리소스 노드 정보 가져오기
        /// </summary>
        public ResourceNode? GetResourceNode(Vector3Int position)
        {
            return worldDataManager.GetResourceNode(position);
        }

        /// <summary>
        /// 모든 리소스 노드 가져오기
        /// </summary>
        public Dictionary<Vector3, ResourceNode> GetAllResourceNodes()
        {
            return worldDataManager.GetAllResourceNodes();
        }

        /// <summary>
        /// 리소스 가용성 확인
        /// </summary>
        public bool IsResourceAvailable(Vector3Int position, int amount)
        {
            return worldDataManager.IsResourceAvailable(position, amount);
        }

        /// <summary>
        /// 특정 타입의 리소스 노드 찾기
        /// </summary>
        public List<Vector3Int> FindResourceNodesByType(ResourceType resourceType)
        {
            List<Vector3Int> foundNodes = new List<Vector3Int>();
            var allResources = worldDataManager.GetAllResourceNodes();

            foreach (var kvp in allResources)
            {
                if (kvp.Value.resourceType == resourceType && !kvp.Value.isDepleted)
                {
                    foundNodes.Add(kvp.Value.position);
                }
            }

            return foundNodes;
        }

        /// <summary>
        /// 특정 반경 내 리소스 노드 찾기
        /// </summary>
        public List<Vector3Int> FindResourceNodesInRadius(Vector3 center, float radius, ResourceType? resourceType = null)
        {
            List<Vector3Int> foundNodes = new List<Vector3Int>();
            var allResources = worldDataManager.GetAllResourceNodes();

            foreach (var kvp in allResources)
            {
                float distance = Vector3.Distance(center, kvp.Key);
                if (distance <= radius)
                {
                    if (resourceType == null || kvp.Value.resourceType == resourceType)
                    {
                        if (!kvp.Value.isDepleted)
                        {
                            foundNodes.Add(kvp.Value.position);
                        }
                    }
                }
            }

            return foundNodes;
        }

        #endregion

        #region 리소스 통계

        /// <summary>
        /// 리소스 타입별 통계
        /// </summary>
        public Dictionary<ResourceType, int> GetResourceStatistics()
        {
            Dictionary<ResourceType, int> stats = new Dictionary<ResourceType, int>();
            var allResources = worldDataManager.GetAllResourceNodes();

            foreach (var kvp in allResources)
            {
                ResourceType type = kvp.Value.resourceType;
                if (!stats.ContainsKey(type))
                {
                    stats[type] = 0;
                }
                stats[type]++;
            }

            return stats;
        }

        /// <summary>
        /// 리소스 총량 통계
        /// </summary>
        public Dictionary<ResourceType, int> GetResourceAmountStatistics()
        {
            Dictionary<ResourceType, int> amounts = new Dictionary<ResourceType, int>();
            var allResources = worldDataManager.GetAllResourceNodes();

            foreach (var kvp in allResources)
            {
                ResourceType type = kvp.Value.resourceType;
                if (!amounts.ContainsKey(type))
                {
                    amounts[type] = 0;
                }
                amounts[type] += kvp.Value.amount;
            }

            return amounts;
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
                Debug.LogWarning($"[ResourceManager] 존재하지 않는 플레이어 {actorNumber}의 {action} 시도");
                return false;
            }

            return true;
        }

        #endregion
    }
}
