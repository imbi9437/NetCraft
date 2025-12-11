using _Project.Script.EventStruct;
using _Project.Script.Items;
using _Project.Script.Items.Feature;
using _Project.Script.Manager;
using Newtonsoft.Json;
using System;

namespace _Project.Script.Generic
{
    public enum EquipmentType
    {
        Head,
        Body,
        Hand
    }
    
    [Serializable]
    public class Equipment
    {
        // TODO : Dictionary로 변경할지 고민
        public ItemInstance[] items;
        
        private Action<ItemEvents.EquipmentChangedEvent> _onEquipmentChanged;

        public Equipment()
        {
            int count = Enum.GetValues(typeof(EquipmentType)).Length;
            items = new ItemInstance[count];
        }
        
        /// <summary> 아이템 장착 </summary>
        /// <param name="item">장착할 아이템</param>
        /// <param name="result">아이템 장착 시 기존 장착 아이템</param>
        /// <returns>장착 성공 여부 반환</returns>
        public bool TryEquipItem(ItemInstance item, out ItemInstance result)
        {
            result = null;

            if (item.TryGetFeatureParam(out EquipParam param) == false) return false;
            
            int index = (int)param.type;
            result = items[index];
            items[index] = item;

            
            EventHub.Instance.RaiseEvent(new ItemEvents.EquipmentChangedEvent(param.type, item));

            _onEquipmentChanged?.Invoke(new ItemEvents.EquipmentChangedEvent(param.type, item));
            return true;
        }
        
        /// <summary> 아이템 장착 해제 </summary>
        /// <param name="type">장착 해제 아이템 타입</param>
        /// <param name="result">해제하는 아이템</param>
        /// <returns>장착 해제 성공 여부 반환</returns>
        public bool TryUnequipItem(EquipmentType type, out ItemInstance result)
        {
            result = items[(int)type];
            
            if (result == null) return false;
            
            items[(int)type] = null;

            //
			EventHub.Instance.RaiseEvent(new ItemEvents.EquipmentChangedEvent(type, null));

			_onEquipmentChanged?.Invoke(new ItemEvents.EquipmentChangedEvent(type, null));
            return true;
        }
    }
}
