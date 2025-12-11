using _Project.Script.Generic;
using _Project.Script.Items;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using static _Project.Script.EventStruct.ItemEvents;


namespace _Project.Script.Manager
{
	
	public class CraftingManager : MonoSingleton<CraftingManager>
	{
		//상태머신 + update로 기능확장
		//작업큐를 만들어서 조합을 실행하면 작업큐에 넣은뒤 currentJob으로 실행
		//매 프레임 작업리스트의 남은 시간을 갱신하면서 시간이 되면 작업큐에서 제거 후 다음 작업 실행

		//근처 작업대 리스트
		//CraftingSensor로부터 추가,제거됨
		public List<CraftStation> nearByCraftStation = new();
		//예약작업들
		private Dictionary<CraftStation, Queue<CraftRecipeData>> stationJobQueue = new();
		//현재 Station을 key로 받는 진행중인 작업들
		private Dictionary<CraftStation, CraftingJobs> currentJobs = new();

		/// <summary>
		/// 조합실행시 시간지연후 조합된 아이템 추가해줌
		/// </summary>
		private void Update()
		{
			//Sensor(플레이어)가 근처에 있어야 작업
			foreach(var station in nearByCraftStation)
			{
				if (station.craftStationType.RequiresSensor() == false)
				{
					continue;
				}
				HandleStationCrafting(station);
			}
			//Sensor(플레이어)가 근처에 없어도 작업
			foreach (var kvp in stationJobQueue)
			{
				CraftStation station = kvp.Key;
				if (station.craftStationType.RequiresSensor()) continue;
				HandleStationCrafting(station);
			}
		}

		void HandleStationCrafting(CraftStation station)
		{
				if(currentJobs.TryGetValue(station, out var job))
				{
					if (job.isCancel)
					{
						RefundIngredients(job);
						currentJobs.Remove(station);
						return;
					}
					job.remainingTime -= Time.deltaTime;
					if(job.remainingTime <= 0)
					{
						CraftResult(job.recipeData, station);

						foreach(var result in job.recipeData.results)
						{
							EventHub.Instance.RaiseEvent(new CraftSuccessEvent(job.recipeData, new ItemInstance
							{
								itemData = result.item,
								count = result.count,
							}));
						}
						currentJobs.Remove(station);
					}
				}
				else if (stationJobQueue.TryGetValue(station, out var queue) && queue.Count > 0)
				{
					StartNextJob(station, queue.Dequeue());
				}
		}



		/// <summary>
		/// 검증로직
		/// 레시피를 매개변수로 받아 인벤토리를 검사하여 레시피에서 요구한 uid와 count가 있는 지를 검사
		/// </summary>
		/// <param name="recipe"></param>
		/// <returns></returns>

		public bool CanCraft(CraftRecipeData recipe, CraftStation station)
		{
			var inventory = DataManager.Instance.localPlayerData.inventory;
			if (inventory == null || station.craftStationType!=recipe.stationType) return false;

			//작업대 검사 로직 필요

			// 인벤토리 아이템 개수 캐싱
			Dictionary<int, int> itemCounts = new();
			//uid가 있는지를 먼저 검사한다음
			foreach (var slot in inventory.itemSlot)
			{
				if (slot == null || slot.itemData == null) continue;
				int uid = slot.itemData.uid;
				if (!itemCounts.ContainsKey(uid)) itemCounts[uid] = 0;
				itemCounts[uid] += slot.count;
			}
			//uid의 count가 레시피가 요구하는 ingredients의 count만큼 있는지 검사
			foreach (var ingredient in recipe.ingredients)
			{
				int uid = ingredient.item.uid;
				if (!itemCounts.TryGetValue(uid, out int haveCount) || haveCount < ingredient.count)
					return false;
			}

			return true;
		}

