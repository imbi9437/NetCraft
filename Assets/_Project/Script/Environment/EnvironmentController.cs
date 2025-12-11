using _Project.Script.Generic;
using _Project.Script.Manager;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using ExitGames.Client.Photon;
using static _Project.Script.EventStruct.EnvironmentEvents;
using System;
using _Project.Script.EventStruct;

public enum DayPhase
{
	Dawn,
	Morning,
	Noon,
	Afternoon,
	Evening,
	Night
}

public enum PhotonEventCode : byte
{
	None=0,
	EnvironmentChange =1
}
namespace _Project.Script.Environment
{
	public class EnvironmentController : MonoBehaviour, IOnEventCallback
	{
		[Header("Time Settings")]
		public float currentHour;
		private DayPhase currentDayPhase;
		private float dayDuration = 600f;
		private float timeScale = 1f;

		[Header("Stat Settings")]
		private float statDecreaseInterval = 10f;
		private float hungerDecreaseAmount = 1f;
		private float thirstDecreaseAmount = 1f;

		private float statTick;

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
		}

		private void Update()
		{
			if (statTick <= statDecreaseInterval)
			{
				statTick += Time.deltaTime;
				return;
			}

			statTick = 0f;
			EventHub.Instance?.RaiseEvent(new RequestDecreaseEvent());
			
			// if (Input.GetKeyDown(KeyCode.T))
			// 	Time.timeScale = 10f;
			// UpdateTime();
			// statTick += Time.deltaTime;
			// if (statTick >= statDecreaseInterval)
			// {
			// 	statTick = 0f;
			//
			// 	UpdateStatDecrease();
			// }
		}

		
		/// <summary>
		/// 시간대 계산
		/// </summary>
		private DayPhase GetDayPhase(float hour)
		{
			float phase = dayDuration / 6f;

			if (hour < phase) return DayPhase.Dawn;
			else if (hour < phase * 2) return DayPhase.Morning;
			else if (hour < phase * 3) return DayPhase.Noon;
			else if (hour < phase * 4) return DayPhase.Afternoon;
			else if (hour < phase * 5) return DayPhase.Evening;
			else return DayPhase.Night;
		}

		/// <summary>
		/// 환경 변화 브로드캐스트 포톤으로 이벤트 전송
		/// </summary>
		private void BroadcastEnvironmentChange(DayPhase phase)
		{
			Debug.Log($"[환경 변화] 현재 시간대: {phase}");
			EventHub.Instance.RaiseEvent(new OnChangeEnvironmentEvent(phase));
			object content = (int)phase; // 전송할 데이터
										 //bool reliable = true; // 신뢰성 있는 전송 여부

			RaiseEventOptions raiseEventOptions = new RaiseEventOptions
			{
				Receivers = ReceiverGroup.Others // 다른 클라이언트들에게 전송
			};

			PhotonNetwork.RaiseEvent((byte)PhotonEventCode.EnvironmentChange, content, raiseEventOptions, SendOptions.SendReliable);
		}



		/// <summary>
		/// 클라이언트에서 환경 이벤트 수신 시 처리
		/// </summary>
		private void OnEnvironmentChanged(OnChangeEnvironmentEvent evt)
		{
			currentDayPhase = evt.newDayPhase;
			Debug.Log($"[클라이언트 환경 동기화] 받은 시간대: {currentDayPhase}");
		}

		/// <summary>
		/// 이벤트 발생시 호출되는 메서드
		/// </summary>
		/// <param name="photonEvent"></param>
		public void OnEvent(EventData photonEvent)
		{
			if (photonEvent.Code != (byte)PhotonEventCode.EnvironmentChange) return;

			if (!PhotonNetwork.IsMasterClient)
			{
				if (photonEvent.Code == 1)
				{
					DayPhase receivedPhase = (DayPhase)(int)photonEvent.CustomData;
					currentDayPhase = receivedPhase;
					Debug.Log($"[클라이언트 환경 동기화 - 포톤] 받은 시간대: {currentDayPhase}");
					EventHub.Instance.RaiseEvent(new OnChangeEnvironmentEvent(receivedPhase));
				}
			}
		}


		/// <summary>
		/// 포톤 마스터일경우 시간 흐름
		/// </summary>
		private void UpdateTime()
		{
			if (!PhotonNetwork.IsMasterClient) return;

            currentHour += Time.deltaTime * timeScale;

            DataManager.Instance.UpdateWorldEnvironmentTime(currentHour);
			if (currentHour >= dayDuration)
				currentHour = 0f;

			DayPhase newPhase = GetDayPhase(currentHour);
			if (newPhase != currentDayPhase)
			{
				currentDayPhase = newPhase;
				BroadcastEnvironmentChange(newPhase);
			}
		}
		
		/// <summary>
		/// 포톤 마스터일경우 스탯 감소 함수, 스폰된 모든 플레이어의 데이터를 호스트가 받아서 감소처리
		/// </summary>
		private void UpdateStatDecrease()
		{
			if(!PhotonNetwork.IsMasterClient) return;

            DataManager.Instance.PlayersStatEnvironmentEffect(hungerDecreaseAmount, thirstDecreaseAmount);
		}
	}
}