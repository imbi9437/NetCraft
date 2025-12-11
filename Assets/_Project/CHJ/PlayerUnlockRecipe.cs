using _Project.Script.Items;
using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 해금하면 여기다가 추가해서 여길기반으로 조건검사
/// PlayerData가 UnlockRecipe를 갖고 있어야 하나
/// 해금할때 DataManager.Instance.localPlayerData.unlockedRecipes.Unlock(UnlockRecipe.IronSword);
/// </summary>
[Serializable]
public class PlayerUnlockRecipe
{
	[SerializeField]
	private List<UnlockRecipe> unlockedList = new List<UnlockRecipe>(); // 저장용

	[NonSerialized]
	private HashSet<UnlockRecipe> unlockedSet = new HashSet<UnlockRecipe>(); // 런타임용

	public void Initialize()
	{
		unlockedSet = new HashSet<UnlockRecipe>(unlockedList);
	}

	public bool IsUnlocked(UnlockRecipe requirement)
	{
		return requirement == UnlockRecipe.None || unlockedSet.Contains(requirement);
	}

	public void Unlock(UnlockRecipe recipe)
	{
		if (unlockedSet.Add(recipe))//해시셋이라 알아서 추가된다는 지피티의 말...
		{
			unlockedList.Add(recipe);
		}
	}

	public void Lock(UnlockRecipe recipe)
	{
		if (unlockedSet.Remove(recipe))
		{
			unlockedList.Remove(recipe);
		}
	}
}

