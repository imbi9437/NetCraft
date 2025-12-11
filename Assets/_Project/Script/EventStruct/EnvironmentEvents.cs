using _Project.Script.Interface;

namespace _Project.Script.EventStruct
{
	public static class EnvironmentEvents
	{
		public struct RequestDecreaseEvent : IEvent {}
		
		/// <summary>
		/// 시간대 변화 이벤트
		/// </summary>
		public struct OnChangeEnvironmentEvent : IEvent
        {
			public DayPhase newDayPhase;
			public OnChangeEnvironmentEvent(DayPhase newDayPhase)
			{
				this.newDayPhase = newDayPhase;
			}
		}

		/// <summary>
		/// 스탯감소 이벤트
		/// </summary>
		public struct OnStatDecreaseEvent : IEvent
        {
			public float hungerDecreaseAmount;
			public float thirstDecreaseAmount;
			public OnStatDecreaseEvent(float hungerDecreaseAmount, float thirstDecreaseAmount)
			{
				this.hungerDecreaseAmount = hungerDecreaseAmount;
				this.thirstDecreaseAmount = thirstDecreaseAmount;
			}
		}


		/// <summary>
		/// 월드타임 데이터 보내기
		/// </summary>
        public struct SendEnvironmentDataEvent : IEvent
        {
			public float worldTime;
			public SendEnvironmentDataEvent(float worldTime)
			{
				this.worldTime = worldTime;
            }

        }

		/// <summary>
		/// 월드타임 데이터 받기
		/// </summary>
		public struct ReceiveEnvironmentDataEvent : IEvent
		{
			public float worldTime;
			public ReceiveEnvironmentDataEvent(float worldTime)
			{
				this.worldTime = worldTime;
            }

        }

    }


}