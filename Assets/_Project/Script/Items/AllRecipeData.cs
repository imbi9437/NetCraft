using _Project.Script.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


[CreateAssetMenu(fileName ="AllRecipeData", menuName ="ScriptableObjects/RecipeData/AllRecipeData")]
public class AllRecipeData : ScriptableObject
{
	public List<CraftRecipeData> allRecipe;

	/// <summary>
	/// CraftStation이 참조를 하여 같은 타입의 레시피를 모조리 가져오는 함수
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public List<CraftRecipeData> GetCraftRecipeDatas(CraftStationType type)
	{
		List<CraftRecipeData> filteredRecipes = new List<CraftRecipeData>();
		foreach(var craftRecipeData in allRecipe)
		{
			if(craftRecipeData.stationType == type)
			{
				filteredRecipes.Add(craftRecipeData);
			}

		}
		return filteredRecipes;
	}

}
