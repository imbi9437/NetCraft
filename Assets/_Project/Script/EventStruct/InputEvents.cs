using _Project.Script.Interface;
using UnityEngine;
using UnityEngine.InputSystem;
namespace _Project.Script.EventStruct
{
	public static class InputEvents
	{
		public struct MoveInputEvent : IEvent
		{
			public Vector2 value;
			
			public MoveInputEvent(Vector2 value)
			{
				this.value = value;
			}
		}

		public struct InteractInputEvent : IEvent
		{
		}

		public struct AttackInputEvent : IEvent
		{
			
		}
		
		public struct ScrollInputEvent : IEvent
		{
			public float value;
			public bool isDown;

			public ScrollInputEvent(float value, bool isDown)
			{
				this.value = value;
				this.isDown = isDown;
			}
		}
		
		
		public struct KeyInputEvent : IEvent
		{
			public Key key;
			public bool isDown;

			public KeyInputEvent(Key key, bool isDown)
			{
				this.key = key;
				this.isDown = isDown;
			}
		}
	}
	
	public struct AnyInputEvent : IEvent { }
	
	public struct InteractionEvent : IEvent
	{
		public Key key;
		public InteractionEvent(Key key) { this.key = key; Debug.Log("상호작용 이벤트 실행"); }
	}

	public struct OpenInventoryEvent : IEvent
	{
		public Key key;
		public OpenInventoryEvent(Key key) { this.key = key; Debug.Log("인벤토리 열기 실행"); }
	}

	public struct OpenEquipmentsEvent : IEvent
	{
		public Key key;
		public OpenEquipmentsEvent(Key key) { this.key = key; Debug.Log("장비창열기 실행"); } 
	}

	public struct SpecialInteractionEvent : IEvent
	{
		public Key key;
		public SpecialInteractionEvent(Key key) { this.key = key; Debug.Log("특별 상호작용 이벤트 실행"); }
	}
}