		/// <summary>
		/// 조합 요청. CanCraft조건 검사후 조합작업 추가 시간지연이 걸리므로 Update에서 실행
		/// </summary>
		public void RequestCraft(CraftRecipeData recipe, CraftStation station)
		{
			if (!CanCraft(recipe, station))
			{
				Debug.LogWarning("재료 부족으로 조합 불가");
				return;
			}
			try
			{
				// 재료 제거
				DataManager.Instance.RemoveItems(recipe.ingredients);
				//작업목록에 넣기
				//craftQueue.Enqueue((recipe, station));
				if (!stationJobQueue.ContainsKey(station))
				{
					stationJobQueue[station] = new Queue<CraftRecipeData>();
				}
				stationJobQueue[station].Enqueue(recipe);

				//UI에 띄우기
				GameObject queuedUI = Instantiate(station.queuedCraftUIPrefab, station.craftQueuePosition);
				queuedUI.GetComponent<QueuedCraftUI>().Init(recipe, null, station);
				station.queuedCraftUIObjects.Add(queuedUI);
				//작업실행시도
				//TryNextCraft();
			}

			catch (Exception e)
			{
				print($"조합 실패{e}");
			}
		}

		/// <summary>
		/// 작업 진행하는 함수
		/// </summary>
		/// <param name="station"></param>
		/// <param name="recipe"></param>
		private void StartNextJob(CraftStation station, CraftRecipeData recipe)
		{
			//실행할때 뭔가 남아있는 예약이 있으면 없애고
			if (station.queuedCraftUIObjects.Count > 0)
			{
				Destroy(station.queuedCraftUIObjects[0]);
				station.queuedCraftUIObjects.RemoveAt(0);
			}

			//새로운 작업을 만들어서
			var job = new CraftingJobs
			{
				recipeData = recipe,
				station = station,
				remainingTime = recipe.time,
			};
			//작업목록 Dictionary에 넣음
			currentJobs[station] = job;
			//UI관련
			GameObject resultGO = Instantiate(station.craftResultPrefab, station.craftQueuePosition);
			var resultUI = resultGO.GetComponent<CraftResultUI>();
			resultUI.Init(recipe, job);
		}

		/// <summary>
		/// 조합결과 아이템을 추가해줌
		/// </summary>
		/// <param name="recipe"></param>
		void CraftResult(CraftRecipeData recipe, CraftStation station)
		{
			try
			{
				//아이템 추가
				foreach (var result in recipe.results)
				{
					var itemInstance = new ItemInstance
					{
						itemData = result.item,
						count = result.count,
					};

					bool success = station.AddCraftItems(itemInstance);
					if (!success)
					{
						Debug.LogWarning("작업대 공간부족");
						RefundIngredients(new CraftingJobs
						{
							recipeData = recipe,
							station = station
						});
						return;
					}
					EventHub.Instance.RaiseEvent(new CraftSuccessEvent(recipe, itemInstance));
					
				}
				Debug.Log("조합 완료");
			}
			catch (Exception ex)
			{
				Debug.LogError($"조합 중 오류 발생: {ex.Message}");
			}
		}

		
		/// <summary>
		/// Queue(예약)작업 목록 취소시
		/// </summary>
		/// <param name="recipe"></param>
		public void CancelQueuedCraft(CraftRecipeData recipe, CraftStation station, GameObject ui)
		{
			if (!stationJobQueue.TryGetValue(station, out var queue)) return;

			var newQueue = new Queue<CraftRecipeData>();
			bool isCanceled = false;

			foreach (var r in queue)
			{
				if (!isCanceled && r == recipe)
				{
					if (ui != null)
					{
						//ui 오브젝트 없애고
						Destroy(ui);
						//예약목록 Queue에서 제거
						station.queuedCraftUIObjects.Remove(ui);
					}
					//재료 돌려주고
					RefundIngredients(new CraftingJobs { recipeData = r });
					isCanceled = true;
				}
				else
				{
					//제거된 Queue를 제외하고 새로 등록
					newQueue.Enqueue(r);
				}
			}
			//그대로 Dictionary에도 등록
			stationJobQueue[station] = newQueue;
		}

		/// <summary>
		/// 재료 인벤토리로 돌려주는 함수
		/// </summary>
		/// <param name="jobs"></param>
		void RefundIngredients(CraftingJobs jobs)
		{
			foreach(var ingredient in jobs.recipeData.ingredients)
			{
				DataManager.Instance.TryAddItem(ingredient.item, ingredient.count, out _);
			}
		}
	}
}
