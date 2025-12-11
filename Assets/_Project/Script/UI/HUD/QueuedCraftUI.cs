using _Project.Script.Items;
using _Project.Script.Manager;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QueuedCraftUI : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private GameObject itemImagePrefab;
	[SerializeField] private Transform itemImagePrefabSlots;

	[SerializeField] private Button cancelButton;
	/// <summary>
	/// CraftingManager의 RequestCraft로 호출되며 UI이미지와 버튼에 기능을 할당함
	/// </summary>
	/// <param name="recipe"></param>
	/// <param name="jobs"></param>
	/// <param name="station"></param>
	public void Init(CraftRecipeData recipe, CraftingJobs jobs, CraftStation station)
	{
		// 예약 작업목록 프리팹 생성 및 이미지 부여
		foreach(var result in recipe.results)
		{
			GameObject GO = Instantiate(itemImagePrefab, itemImagePrefabSlots);
			Image image = GO.GetComponent<Image>();
			image.sprite = result.item.icon;
		}
		//취소버튼에 예약취소능력 부여 취소시 아이템 인벤토리로 돌아감
		cancelButton.onClick.RemoveAllListeners();
		cancelButton.onClick.AddListener(() =>
		{
			print($"예약취소버튼 눌림");
			CraftingManager.Instance.CancelQueuedCraft(recipe, station, this.gameObject);
		});
		cancelButton.gameObject.SetActive(false);
	}

	#region 취소버튼 활/비활
	public void OnPointerEnter(PointerEventData eventData)
	{
		cancelButton.gameObject.SetActive(true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		cancelButton.gameObject.SetActive(false);
	}
	#endregion
}
