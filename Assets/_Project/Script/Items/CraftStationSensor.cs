using _Project.Script.Items;
using _Project.Script.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 작업대를 감지하는 클래스 플레이어쪽에 합치는 것도 가능
/// </summary>
public class CraftStationSensor : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		var station = other.GetComponent<CraftStation>();
		if (station != null)
		{
			CraftingManager.Instance.nearByCraftStation.Add(station);
			CraftingUIController.Instance.SummonNearbyStationList();
			print($"CraftingManager 작업대 추가 : {station.craftStationType}");
			
		}
	}

	private void OnTriggerExit(Collider other)
	{
		var station= other.GetComponent<CraftStation>();
		if (station != null)
		{
			CraftingManager.Instance.nearByCraftStation.Remove(station);
			CraftingUIController.Instance.SummonNearbyStationList();
			print($"CraftingManager 작업대 제거 : {station.craftStationType}");
		}
	}
}
