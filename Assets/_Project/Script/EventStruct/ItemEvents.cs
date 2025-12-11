using _Project.Script.Generic;
using _Project.Script.Interface;
using _Project.Script.Items;
using System.Collections.Generic;

namespace _Project.Script.EventStruct
{
    public static class ItemEvents
    {
        public struct InventoryChangedEvent : IEvent { }
        public struct InventorySlotChangedEvent : IEvent
        {
            public int index;
            public ItemInstance item;
        
            public InventorySlotChangedEvent(int index, ItemInstance item)
            {
                this.index = index;
                this.item = item;
            }
        }
        
        public struct EquipmentChangedEvent : IEvent
        {
            public EquipmentType type;
            public ItemInstance item;

            public EquipmentChangedEvent(EquipmentType type, ItemInstance item)
            {
                this.type = type;
                this.item = item;
            }
        }
		public struct CraftSuccessEvent : IEvent
		{
			public CraftRecipeData Recipe;
			public ItemInstance Results;
			public CraftSuccessEvent(CraftRecipeData recipe, ItemInstance results)
			{
				Recipe = recipe;
				Results = results;
			}
		}
		public struct StatChangeRequestEvent : IEvent
        {
            
        }

        public struct StatChangedEvent : IEvent
        {
            
        }
    }
}
