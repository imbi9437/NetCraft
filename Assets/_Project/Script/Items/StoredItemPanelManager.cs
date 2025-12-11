using _Project.Script.Items;
using _Project.Script.Manager;
using _Project.Script.UI.HUD;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoredItemPanelManager : MonoBehaviour
{
	public static StoredItemPanelManager Instance;

	[SerializeField] private GameObject stationStoredItemPanel;

	public StoredItemSlot[] itemSlots;
	public List<ItemInstance> stationStoredItem = new();
	private Button allTakesButton;



	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
			return;
		}
		//버튼으로 모든 아이템 추가
		itemSlots = stationStoredItemPanel.GetComponentsInChildren<StoredItemSlot>();
		allTakesButton = stationStoredItemPanel.GetComponentInChildren<Button>();
		allTakesButton.onClick.AddListener(() =>
		{
			if (stationStoredItem == null || stationStoredItem.Count == 0) return;

			List<ItemInstance> remainingItems = new();

			foreach (var item in stationStoredItem)
			{
				//인벤토리의 남은 칸만큼 아이템을 추가
				int added = DataManager.Instance.TryAddItemAndGetAddedCount(item);

				if (added < item.count)
				{
					remainingItems.Add(new ItemInstance
					{
						itemData = item.itemData,
						count = item.count - added
					});
				}
			}
			stationStoredItem = remainingItems;
			RefreshUI();
		});
	}

	/// <summary>
	/// 초기화 시 사용. UI갱신은 RefreshUI 사용 권장
	/// </summary>
	public void SetStationStoredItem(List<ItemInstance> items)
	{
		stationStoredItem = items;
		RefreshUI();
	}

	/// <summary>
	/// UI를 현재 리스트(stationStoredItem)에 맞춰 갱신
	/// </summary>
	public void RefreshUI()
	{
		for (int i = 0; i < itemSlots.Length; i++)
		{
			if (i < stationStoredItem.Count)
			{
				itemSlots[i].SetItem(stationStoredItem[i]);
			}
			else
			{
				itemSlots[i].SetItem(null);
			}
		}
	}

	/// <summary>
	/// 패널 껐다키기
	/// </summary>
	/// <param name="isOn"></param>
	public void StoredItemPanelOnOff(bool isOn)
	{
		stationStoredItemPanel.SetActive(isOn);
	}
}
