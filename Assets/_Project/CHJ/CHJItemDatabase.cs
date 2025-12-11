using _Project.Script.Generic;
using System.Collections;
using System.Collections.Generic;
using _Project.Script.Items;
using UnityEngine;
public class CHJItemDatabase : MonoSingleton<CHJItemDatabase>
{
	[SerializeField]
	private List<ItemData> allItems;
	
	Dictionary<int, ItemData> itemLookup;
	protected override void Awake()
	{
		base.Awake();

		allItems = new List<ItemData>(Resources.LoadAll<ItemData>("ItemData"));//Resources에 있는 모든 아이템을 참조해놓기
		itemLookup = new Dictionary<int, ItemData>();
		foreach(ItemData item in allItems)
		{
			if(item != null)
			{
				itemLookup[item.uid] = item;
			}
		}
	}

	public ItemData GetItemDataById(int uid)
	{
		itemLookup.TryGetValue(uid, out ItemData item);
		return item;
	}


}
