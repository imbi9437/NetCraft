using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using _Project.Script.Manager;
using _Project.Script.UI.GlobalUI;
using UnityEngine.Rendering;
using DG.Tweening;

public class RebindUI : MonoBehaviour
{
	[SerializeField] InputActionAsset inputActions;
	[SerializeField] Transform actionListParent;
	[SerializeField] GameObject actionItemPrefab;
	[SerializeField] Button applyButton; //키변경 적용
	[SerializeField] Button resetButton; //디폴트 키로 초기화

	bool isDirty = false;
	//applyButton을 누르지 않고 다른 메뉴로 이동할시에 '변경사항을 저장하시겠습니까?'를 TwoButtonPopup을 이용해서 띄우는 방법...?

	//InputAction의 확장기능
	private InputActionRebindingExtensions.RebindingOperation currentRebindingOperation;
	Image currentRebindingButtonImage;


	private void Awake()
	{
		resetButton.onClick.RemoveAllListeners();
		resetButton.onClick.AddListener(ResetBindings);
		applyButton.onClick.RemoveAllListeners();
		applyButton.onClick.AddListener(()=> 
		{ 
			SaveRebinds();
			InputManager.Instance.LoadRebinds();
			RefreshUI();
		});
	}

	private void Start()
	{
		LoadRebinds();
		var map = inputActions.FindActionMap("Player");
		
		foreach (var action in map.actions)
		{
			CreateActionUI(action);
		}
	}

	/// <summary>
	/// 키변경 버튼을 생성
	/// </summary>
	/// <param name="action"></param>

	void CreateActionUI(InputAction action)
	{
		// 액션의 바인딩마다 버튼 생성
		for (int i = 0; i < action.bindings.Count; i++)
		{
			GameObject go = Instantiate(actionItemPrefab, actionListParent);
			TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();
			Button button = go.GetComponent<Button>();
			Image image = button.GetComponent<Image>();

			string bindingName = GetBindingName(action, i);
			text.text = $"{action.name}: {bindingName}";

			int bindingIndex = i; // 클로저 문제 방지용 복사본

			button.onClick.AddListener(() =>
			{
				if (currentRebindingOperation != null)
				{
					if (currentRebindingButtonImage != image)
					{
						if (currentRebindingButtonImage != null)
						{
							currentRebindingButtonImage.DOColor(Color.red, 0.1f).SetLoops(4, LoopType.Yoyo).OnComplete(() =>
								{
									currentRebindingButtonImage.color = Color.white;
								});
						}
					}
					return;
				}

				currentRebindingButtonImage = image;
				button.interactable = false;
				text.text = $"{action.name} : Press any key...";

				currentRebindingOperation = StartRebind(action, button, text, bindingIndex);
			});
		}
	}

	/// <summary>
	/// bindingpath로부터 key를 파싱받기 위해 만든 함수
	/// </summary>
	/// <param name="action"></param>
	/// <returns></returns>
	string GetBindingName(InputAction action, int bindingIndex)
	{
		if (bindingIndex >= 0 && bindingIndex < action.bindings.Count)
		{
			return InputControlPath.ToHumanReadableString(
				action.bindings[bindingIndex].effectivePath,
				InputControlPath.HumanReadableStringOptions.OmitDevice);
		}
		return "Unbound";
	}

	/// <summary>
	/// 마우스를 제외한 키값을 입력받는 함수(버튼을 누를시 활성화) //한번에 하나씩만 받도록, 같은 키 값이 중복되지 않도록
	/// </summary>
	/// <param name="action"></param>
	/// <param name="button"></param>
	/// <param name="text"></param>

	InputActionRebindingExtensions.RebindingOperation StartRebind(InputAction action, Button button, TextMeshProUGUI text, int bindingIndex)
	{
		return action.PerformInteractiveRebinding(bindingIndex)
			.WithControlsExcluding("Mouse")
			.OnComplete(operation =>
			{
				string newBindingPath = action.bindings[bindingIndex].effectivePath;

				if (AlreadyHasKey(action, newBindingPath))
				{
					Debug.Log("이미 할당된 키임");
					action.RemoveBindingOverride(bindingIndex);

					currentRebindingOperation = null;
					currentRebindingButtonImage = null;

					button.interactable = true;
					text.text = $"*{action.name}: {GetBindingName(action, bindingIndex)}";

					return;
				}

				currentRebindingOperation = null;
				currentRebindingButtonImage = null;

				button.interactable = true;
				text.text = $"*{action.name}: {GetBindingName(action, bindingIndex)}";

				SaveRebinds();
			}).Start();
	}


	/// <summary>
	/// 키를 입력하면 PlayerPrefs에 저장
	/// </summary>
	void SaveRebinds()
	{
		var map = inputActions.FindActionMap("Player");
		string rebinds = map.SaveBindingOverridesAsJson();
		PlayerPrefs.SetString("rebinds",rebinds);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// PlayerPrefs로부터 저장된 키값을 가져옴
	/// </summary>
	public void LoadRebinds()
	{
		var map = inputActions.FindActionMap("Player");
		if (PlayerPrefs.HasKey("rebinds"))
		{
			string rebinds = PlayerPrefs.GetString("rebinds");
			map.LoadBindingOverridesFromJson(rebinds);
		}
	}

	/// <summary>
	/// 초기화버튼
	/// </summary>
	public void ResetBindings()
	{
		PlayerPrefs.DeleteKey("rebinds");
		inputActions.RemoveAllBindingOverrides();
		RefreshUI();
	}

	/// <summary>
	/// UI새로고침
	/// </summary>
	public void RefreshUI()
	{
		foreach(Transform child in actionListParent)
		{
			Destroy(child.gameObject);
		}
		var map = inputActions.FindActionMap("Player");
		foreach(var action in map)
		{
			CreateActionUI(action);
		}
	}

	bool AlreadyHasKey(InputAction currentAction, string newbindingPath)
	{
		var actionMap = currentAction.actionMap;
		foreach( var action in actionMap)
		{
			if (action == currentAction) continue;
			foreach(var binding in action.bindings)
			{
				if(binding.effectivePath == newbindingPath)
				{
					return true;//키 중복임!
				}
			}
		}
		return false;//중복아님!
	}

}
