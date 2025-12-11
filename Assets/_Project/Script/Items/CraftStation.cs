using _Project.Script.Items;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 작업대(Campfire, Workbench등)의 컴포넌트로 사용할 예정
/// 주변에 작업대가 있는지를 확인하는 함수를 DataManager혹은 CraftingManager에게 달고 CancraftOrCook에 조건으로 추가
/// 작업대는 모든 플레이어에게 공유되어야하므로 생성시 Photon Instantiate를 사용토록하자
/// </summary>
public class CraftStation : MonoBehaviour
{
	//AllRecipeData는 모든 레시피데이터를 가지고 있으며 얘한테서 같은 타입의 레시피를 가져옴
	[SerializeField] AllRecipeData allRecipeData;

	[Header("인스펙터에서 타입지정하면 알아서 레시피를 가짐")]
	public CraftStationType craftStationType;
	public List<CraftRecipeData> recipeDatas;

	[Header("만들어진 아이템들")]
	public List<ItemInstance> craftedItems = new();
    //우클릭 누르면 StoredItemPanelManager에게 List<ItemInstance>를 넘김

    [Header("지금 만들고 있는 아이템")]
	public Transform craftQueuePosition;
	public GameObject craftResultPrefab;

	[Header("예약 걸어놓은 아이템")]
	public List<GameObject> queuedCraftUIObjects = new();
	public GameObject queuedCraftUIPrefab;

	bool isOn = false;
	private void Awake()
	{
		//모든 레시피 데이터중 station이 가지고 있는 타입과 같은 레시피만 가져오기
		recipeDatas = allRecipeData.GetCraftRecipeDatas(craftStationType);
	}

	/// <summary>
	/// craftedItems에 ItemInstace를 저장하되 ItemInstace의 개수제한을 따라가며 총 슬롯은 10개로 제한됨
	/// </summary>
	/// <param name="newItem"></param>
	/// <returns></returns>
	public bool AddCraftItems(ItemInstance newItem)
	{
		int toAdd  = newItem.count;
		int maxStack = newItem.GetMaxStack();
		foreach (var existing in craftedItems)
		{
			if (ReferenceEquals(existing.itemData, newItem.itemData))
			{
				int spaceLeft = maxStack - existing.count;

				if(spaceLeft > 0)
				{
					int adding = Mathf.Min(spaceLeft, toAdd);
				existing.count += newItem.count;
					toAdd -= adding;

					if (toAdd <= 0)
					{
						StoredItemPanelManager.Instance.RefreshUI();
						return true;
					}
				}
			}
		}
		//10개 제한
		while(toAdd > 0)
		{
			if (craftedItems.Count >= 10)
			{
				Debug.Log("작업대에 공간이 부족합니다.");
				return false;
			}
			int adding = Mathf.Min(maxStack, toAdd);
			craftedItems.Add(new ItemInstance
			{
				itemData = newItem.itemData,
				count = adding
			});
			toAdd -= adding;
		}
		//UI갱신
		StoredItemPanelManager.Instance.RefreshUI();
		return true;
	}
    private void Update()
    {
		if (Input.GetMouseButtonDown(1))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hit))
			{
				if (hit.collider.gameObject == this.gameObject)
				{
					StoredItemPanelManager.Instance.SetStationStoredItem(craftedItems);
					isOn = !isOn;
					StoredItemPanelManager.Instance.StoredItemPanelOnOff(isOn);
					Debug.Log($"작업대 우클릭 감지됨!{gameObject.name}");
				}
			}
		}
	}
}
