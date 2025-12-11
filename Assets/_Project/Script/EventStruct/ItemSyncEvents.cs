using _Project.Script.Interface;
using _Project.Script.Items;
using UnityEngine;

namespace _Project.Script.EventStruct
{
    /// <summary>
    /// 인벤토리 동기화 이벤트
    /// </summary>
    public struct OnInventorySyncEvent : IEvent
    {
        public int actorNumber;
        public ItemInstance[] inventory;
        public bool isLocalPlayer;
    }

    /// <summary>
    /// 장착 아이템 동기화 이벤트
    /// </summary>
    public struct OnEquippedItemsSyncEvent : IEvent
    {
        public int actorNumber;
        public ItemInstance[] equippedItems;
    }

    /// <summary>
    /// 아이템 사용 이벤트
    /// </summary>
    public struct OnItemUsedEvent : IEvent
    {
        public int actorNumber;
        public int slotIndex;
        public ItemInstance item;
    }

    /// <summary>
    /// 아이템 드롭 이벤트
    /// </summary>
    public struct OnItemDroppedEvent : IEvent
    {
        public int actorNumber;
        public int slotIndex;
        public ItemInstance item;
        public Vector3 dropPosition;
    }

    /// <summary>
    /// 아이템 획득 이벤트
    /// </summary>
    public struct OnItemPickedUpEvent : IEvent
    {
        public int actorNumber;
        public int itemUID;
        public int count;
    }

    /// <summary>
    /// 아이템 교환 이벤트
    /// </summary>
    public struct OnItemTradedEvent : IEvent
    {
        public int fromActorNumber;
        public int toActorNumber;
        public int itemUID;
        public int count;
    }

}
