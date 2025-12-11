using _Project.Script.EventStruct;
using _Project.Script.Generic;
using _Project.Script.Interface;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

using EVT = _Project.Script.EventStruct.InputEvents;

namespace _Project.Script.Manager
{
	[DefaultExecutionOrder(-50)]
	public class InputManager : MonoSingleton<InputManager>
	{
		private PlayerInputActions inputActions;

		private Dictionary<string, Func<Key, IEvent>> actionEventMap;


		private float scrollValue;

		protected override void Awake()
		{
			base.Awake();
			
			inputActions = new PlayerInputActions();

			actionEventMap = new Dictionary<string, Func<Key, IEvent>>
			{
				{"Interaction", key => new InteractionEvent(key)},
				{"Inventory", key => new OpenInventoryEvent(key)},
				{ "Equipments", key => new OpenEquipmentsEvent(key)}
				//이밑으로 쭉 추가가능
			};
		}


		private void OnEnable()
		{
			inputActions.Enable();
			foreach (var action in inputActions.Player.Get())
			{
				if (actionEventMap.ContainsKey(action.name))
				{
					action.performed += OnActionPerformed; 
				}
			}
		}

		private void Update()
		{
			if (Input.anyKeyDown)
			{
				EventHub.Instance.RaiseEvent(new AnyInputEvent());
			}
			
			CheckMoveInput();
			CheckMouseScroll();
			CheckCameraControl();
			CheckInteractInput();
			CheckAttackInput();
		}

		private void OnDisable()
		{
			foreach (var action in inputActions.Player.Get())
			{
				if (actionEventMap.ContainsKey(action.name))
				{
					action.performed -= OnActionPerformed;
				}
			}
			inputActions.Disable();
		}

		private void CheckMoveInput()
		{
			float horizontal = Input.GetAxisRaw("Horizontal");
			float vertical = Input.GetAxisRaw("Vertical");
			EventHub.Instance.RaiseEvent(new EVT.MoveInputEvent(new Vector2(horizontal, vertical)));
		}

		private void CheckMouseScroll()
		{
			scrollValue = Input.GetAxis("Mouse ScrollWheel");

			if (Mathf.Approximately(scrollValue, 0f)) return;

			bool isDown = scrollValue < 0f;
			EventHub.Instance.RaiseEvent(new EVT.ScrollInputEvent(scrollValue, isDown));
		}

		private void CheckInteractInput()
		{
			if (Input.GetKeyDown(KeyCode.F)) EventHub.Instance.RaiseEvent(new EVT.InteractInputEvent());
		}

		private void CheckAttackInput()
		{
			if (Input.GetKeyDown(KeyCode.Space)) EventHub.Instance.RaiseEvent(new EVT.AttackInputEvent());
		}
		
		private void CheckCameraControl()
		{
			if (Input.GetKeyDown(KeyCode.Q)) EventHub.Instance.RaiseEvent(new EVT.KeyInputEvent(Key.Q, true));
			if (Input.GetKeyDown(KeyCode.E)) EventHub.Instance.RaiseEvent(new EVT.KeyInputEvent(Key.E, true));
		}
		

		/// <summary>
		/// 키 입력시 발생되는 이벤트를 발행하는 함수
		/// </summary>
		/// <param name="context"></param>
		private void OnActionPerformed(InputAction.CallbackContext context)
		{
			var action = context.action;
			string bindingPath = action.bindings[0].effectivePath;
			Key keyEnum = GetKeyFromBindingPath(bindingPath);
			switch (action.name)
			{
				case "Interaction": 
					EventHub.Instance.RaiseEvent(new InteractionEvent(keyEnum));
					break;
				case "Inventory":
					EventHub.Instance.RaiseEvent(new OpenInventoryEvent(keyEnum));
					break;
				case "Equipments":
					EventHub.Instance.RaiseEvent(new OpenEquipmentsEvent(keyEnum));
					break;
				case "SpecialInteraction":
					EventHub.Instance.RaiseEvent(new SpecialInteractionEvent(keyEnum));
					break;
			}
		}

		/// <summary>
		/// 키코드로 변환해주는 함수
		/// </summary>
		/// <param name="bindingPath"></param>
		/// <returns></returns>
		private Key GetKeyFromBindingPath(string bindingPath)
		{
			// "<Keyboard>/f" 형태에서 마지막 문자 추출 (간단한 예)
			if (string.IsNullOrEmpty(bindingPath))
				return Key.None;

			// 경로는 "<Keyboard>/f" 이므로 '/' 뒤 문자만 따오기
			int slashIndex = bindingPath.LastIndexOf('/');
			if (slashIndex < 0 || slashIndex == bindingPath.Length - 1)// /와 같거나 1칸 앞에 있는 문자는 거름
				return Key.None;

			string keyString = bindingPath.Substring(slashIndex + 1); // /로부터 1칸뒤에있는 문자를 받아옴

			// Key enum 파싱 시 대문자로 변환 필요
			try
			{
				Key keyEnum = (Key)Enum.Parse(typeof(Key), keyString, true);
				return keyEnum;
			}
			catch
			{
				return Key.None;
			}
		}

		public void LoadRebinds()
		{
			var map = inputActions.asset.FindActionMap("Player");
			if (PlayerPrefs.HasKey("rebinds"))
			{
				string rebinds = PlayerPrefs.GetString("rebinds");
				map.LoadBindingOverridesFromJson(rebinds);
			}
		}
	}
}
