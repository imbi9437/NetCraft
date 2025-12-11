using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using PhotonTable = ExitGames.Client.Photon.Hashtable;

[Obsolete]
public class JoinPanel : MonoBehaviour
{
    public InputField nameInput;
    public Button nameChangeButton;
    public Button connectButton;
    public Button createButton; //방만드는 버튼 
    public Button joinRoombutton; //있는 방에 들어가는 버튼 
    public Text logText;

    private void Awake()
    {
        nameInput.onValueChanged.AddListener(OnNameInputEdit);
        nameChangeButton.onClick.AddListener(nameChangeButtonClick);
        connectButton.onClick.AddListener(ConnectButtonClick);
        createButton.onClick.AddListener(CreateButtonClick);
        joinRoombutton.onClick.AddListener(JoinRoomButtonClick);
    }

    //닉네임 입력란에 문자를 입력할떄마다 특수문자는 완전히 입력이 안되도록 
    public void OnNameInputEdit(string input)
    {
        // nameInput.text = "", //InputField의 Text프로퍼티에 문자열을 할당하면 onValueChanged가 수행되므로 
        //OnValueChanged를 호출하지 않고 텍스트를 교체하는 함수를 사용해야한다 
        nameInput.SetTextWithoutNotify(input.ToValidString());
        logText.text = "";
    }

    public void nameChangeButtonClick()
    {
        string name = nameInput.text;
        //유요한 닉네임인지 검증 , 즉 미완성 한글이 포함되었는지 검사
        if (name.NicknameValidate())
        {
            //ChatManager.Instance.SetNickname(name);
            logText.text = "<color=green>닉네임변경 완료</color>";
        }
        else
        {
            logText.text = "<color=red>닉네임이 미완성 한글이 포함되어 있습니다 </color>";
        }
    }

    public void ConnectButtonClick()
    {
        PhotonNetwork.NickName = nameInput.text;
        PhotonNetwork.ConnectUsingSettings();
        //ChatManager.Instance.Connect();
        connectButton.interactable = false;
    }

    public void CreateButtonClick()
    {
        RoomOptions options = new RoomOptions()
        {
            MaxPlayers = 5
        };
        PhotonNetwork.CreateRoom(PhotonNetwork.LocalPlayer.UserId,options);
        createButton.interactable = false;
    }

    public void JoinRoomButtonClick()
    {
        // string masterID = (string)PhotonNetwork.CurrentRoom.CustomProperties["MasterID"];
        // PhotonNetwork.JoinRoom(masterID);
        
         
        if (PhotonNetwork.InLobby)
        {
            // 로비에 접속되어 있는 경우에만 방 참여를 시도합니다.
            PhotonNetwork.JoinRandomRoom();
            joinRoombutton.interactable = false;
        }
        else
        {
            // 로비에 접속되어 있지 않은 경우 오류 메시지 등을 표시합니다.
            logText.text = "<color=red>로비에 먼저 접속해주세요!</color>";
        }
    }

    public void ReturenJoinPanel()
    {
        createButton.interactable = true;
    }

    public void OnJoinedServer()
    {
        connectButton.GetComponentInChildren<Text>().text = "접속됨";
    }
}