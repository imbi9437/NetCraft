using _Project.Script.Items;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeUI : MonoBehaviour
{

	[SerializeField] private Transform itemPosition;
	[SerializeField] private GameObject itemImagePrefab;

	/// <summary>
	/// 레시피 재료들을 보여주는 함수
	/// </summary>
	/// <param name="recipe"></param>
	/// <param name="station"></param>
	public void Init(CraftRecipeData recipe, CraftStation station)
	{
		foreach(var ingredient in recipe.ingredients)
		{
			GameObject GO = Instantiate(itemImagePrefab, itemPosition);
			Image image = GO.GetComponent<Image>();
			TextMeshProUGUI text = GO.GetComponentInChildren<TextMeshProUGUI>();
			image.sprite = ingredient.item.icon;
			text.text = ingredient.count.ToString();
		}
	}
}
