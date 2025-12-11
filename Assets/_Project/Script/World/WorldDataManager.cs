using UnityEngine;
using System.Collections.Generic;

namespace _Project.Script.World
{
    /// <summary>
    /// 월드 데이터 관리 전담 클래스
    /// 타일, 구조물, 리소스 데이터를 관리
    /// </summary>
    public class WorldDataManager
    {
        // 월드 상태 데이터
        private Dictionary<Vector3Int, WorldTileData> worldTiles = new Dictionary<Vector3Int, WorldTileData>();
        private Dictionary<int, NetworkStructure> networkStructures = new Dictionary<int, NetworkStructure>();
        private Dictionary<Vector3, ResourceNode> resourceNodes = new Dictionary<Vector3, ResourceNode>();

        // 생성된 구조물 GameObject 관리
        private Dictionary<int, GameObject> structureGameObjects = new Dictionary<int, GameObject>();

        // ID 관리
        private int nextStructureId = 1;

        #region 월드 초기화

        /// <summary>
        /// 월드 초기화
        /// </summary>
        public void InitializeWorld(int worldSize)
        {
            // 기본 월드 타일 생성
            for (int x = -worldSize / 2; x < worldSize / 2; x += 10)
            {
                for (int z = -worldSize / 2; z < worldSize / 2; z += 10)
                {
                    Vector3Int tilePos = new Vector3Int(x, 0, z);
                    worldTiles[tilePos] = new WorldTileData
                    {
                        position = tilePos,
                        tileType = TileType.Grass,
                        isOccupied = false,
                        structureId = -1
                    };
                }
            }

            Debug.Log($"[WorldDataManager] 월드 초기화 완료 - 타일 수: {worldTiles.Count}");
        }

        #endregion

        #region 구조물 관리

        /// <summary>
        /// 구조물 추가
        /// </summary>
        public int AddStructure(Vector3 position, Quaternion rotation, StructureType structureType, int ownerActorNumber)
        {
            int structureId = nextStructureId++;
            NetworkStructure structure = new NetworkStructure
            {
                id = structureId,
                position = position,
                rotation = rotation,
                structureType = structureType,
                health = 100f,
                isDestroyed = false,
                ownerActorNumber = ownerActorNumber
            };

            networkStructures[structureId] = structure;

            // 월드 타일 업데이트
            Vector3Int tilePos = new Vector3Int(Mathf.RoundToInt(position.x), 0, Mathf.RoundToInt(position.z));
            if (worldTiles.ContainsKey(tilePos))
            {
                var tile = worldTiles[tilePos];
                tile.isOccupied = true;
                tile.structureId = structureId;
                worldTiles[tilePos] = tile;
            }

            return structureId;
        }

