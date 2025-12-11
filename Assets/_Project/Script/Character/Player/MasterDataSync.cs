using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Script.Character.Player;
using _Project.Script.EventStruct;
using _Project.Script.Manager;
using Photon.Pun;
using UnityEngine;

public class MasterDataSync : MonoBehaviour
{
    [SerializeField] private PhotonView masterView;
    [SerializeField] private PlayerNetworkHandler handler;

    private void Awake()
    {
        if (PhotonNetwork.IsMasterClient == false) return;
        
        EventHub.Instance.RegisterEvent<EnvironmentEvents.RequestDecreaseEvent>(RequestDecreasePlayerData);
    }

    private void OnDestroy()
    {
        if (PhotonNetwork.IsMasterClient == false) return;
        
        EventHub.Instance?.UnregisterEvent<EnvironmentEvents.RequestDecreaseEvent>(RequestDecreasePlayerData);
    }

    private void RequestDecreasePlayerData(EnvironmentEvents.RequestDecreaseEvent evt)
    {
        if (handler.photonView.IsMine == false)
            masterView.RPC(nameof(EnvironmentEvent), handler.photonView.Owner);
        else
        {
            DataManager.Instance.ChangePlayerData();
        }
    }

    [PunRPC]
    public void EnvironmentEvent(PhotonMessageInfo info)
    {
        DataManager.Instance.ChangePlayerData();
    }
}
