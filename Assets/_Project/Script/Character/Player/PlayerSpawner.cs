using System.Collections.Generic;
using _Project.Script.Character.Player;
using _Project.Script.EventStruct;
using _Project.Script.Manager;
using _Project.Script.Generic;
using Photon.Pun.UtilityScripts;
using Photon.Pun;
using UnityEngine;

namespace _Project.Script.Character.Player
{
    /// <summary>
    /// 플레이어 스폰 관리 시스템
    /// 방 입장 시 자동으로 플레이어 생성 및 관리
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        private string playerPrefabName = "Player";
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnYOffset = 1f;

        [Header("Player Management")]
        [SerializeField] private Transform playerParent;

        // 스폰된 플레이어들 관리 (통합된 PlayerStateMachine 사용)
        private PlayerStateMachine localPlayer;

        private Dictionary<int, PlayerStateMachine> spawnedPlayers = new();

        protected void Awake()
        {
            // 플레이어 부모 오브젝트 설정
            if (playerParent == null)
            {
                GameObject playerParentObj = new GameObject("Players");
                playerParent = playerParentObj.transform;
            }

            // 스폰 포인트가 없으면 기본 스폰 포인트 생성
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                CreateDefaultSpawnPoints();
            }
        }

        private void Start()
        {
            // 이벤트 구독
            EventHub.Instance.RegisterEvent<OnPlayerSpawnedEvent>(OnPlayerSpawned);

            // PUN2 이벤트 구독
            if (PhotonNetwork.IsConnected)
            {
                SpawnLocalPlayer();
            }
        }

        #region 플레이어 스폰

        /// <summary>
        /// 로컬 플레이어 스폰
        /// </summary>
        private void SpawnLocalPlayer()
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                // 이미 스폰된 플레이어가 있는지 확인
                if (localPlayer != null)
                {
                    Debug.LogWarning("[PlayerSpawner] 로컬 플레이어가 이미 존재합니다.");
                    return;
                }

                // 스폰 포인트 선택
                Vector3 spawnPosition = GetSpawnPosition();

                // 플레이어 생성 (PhotonNetwork.Instantiate 사용)
                GameObject playerObj = PhotonNetwork.Instantiate(
                    playerPrefabName,
                    spawnPosition,
                    Quaternion.identity
                );

                CHJPlayerStatusHandler handler = playerObj.GetComponent<CHJPlayerStatusHandler>();
                //handler.ApplyStatus(FirebaseManager.Instance.currentStatus); //게임매니저가 건네준 임시저장소에서 데이터 가져오기 이론상 가능
                if (playerObj != null)
                {
                    // PlayerStateMachine 컴포넌트 가져오기 (통합된 시스템)
                    localPlayer = playerObj.GetComponentInChildren<PlayerStateMachine>();

                    if (localPlayer != null)
                    {
                        // 플레이어 부모 설정
                        playerObj.transform.SetParent(playerParent);

                        // 딕셔너리에 추가
                        spawnedPlayers[PhotonNetwork.LocalPlayer.ActorNumber] = localPlayer;

                        Debug.Log($"[PlayerSpawner] 로컬 플레이어 스폰 완료 - ActorNumber: {PhotonNetwork.LocalPlayer.ActorNumber}");
                    }
                    else
                    {
                        Debug.LogError("[PlayerSpawner] PlayerStateMachine 컴포넌트를 찾을 수 없습니다.");
                        PhotonNetwork.Destroy(playerObj);
                    }
                }
                else
                {
                    Debug.LogError("[PlayerSpawner] 플레이어 생성 실패");
                }
            }
            else
            {
                Debug.LogWarning("[PlayerSpawner] 네트워크에 연결되지 않았거나 방에 입장하지 않았습니다.");
            }
        }

        /// <summary>
        /// 다른 플레이어 스폰 (PhotonNetwork.Instantiate로 자동 처리됨)
        /// </summary>
        public void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            Debug.Log($"[PlayerSpawner] 플레이어 입장: {newPlayer.NickName} (ActorNumber: {newPlayer.ActorNumber})");

            // 이미 스폰된 플레이어인지 확인
            if (spawnedPlayers.ContainsKey(newPlayer.ActorNumber))
            {
                Debug.LogWarning($"[PlayerSpawner] 플레이어 {newPlayer.ActorNumber}가 이미 존재합니다.");
                return;
            }

            // PlayerStateMachine을 찾기
            PlayerStateMachine[] allPlayers = FindObjectsOfType<PlayerStateMachine>();
            foreach (PlayerStateMachine player in allPlayers)
            {
                if (player.GetComponent<PhotonView>().Owner.ActorNumber == newPlayer.ActorNumber)
                {
                    spawnedPlayers[newPlayer.ActorNumber] = player;
                    player.transform.SetParent(playerParent);
                    Debug.Log($"[PlayerSpawner] 플레이어 {newPlayer.ActorNumber} 등록 완료");
                    break;
                }
            }
        }

        /// <summary>
        /// 플레이어 제거
        /// </summary>
        public void OnPlayerLeftRoom(Photon.Realtime.Player leftPlayer)
        {
            Debug.Log($"[PlayerSpawner] 플레이어 퇴장: {leftPlayer.NickName} (ActorNumber: {leftPlayer.ActorNumber})");

            if (spawnedPlayers.ContainsKey(leftPlayer.ActorNumber))
            {
                PlayerStateMachine player = spawnedPlayers[leftPlayer.ActorNumber];

                // 딕셔너리에서 제거
                spawnedPlayers.Remove(leftPlayer.ActorNumber);

                // 로컬 플레이어인지 확인
                if (player == localPlayer)
                {
                    localPlayer = null;
                }

                // 오브젝트 파괴는 PhotonNetwork가 자동으로 처리
                Debug.Log($"[PlayerSpawner] 플레이어 {leftPlayer.ActorNumber} 제거 완료");
            }
        }

        #endregion

        #region 스폰 포인트 관리

        /// <summary>
        /// 스폰 포인트 가져오기
        /// </summary>
        private Vector3 GetSpawnPosition()
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                // 기존 플레이어들과 겹치지 않는 위치 찾기
                for (int i = 0; i < spawnPoints.Length; i++)
                {
                    Vector3 spawnPos = spawnPoints[i].position + Vector3.up * spawnYOffset;

                    // 다른 플레이어와 겹치지 않는지 확인
                    if (!IsPositionOccupied(spawnPos))
                    {
                        return spawnPos;
                    }
                }

                // 모든 스폰 포인트가 점유된 경우 첫 번째 포인트 사용
                return spawnPoints[0].position + Vector3.up * spawnYOffset;
            }

            // 기본 위치 (원점)
            return Vector3.up * spawnYOffset;
        }

        /// <summary>
        /// 위치가 점유되었는지 확인
        /// </summary>
        private bool IsPositionOccupied(Vector3 position)
        {
            float checkRadius = 2f;
            Collider[] colliders = Physics.OverlapSphere(position, checkRadius);

            foreach (Collider col in colliders)
            {
                if (col.GetComponent<PlayerStateMachine>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 기본 스폰 포인트 생성
        /// </summary>
        private void CreateDefaultSpawnPoints()
        {
            Debug.Log("[PlayerSpawner] 기본 스폰 포인트 생성");

            spawnPoints = new Transform[4];

            // 4방향으로 스폰 포인트 생성
            Vector3[] positions = new Vector3[]
            {
                new Vector3(0, 0, 0),      // 중앙
                new Vector3(5, 0, 0),      // 동쪽
                new Vector3(-5, 0, 0),     // 서쪽
                new Vector3(0, 0, 5)       // 북쪽
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
                spawnPoint.transform.position = positions[i];
                spawnPoints[i] = spawnPoint.transform;
            }
        }

        #endregion

        #region 공개 API

        /// <summary>
        /// 로컬 플레이어 가져오기 (통합된 시스템)
        /// </summary>
        public PlayerStateMachine GetLocalPlayer()
        {
            return localPlayer;
        }

        /// <summary>
        /// 특정 플레이어 가져오기 (통합된 시스템)
        /// </summary>
        public PlayerStateMachine GetPlayer(int actorNumber)
        {
            return spawnedPlayers.ContainsKey(actorNumber) ? spawnedPlayers[actorNumber] : null;
        }

        /// <summary>
        /// 모든 플레이어 가져오기 (통합된 시스템)
        /// </summary>
        public System.Collections.Generic.Dictionary<int, PlayerStateMachine> GetAllPlayers()
        {
            return new System.Collections.Generic.Dictionary<int, PlayerStateMachine>(spawnedPlayers);
        }

        /// <summary>
        /// 플레이어 수 가져오기
        /// </summary>
        public int GetPlayerCount()
        {
            return spawnedPlayers.Count;
        }

        #endregion

        #region 이벤트 처리

        /// <summary>
        /// 플레이어 스폰 완료 이벤트 처리
        /// </summary>
        private void OnPlayerSpawned(OnPlayerSpawnedEvent spawnEvent)
        {
            Debug.Log($"[PlayerSpawner] 플레이어 스폰 이벤트 수신 - IsMine: {spawnEvent.isMine}, Owner: {spawnEvent.ownerActorNumber}, LocalActorNumber: {spawnEvent.localActorNumber}");

            // 로컬 플레이어인 경우에만 입력 활성화
            if (spawnEvent.isMine)
            {
                EnableLocalPlayerInput();
            }
        }

        #endregion

        #region 입력 관리

        /// <summary>
        /// 로컬 플레이어 입력 활성화 (이벤트 기반)
        /// </summary>
        private void EnableLocalPlayerInput()
        {
            // PlayerInput 활성화 이벤트 발생
            EventHub.Instance.RaiseEvent(new OnPlayerInputEnabledEvent(true));
            Debug.Log("[PlayerSpawner] 로컬 플레이어 입력 활성화 이벤트 발생");
        }

        #endregion

        #region 디버그

        private void OnDrawGizmos()
        {
            // 스폰 포인트 표시
            if (spawnPoints != null)
            {
                Gizmos.color = Color.yellow;
                foreach (Transform spawnPoint in spawnPoints)
                {
                    if (spawnPoint != null)
                    {
                        Gizmos.DrawWireSphere(spawnPoint.position + Vector3.up * spawnYOffset, 1f);
                    }
                }
            }

            // 플레이어 위치 표시
            Gizmos.color = Color.green;
            foreach (var player in spawnedPlayers.Values)
            {
                if (player != null)
                {
                    Gizmos.DrawWireCube(player.transform.position + Vector3.up, Vector3.one * 0.5f);
                }
            }
        }

        #endregion

        private void OnDestroy()
        {
            // 이벤트 해제
            if (EventHub.Instance != null)
            {
                EventHub.Instance.UnregisterEvent<OnPlayerSpawnedEvent>(OnPlayerSpawned);
            }
        }
    }
}
