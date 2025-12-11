using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Voice.PUN;
using UnityEngine;

public class Test : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PhotonNetwork.ConnectUsingSettings();
            
            Debug.Log("asd");
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            PhotonNetwork.Disconnect();
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            PhotonNetwork.JoinRandomOrCreateRoom();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            PhotonNetwork.LeaveRoom();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(PhotonNetwork.IsConnected);
            Debug.Log(PhotonNetwork.IsMasterClient);
            //Debug.Log(PunVoiceClient.Instance.ClientState);
        }
    }
}
