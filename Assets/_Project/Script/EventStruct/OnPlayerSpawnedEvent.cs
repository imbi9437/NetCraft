using _Project.Script.Interface;
using UnityEngine;

namespace _Project.Script.Manager
{
    /// <summary>
    /// 플레이어 스폰 완료 이벤트
    /// NetworkPlayer에서 PlayerSpawner로 PhotonView 정보 전달
    /// </summary>
    public struct OnPlayerSpawnedEvent : IEvent
    {
        public bool isMine;
        public int ownerActorNumber;
        public int localActorNumber;

        public OnPlayerSpawnedEvent(bool isMine, int ownerActorNumber, int localActorNumber)
        {
            this.isMine = isMine;
            this.ownerActorNumber = ownerActorNumber;
            this.localActorNumber = localActorNumber;
        }
    }
}
