using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

namespace _Project.Script.VoiceChat
{
    public class NetworkManagerVoice : MonoBehaviourPunCallbacks
    {
        [Header("Connection Settings")]
        private string gameVersion = "1.0";
        public byte maxPlayersPerRoom = 4;
        public string playerPrefabName = "Player"; // 플레이어 프리팹 이름을 여기서 관리

        [Header("Voice Settings")]
        // 모든 PlayerVoice 인스턴스를 관리하는 중앙 목록 (static으로 선언하여 어디서든 접근 가능)
        public static readonly List<PlayerVoice> AllPlayerVoices = new List<PlayerVoice>();

        // 싱글톤 인스턴스
        public static NetworkManagerVoice Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.AutomaticallySyncScene = true;
            ConnectToPhoton();
        }

        void ConnectToPhoton()
        {
            if (!PhotonNetwork.IsConnected)
            {
                Debug.Log("포톤 서버에 연결 중...");
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("✅ 포톤 마스터 서버 연결 성공!");
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            Debug.Log("✅ 로비 입장! 테스트 방에 참여합니다.");
            JoinTestRoom();
        }

        void JoinTestRoom()
        {
            RoomOptions options = new RoomOptions
            {
                MaxPlayers = maxPlayersPerRoom,
                IsVisible = true,
                IsOpen = true
            };
            PhotonNetwork.JoinOrCreateRoom("VoiceTestRoom", options, TypedLobby.Default);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log($"✅ 방 입장! 현재 플레이어 수: {PhotonNetwork.CurrentRoom.PlayerCount}");
            CreatePlayer();
        }

        void CreatePlayer()
        {
            Vector3 spawnPos = new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
            GameObject player = PhotonNetwork.Instantiate(playerPrefabName, spawnPos, Quaternion.identity);
            Debug.Log($"플레이어 생성: {player.name}");
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"새 플레이어 입장: {newPlayer.NickName}");
            // PlayerVoice가 스스로 목록에 등록하므로 여기서 별도 처리가 필요 없습니다.
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"플레이어 퇴장: {otherPlayer.NickName}");
            // PlayerVoice가 스스로 목록에서 제거하므로 여기서 별도 처리가 필요 없습니다.
        }
    }
}