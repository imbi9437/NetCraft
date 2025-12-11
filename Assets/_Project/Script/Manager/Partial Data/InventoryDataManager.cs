using _Project.Script.Character.Network;
using _Project.Script.Character.Player;
using _Project.Script.Core;
using _Project.Script.EventStruct;
using _Project.Script.Generic;
using _Project.Script.Items;
using _Project.Script.Items.Feature;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace _Project.Script.Manager
{
    //Inventory 관련 기능들만 분리된 DataManager
    public partial class DataManager
    {
	    private Inventory LocalInventory => localPlayerData.inventory;
	    private Equipment LocalEquipment => localPlayerData.equippedItems;
	    
	    public bool TryAddItem(ItemData data, int count, out bool isDestroyObject)
	    {
			print($"localPlayerData is null? {localPlayerData==null}, localPlayerData.inventory is null? {localPlayerData.inventory==null}");
		    bool result = LocalInventory.TryAddItem(data, count, out int remain);
		    isDestroyObject = remain <= 0;
		    return result;
	    }
	    public bool TryAddItem(ItemInstance instance, out bool isDestroyObject) => TryAddItem(instance.itemData, instance.count, out isDestroyObject);

		/// <summary>
		/// 아이템을 인벤토리에 가능한 만큼 추가하고, 실제로 추가된 수량을 반환하는 메소드
		/// 복수의 아이템을 한번에 받고 싶을때 사용
		/// </summary>
		public int TryAddItemAndGetAddedCount(ItemInstance instance)
		{
			if (instance == null || instance.count <= 0)
				return 0;

			int originalCount = instance.count;//넣고자 하는 개수
			int remain;//남은 개수
			LocalInventory.TryAddItem(instance.itemData, instance.count, out remain);

			int added = originalCount - remain;//실제로 넣은 개수
			return added;
		}

		/// <summary>
		/// 인벤토리 슬롯을 모두 뒤져서 item에 해당하는 아이템을 제거 ItemInstance기반
		/// </summary>
		/// <param name="item"></param>
		public void RemoveItem(ItemInstance item)
        {
			// TODO : 아이템 장비시 인벤토리에서 아이템 제거 or 드랍
			if (item == null) return;
			Inventory inventory = localPlayerData.inventory;
			 int remaining = item.count;

			for(int i = 0; i < inventory.itemSlot.Length && remaining>0; i++)
			{
				var slot = inventory.itemSlot[i];
				if (slot == null) continue;
				if(ReferenceEquals(slot.itemData, item.itemData))
				{
					int removeCount = Mathf.Min(slot.count, remaining);
					inventory.RemoveItem(i,removeCount);
					remaining -= removeCount;
				}
			}
        }

		/// <summary>
		/// 조합시 uid기반으로 같은 아이템을 찾아내어 레시피에 제시된 조건에 맞는 개수만큼 제거하는 함수
		/// 사용할시 CraftRecipeData에는 List<CraftRecipeElement>가 result와 ingredients 두개가 있으므로 유의해서 사용.
		/// 사실상 ingredients로 고정
		/// </summary>
		/// <param name="ingredients"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void RemoveItems(List<CraftRecipeElement> ingredients)
		{
			var inventory = localPlayerData.inventory;
			if (inventory == null) return;

			//매개변수로 받은 List<CraftRecipeElement>의, item의, uid를 기반으로 인벤토리로부터 재료를 제거
			foreach (var ingredient in ingredients)
			{
				int uid = ingredient.item.uid;
				int remaining = ingredient.count;

				for (int i = 0; i < inventory.itemSlot.Length && remaining > 0; i++)
				{
					var slot = inventory.itemSlot[i];
					if (slot == null || slot.itemData == null || slot.itemData.uid != uid) continue;

					int removeCount = Mathf.Min(slot.count, remaining);
					inventory.RemoveItem(i, removeCount);
					remaining -= removeCount;
				}

				if (remaining > 0)
					throw new InvalidOperationException($"재료 제거 실패: 아이템 UID {uid} 부족");
			}
		}
		public void SwapItem(int from, int to) => localPlayerData.inventory.SwapItem(from, to);

		/// <summary>
		/// 장비장착 메소드 
		/// </summary>
		/// <param name="index"></param>
		public void EquipItem(int index)
        {
	        var item = localPlayerData.inventory.itemSlot[index];
	        if (item == null) return;

			//RemoveItem실행시 원본데이터가 지워지는 문제가 발생해 Clone으로 적용
			var clonedItem = item.Clone();

			if (!clonedItem.TryGetFeatureParam(out EquipParam equipParam)) return;
				UnequipItem(equipParam.type);

	        bool result = localPlayerData.equippedItems.TryEquipItem(clonedItem, out ItemInstance resultItem);
	        
	        if (result == false) return;

			//인벤토리에 기존장비 넣고
	        if (resultItem != null) TryAddItem(resultItem.itemData, resultItem.count, out bool _);
			//있던 것은 제거
	        RemoveItem(item);

			if (clonedItem.TryGetFeature<EquipFeature>(out var feature))
			{
				feature.Use(clonedItem, equipParam);
			}

			Debug.Log($"현재 speed {localPlayerData.speed} ");
			//Debug.Log($"현재 hp {localPlayerData.health} ");
		}

        public void UnequipItem(EquipmentType type)
        {
            bool result = localPlayerData.equippedItems.TryUnequipItem(type, out ItemInstance resultItem);
            
            if (result == false) return;
            TryAddItem(resultItem.itemData, resultItem.count, out bool _);
            
			if(resultItem.TryGetFeature<EquipFeature>(out var feature)&& resultItem.TryGetFeatureParam<EquipParam>(out var EquipParam))
			{
				feature.Remove(resultItem, EquipParam);
			}
			
			Debug.Log($"장비 해제시 스탯 : {localPlayerData.speed}");
        }

        public bool TryUseItem(int index)
        {
	        if (LocalInventory == null) return false;

	        bool used = LocalInventory.TryUseItem(index);
	        return used;
        }

    }
}