        /// <summary>
        /// 구조물 제거
        /// </summary>
        public bool RemoveStructure(int structureId)
        {
            if (networkStructures.ContainsKey(structureId))
            {
                var structure = networkStructures[structureId];
                structure.isDestroyed = true;
                networkStructures[structureId] = structure;

                // 월드 타일 업데이트
                Vector3Int tilePos = new Vector3Int(Mathf.RoundToInt(structure.position.x), 0, Mathf.RoundToInt(structure.position.z));
                if (worldTiles.ContainsKey(tilePos))
                {
                    var tile = worldTiles[tilePos];
                    tile.isOccupied = false;
                    tile.structureId = -1;
                    worldTiles[tilePos] = tile;
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// 구조물 정보 가져오기
        /// </summary>
        public NetworkStructure? GetStructure(int structureId)
        {
            return networkStructures.TryGetValue(structureId, out var structure) ? structure : null;
        }

        /// <summary>
        /// 모든 구조물 가져오기
        /// </summary>
        public Dictionary<int, NetworkStructure> GetAllStructures()
        {
            return new Dictionary<int, NetworkStructure>(networkStructures);
        }

        /// <summary>
        /// 구조물 소유권 확인
        /// </summary>
        public bool CanDestroyStructure(int structureId, int actorNumber, bool isMasterClient)
        {
            if (!networkStructures.ContainsKey(structureId)) return false;

            var structure = networkStructures[structureId];
            return structure.ownerActorNumber == actorNumber || isMasterClient;
        }

        /// <summary>
        /// 플레이어의 구조물 목록 가져오기
        /// </summary>
        public List<int> GetPlayerStructures(int actorNumber)
        {
            List<int> playerStructures = new List<int>();

            foreach (var structure in networkStructures.Values)
            {
                if (structure.ownerActorNumber == actorNumber && !structure.isDestroyed)
                {
                    playerStructures.Add(structure.id);
                }
            }

            return playerStructures;
        }

        #endregion

        #region 리소스 관리

        /// <summary>
        /// 리소스 채집
        /// </summary>
        public bool HarvestResource(Vector3 position, int amount)
        {
            if (resourceNodes.ContainsKey(position))
            {
                var resource = resourceNodes[position];
                resource.amount -= amount;
                resource.amount = Mathf.Max(0, resource.amount);

                if (resource.amount <= 0)
                {
                    resource.isDepleted = true;
                }

                resourceNodes[position] = resource;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 리소스 재생성
        /// </summary>
        public void RegenerateResource(Vector3Int position, ResourceType resourceType, int amount)
        {
            ResourceNode resource = new ResourceNode
            {
                position = position,
                resourceType = resourceType,
                amount = amount,
                isDepleted = false
            };

            resourceNodes[position] = resource;
        }

        /// <summary>
        /// 리소스 노드 정보 가져오기
        /// </summary>
        public ResourceNode? GetResourceNode(Vector3Int position)
        {
            return resourceNodes.TryGetValue(position, out var resource) ? resource : null;
        }

        /// <summary>
        /// 모든 리소스 노드 가져오기
        /// </summary>
        public Dictionary<Vector3, ResourceNode> GetAllResourceNodes()
        {
            return new Dictionary<Vector3, ResourceNode>(resourceNodes);
        }

        #endregion

        #region 월드 타일 관리

        /// <summary>
        /// 월드 타일 정보 가져오기
        /// </summary>
        public WorldTileData? GetWorldTile(Vector3Int position)
        {
            return worldTiles.TryGetValue(position, out var tile) ? tile : null;
        }

        /// <summary>
        /// 위치 점유 여부 확인
        /// </summary>
        public bool IsPositionOccupied(Vector3 position)
        {
            Vector3Int tilePos = new Vector3Int(Mathf.RoundToInt(position.x), 0, Mathf.RoundToInt(position.z));

            if (worldTiles.ContainsKey(tilePos))
            {
                return worldTiles[tilePos].isOccupied;
            }

            return false;
        }

        /// <summary>
        /// 리소스 가용성 확인
        /// </summary>
        public bool IsResourceAvailable(Vector3Int position, int amount)
        {
            if (resourceNodes.ContainsKey(position))
            {
                var resource = resourceNodes[position];
                return !resource.isDepleted && resource.amount >= amount;
            }

            return false;
        }

        #endregion

        #region GameObject 관리

        /// <summary>
        /// 구조물 GameObject 추가
        /// </summary>
        public void AddStructureGameObject(int structureId, GameObject structureObject)
        {
            structureGameObjects[structureId] = structureObject;
        }

        /// <summary>
        /// 구조물 GameObject 제거
        /// </summary>
        public void RemoveStructureGameObject(int structureId)
        {
            if (structureGameObjects.ContainsKey(structureId))
            {
                structureGameObjects.Remove(structureId);
            }
        }

        /// <summary>
        /// 구조물 GameObject 가져오기
        /// </summary>
        public GameObject GetStructureGameObject(int structureId)
        {
            return structureGameObjects.TryGetValue(structureId, out var obj) ? obj : null;
        }

        #endregion

        #region 통계 및 정보

        /// <summary>
        /// 월드 통계 정보
        /// </summary>
        public WorldStatistics GetWorldStatistics()
        {
            int activeCount = 0;
            foreach (var structure in networkStructures.Values)
            {
                if (!structure.isDestroyed)
                    activeCount++;
            }

            return new WorldStatistics
            {
                tileCount = worldTiles.Count,
                structureCount = networkStructures.Count,
                resourceCount = resourceNodes.Count,
                activeStructureCount = activeCount
            };
        }

        #endregion
    }

    /// <summary>
    /// 월드 통계 정보 구조체
    /// </summary>
    [System.Serializable]
    public struct WorldStatistics
    {
        public int tileCount;
        public int structureCount;
        public int resourceCount;
        public int activeStructureCount;
    }
}
