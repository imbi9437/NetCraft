using _Project.Script.Items;
using _Project.Script.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftResultUI : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
{

	[SerializeField] private Slider remainTimeSlider;
	[SerializeField] private Transform itemImagePrefabParent;
	[SerializeField] private GameObject itemImagePrefab;

	[SerializeField] private Button cancelButton;

	CraftingJobs jobs;

	/// <summary>
	/// 현재 작업중인 조합을 UI에 생성 Recipe를 매개변수로 받아 count같은것도 표시 가능
	/// </summary>
	/// <param name="recipe"></param>
	/// <param name="jobs"></param>
	public void Init(CraftRecipeData recipe, CraftingJobs jobs)
	{
		this.jobs = jobs;

		remainTimeSlider.maxValue = recipe.time;
		foreach(var result in recipe.results)
		{
			GameObject resultGO = Instantiate(itemImagePrefab, itemImagePrefabParent);
			Image image = resultGO.GetComponent<Image>();
			image.sprite = result.item.icon;
		}
		cancelButton.onClick.RemoveAllListeners();
		cancelButton.onClick.AddListener(() => 
		{
			print($"작업취소버튼 눌림");
			jobs.isCancel = true;
			Destroy(gameObject);
		});
		cancelButton.gameObject.SetActive(false);
	}


	/// <summary>
	/// 남은 작업시간을 보여주고 끝난작업을 UI상에서 없애는 Update
	/// </summary>
	private void Update()
	{
		if (jobs == null)
		{
			Debug.LogWarning("CraftResultUI: jobs is null");
			return;
		}

		remainTimeSlider.value = jobs.remainingTime;
		if(jobs.remainingTime <= 0 )
		{
			Destroy(gameObject);
		}
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
