using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Script.Items;
using _Project.Script.Manager;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 테스트용이였으나 여러 기능추가로 인해 실전용으로 바뀔지도
/// </summary>
public class CraftingUIController : MonoBehaviour
{
	public static CraftingUIController Instance;
	private void Awake()
	{
		Instance = this;

	}

	[Header("테스트용 아이템 추가")]
    public ItemData[] data;
    public int count = 1;
	int currentRemoveSlot = 0;

	public CraftStation currentStation= null;

	[Header("작업대관련")]
	[SerializeField] private Transform stationParent;
	[SerializeField] private GameObject stationButtonPrefab;

	[Header("갖고있는 레시피 스크롤 렉트로 띄우기")]
	[SerializeField] private GameObject recipeParent;
	[SerializeField] private Transform content;
	[SerializeField] private GameObject recipeButtonPrefab;

	/// <summary>
	/// CraftStation이 가지고 있는 레시피를 띄우는 함수
	/// TODO : 재료가 충분하지 않으면 버튼을 비활성화하거나 애초에 재료량을 표시하거나
	/// </summary>
	public void SummonCurrentStationRecipeList()
	{
		foreach(Transform child in content)
		{
			Destroy(child.gameObject);
		}
		if (currentStation == null) return;

		foreach(var recipe in currentStation.recipeDatas)
		{
			GameObject recipeGO = Instantiate(recipeButtonPrefab, content);
			Button recipeButton = recipeGO.GetComponent<Button>();
			TextMeshProUGUI text = recipeGO.GetComponentInChildren<TextMeshProUGUI>();
			text.text = recipe.name;
			RecipeUI recipeUI = recipeGO.GetComponent<RecipeUI>();
			recipeUI.Init(recipe,currentStation);
			
			recipeButton.onClick.AddListener(()=> 
			{ 
				DataManager.Instance.Craft(recipe, currentStation);
			});
		}
	}

	/// <summary>
	/// 근처의 Station을 UI로 생성하기
	/// 어떤 작업대에서 일을 수행할지를 구분하기 위한
	/// </summary>
	public void SummonNearbyStationList()
	{
		foreach(Transform child in stationParent)
		{
			Destroy(child.gameObject);
		}

		foreach (var station in CraftingManager.Instance.nearByCraftStation)
		{ 
			GameObject stationGO = Instantiate(stationButtonPrefab, stationParent); 
			Button button = stationGO.GetComponent<Button>();
			TextMeshProUGUI text = stationGO.GetComponentInChildren<TextMeshProUGUI>();
			text.text = station.craftStationType.ToString();
			button.onClick.AddListener(() =>
			{   
				currentStation = null;
				currentStation = station;
				recipeParent.gameObject.SetActive(true);
				SummonCurrentStationRecipeList();
			}
			);
		}
	}

	public void Update()
    {
		//아이템 추가 테스트
		if (Input.GetKeyDown(KeyCode.Q))
		{
			for (int i = 0; i < data.Length; i++)
			{
				DataManager.Instance.TryAddItem(data[i], count, out bool isDestroyObject);
			}
		}

		//아이템 제거 테스트
		if(Input.GetKeyDown(KeyCode.E))
		{
			var inventory = DataManager.Instance.localPlayerData.inventory;
			while(currentRemoveSlot< inventory.itemSlot.Length && inventory.itemSlot[currentRemoveSlot] == null)
			{
				currentRemoveSlot++;
			}
			if (currentRemoveSlot >= inventory.itemSlot.Length)
			{
				currentRemoveSlot = 0;
			}
			
			var slotItem = inventory.itemSlot[currentRemoveSlot];
			if(slotItem != null)
			{
				DataManager.Instance.RemoveItem(new ItemInstance { itemData = slotItem.itemData, count = 1});
				if(inventory.itemSlot[currentRemoveSlot] == null || inventory.itemSlot[currentRemoveSlot].count<=0)
				{
					currentRemoveSlot++;
				}
			}
		}
	}
}
