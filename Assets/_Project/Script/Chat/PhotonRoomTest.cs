using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhotonTable = ExitGames.Client.Photon.Hashtable;
public class PhotonRoomTest : MonoBehaviourPunCallbacks
{
    public static PhotonRoomTest Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    
    public override void OnCreatedRoom()
    {
        string MasterID = PhotonNetwork.LocalPlayer.UserId; //방을 만들때 유저id 저장 
        string MasterNickName = PhotonNetwork.LocalPlayer.NickName; //방을 만들때 유저닉네임 저장 
        
        
        PhotonTable chatMasterId = new PhotonTable();   //커스텀프로퍼티 chatMasterId에 닉네임과 ,아이디 저장 
        chatMasterId.Add("MasterID", MasterID);
        chatMasterId.Add("MasterNickname", MasterNickName);
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(chatMasterId);    //커스텀 프로퍼티 실행
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방에 참가하였습니다 ");

        // 1. 바로 커스텀 프로퍼티 확인 (이미 동기화된 경우)
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("MasterID") && 
            PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("MasterNickname"))
        {
            string masterID = (string)PhotonNetwork.CurrentRoom.CustomProperties["MasterID"];
            string masterNickname = (string)PhotonNetwork.CurrentRoom.CustomProperties["MasterNickname"];
        
            if (!string.IsNullOrEmpty(masterID) && !string.IsNullOrEmpty(masterNickname))
            {
                Debug.Log("커스텀 프로퍼티로 바로 채팅방 입장");
                //ChatManager.Instance.ChatStart(masterID);
                //ChatManager.Instance.chatPanel.roomNameLabel.text = $"{masterNickname}의 채널";
            }
        }
        else
        {
            Debug.Log("커스텀 프로퍼티가 아직 동기화되지 않았습니다. OnRoomPropertiesUpdate를 기다립니다.");
        }
    }


    public override void OnConnectedToMaster()
    {
        Debug.Log("서버 연결 완료! 로비에 진입합니다.");
        PhotonNetwork.JoinLobby();
        // 로비에 성공적으로 진입하면 OnJoinedLobby()가 호출됩니다. 여기서 아이디와 비밀번호를 통과시키면 JoinedLobby()를 호출시키면 될것 같습니다 
    }
    
    public override void OnJoinedLobby()
    {
        Debug.Log("로비 진입 완료! 이제 방을 만들거나 참여할 수 있습니다.");
        // 이제 방 만들기/참여 버튼을 활성화하세요.
    }

    public override void OnLeftRoom()
    {
        Debug.Log("방을 떠났습니다");
    }

    public override void OnLeftLobby()
    {
        Debug.Log("로비를 떠났습니다");
    }
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) //방에 들어왔을떄 실행하는 채널입장 함수 
    {
        // 이 로그가 출력되는지 확인
        Debug.Log("OnRoomPropertiesUpdate 콜백 호출됨");
        
        if (propertiesThatChanged.ContainsKey("MasterID") && propertiesThatChanged.ContainsKey("MasterNickname"))
        {
            string masterID = (string)propertiesThatChanged["MasterID"];
            string masterNickname = (string)propertiesThatChanged["MasterNickname"];
        
            if (!string.IsNullOrEmpty(masterID) && !string.IsNullOrEmpty(masterNickname))
            {
                Debug.Log("동기화된 프로퍼티로 채팅방 입장");
                //ChatManager.Instance.ChatStart(masterID);
                //ChatManager.Instance.chatPanel.roomNameLabel.text = $"{masterNickname}의 채널";
            }
        }
    }
}
