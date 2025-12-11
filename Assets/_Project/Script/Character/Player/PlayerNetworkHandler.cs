using System;
using UnityEngine;
using Photon.Pun;
using _Project.Script.EventStruct;
using _Project.Script.Manager;
using _Project.Script.Data;
using _Project.Script.Generic;

namespace _Project.Script.Character.Player
{
    /// <summary>
    /// 플레이어 네트워크 처리 클래스
    /// 네트워크 동기화 및 이벤트 처리만 담당
    /// 
    /// 최적화된 네트워크 구조:
    /// - 실시간 데이터 (위치, 회전, 이동상태): SendNext() 사용
    /// - 상태 정보 (체력, 이름, 팀): CustomProperties 사용
    /// - 이벤트 (공격, 스킬): RPC 사용
    /// </summary>
    public class PlayerNetworkHandler : MonoBehaviourPun, IPunOwnershipCallbacks
    {
        [Header("네트워크 설정")]
        [SerializeField] private PhotonView photonView;

        [Header("플레이어 데이터")]
        [SerializeField] private PlayerData playerData;

        [SerializeField] private PhotonView masterView;

        public PlayerData PlayerData => playerData;
        
        private void Awake()
        {
            photonView ??= GetComponent<PhotonView>();

            if (photonView.IsMine)
                playerData = DataManager.Instance.localPlayerData;
            else if (PhotonNetwork.IsMasterClient)
            {
                string uid = photonView.Owner.CustomProperties["uid"].ToString();
                playerData = DataManager.Instance.localUserData.worldData.playerData[uid];
            }
        }

        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (masterView.IsMine == false)
                {
                    masterView.RequestOwnership();
                }
            }
            else
            {
                if (masterView.IsMine) masterView.TransferOwnership(PhotonNetwork.MasterClient);
            }
        }

        public void OnOwnershipRequest(PhotonView targetView, Photon.Realtime.Player requestingPlayer)
        {
            if (targetView.IsMine && requestingPlayer.IsMasterClient) targetView.TransferOwnership(requestingPlayer);
        }

        public void OnOwnershipTransfered(PhotonView targetView, Photon.Realtime.Player previousOwner)
        {
            
        }

        public void OnOwnershipTransferFailed(PhotonView targetView, Photon.Realtime.Player senderOfFailedRequest)
        {
            
        }
    }
}
