using UnityEngine;
using _Project.Script.Manager;
using _Project.Script.Generic;
public enum MentalState
{
	평온,
	공포,
	무기력
}

[RequireComponent(typeof(CHJPlayerCharacter))]
public class CHJPlayerStatusHandler : MonoBehaviour
{
	public PlayerData CurrentStatus { get; set; }

	private CHJPlayerCharacter playerCharacter;

	private void Awake()
	{
		playerCharacter = GetComponent<CHJPlayerCharacter>();
	}

	private void OnEnable()
	{
	}
	/// <summary>
	/// 시간이 지남에 따른 스탯감소(호스트 전용)
	/// </summary>
	/// <param name="hungerDelta"></param>
	/// <param name="thirstDelta"></param>
	public void ApplyStatDecrease(float hungerDelta, float thirstDelta)
	{
		CurrentStatus.hunger = CurrentStatus.hunger - hungerDelta;
		//CurrentStatus.thirst = CurrentStatus.thirst - thirstDelta;
	}

	/// <summary>
	/// 외부에서 플레이어 상태를 받아 적용하는 함수
	/// </summary>
	/// <param name="newState">새로운 상태 데이터</param>
	public void ApplyStatus(PlayerData newState)
	{
		if (newState == null)
		{
			Debug.LogWarning("[StatusHandler] 받은 상태가 null입니다.");
			return;
		}

		CurrentStatus = newState;
		
		//ApplyMentalState(newState.mentalState);
		// 추가적인 스탯 적용이 필요하면 여기에 구현
	}

	/// <summary>
	/// 현재 상태에 따라 스탯을 감소시킴 (호스트 전용)
	/// </summary>

	private void ApplyPosition(Vector3 newPosition)
	{
		transform.position = newPosition;
	}

	/// <summary>
	/// 멘탈	상태에 따른 효과 적용
	/// </summary>
	/// <param name="mentalStateStr"></param>
	private void ApplyMentalState(string mentalStateStr)
	{
		if (!System.Enum.TryParse(mentalStateStr, out MentalState parsedState))
		{
			Debug.LogWarning($"[StatusHandler] 알 수 없는 정신 상태: {mentalStateStr}");
			return;
		}

		switch (parsedState)
		{
			case MentalState.공포:
				playerCharacter.moveSpeed = -5;
				break;
			case MentalState.무기력:
				playerCharacter.moveSpeed = 2;
				break;
			case MentalState.평온:
				playerCharacter.moveSpeed = 5;
				break;
			default:
				playerCharacter.moveSpeed = 5;
				break;
		}
	}
}
