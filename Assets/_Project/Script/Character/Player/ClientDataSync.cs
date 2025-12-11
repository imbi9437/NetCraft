using System.Collections;
using System.Collections.Generic;
using _Project.Script.Character.Player;
using Photon.Pun;
using UnityEngine;

public class ClientDataSync : MonoBehaviour, IPunObservable
{
    [SerializeField] private PlayerNetworkHandler handler;
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(handler.PlayerData.attack);
            stream.SendNext(handler.PlayerData.defense);
            stream.SendNext(handler.PlayerData.speed);
            
            stream.SendNext(handler.transform.position.x);
            stream.SendNext(handler.transform.position.y);
            stream.SendNext(handler.transform.position.z);
            
            stream.SendNext(handler.transform.rotation.x);
            stream.SendNext(handler.transform.rotation.y);
            stream.SendNext(handler.transform.rotation.z);
            stream.SendNext(handler.transform.rotation.w);
        }
        else
        {
            float attack = (float)stream.ReceiveNext();
            float defence = (float)stream.ReceiveNext();
            float speed = (float)stream.ReceiveNext();
            
            float posX = (float)stream.ReceiveNext();
            float posY = (float)stream.ReceiveNext();
            float posZ = (float)stream.ReceiveNext();
            
            float rotX = (float)stream.ReceiveNext();
            float rotY = (float)stream.ReceiveNext();
            float rotZ = (float)stream.ReceiveNext();
            float rotW = (float)stream.ReceiveNext();

            if (handler?.PlayerData != null)
            {
                handler.PlayerData.attack = attack;
                handler.PlayerData.defense = defence;
                handler.PlayerData.speed = speed;

                handler.PlayerData.posX = posX;
                handler.PlayerData.posY = posY;
                handler.PlayerData.posZ = posZ;

                handler.PlayerData.rotX = rotX;
                handler.PlayerData.rotY = rotY;
                handler.PlayerData.rotZ = rotZ;
                handler.PlayerData.rotW = rotW;
            }
        }
    }
}
