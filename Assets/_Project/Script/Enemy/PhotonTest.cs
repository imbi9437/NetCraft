using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkAutoJoiner : MonoBehaviourPunCallbacks
{
    public SpawnObject[] spawnObject;
    // 1. Awake 시 자동 연결 시작
    private void Awake()
    {
        // Photon 설정을 사용하여 서버에 연결을 시도합니다.
        // 연결이 성공하면 OnConnectedToMaster()가 호출됩니다.
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("🌐 Photon 서버에 자동 연결 시도...");
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.AutomaticallySyncScene = true; // 씬 동기화 옵션 활성화
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            for (int i = 0; i < spawnObject.Length; i++)
            {
                spawnObject[i].Spawn();
            }
        }
    }

    // 2. 서버 연결 성공 시 로비 진입 시도
    public override void OnConnectedToMaster()
    {
        Debug.Log("✅ 서버 연결 성공! 로비 진입 시도...");
        // 로비에 진입합니다. 로비 진입 후 OnJoinedLobby()가 호출됩니다.
        PhotonNetwork.JoinLobby();
    }

    // 3. 로비 진입 성공 시 방 진입 시도
    public override void OnJoinedLobby()
    {
        Debug.Log("✅ 로비 진입 성공! 무작위 방 진입 시도...");
        // 무작위 방에 진입을 시도합니다.
        PhotonNetwork.JoinRandomRoom();
    }

    // 4. 무작위 방 진입 실패 시 (방이 없는 경우) 새로운 방 생성
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"⚠️ 방 진입 실패 ({message}). 새로운 방 생성...");
        
        // 방 생성 옵션을 설정합니다. (테스트용이므로 최대 인원 4명으로 설정)
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 4,
            IsVisible = true,
            IsOpen = true
        };
        
        // "TestRoom"이라는 이름으로 방을 생성합니다.
        PhotonNetwork.CreateRoom("TestRoom", roomOptions);
    }

    // 5. 방 진입(혹은 생성) 성공 시 최종 확인
    public override void OnJoinedRoom()
    {
        Debug.Log($"🎉 방 진입 성공! 현재 방: {PhotonNetwork.CurrentRoom.Name}");
        Debug.Log($"👥 현재 플레이어 수: {PhotonNetwork.CurrentRoom.PlayerCount}");
        
        // 여기서 Enemy 스포너 테스트를 위한 로직을 시작할 수 있습니다.
        
        // 이제 Enemy나 SpawnObject를 가진 씬에서 테스트를 진행하면 됩니다.
    }
}