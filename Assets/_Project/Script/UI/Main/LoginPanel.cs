using System;
using _Project.Script.Manager;
using _Project.Script.Core;
using _Project.Script.EventStruct;
using _Project.Script.UI.GlobalUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace _Project.Script.UI.Main
{
	public class LoginPanel : MainPanel
	{
		public override MainMenuPanelType UIType => MainMenuPanelType.Login;
		
		[SerializeField] private TMP_Text titleText;
		[SerializeField] private TMP_Text changeModeText;
		
		[SerializeField] private TMP_InputField emailInputField;
		[SerializeField] private TMP_InputField passwordInputField;
		[SerializeField] private TMP_InputField nickNameInputField;
		
		[SerializeField] private Button loginButton;
		[SerializeField] private Button registerButton;
		[SerializeField] private Button changeModeButton;
		[SerializeField] private Button quitGameButton;

		[SerializeField] private GameObject nickNameObject;
		
		private bool isRegisterMode = false;
		
		private void OnEnable()
		{
			isRegisterMode = false;
			
			loginButton.onClick.AddListener(OnLoginButtonClick);
			registerButton.onClick.AddListener(OnRegisterButtonClick);
			quitGameButton.onClick.AddListener(OnQuitGameButtonClick);
			changeModeButton.onClick.AddListener(OnChangeModeButtonClick);

			EventHub.Instance.RegisterEvent<FirebaseEvents.FirebaseLoginSuccess>(LoginSuccess);
			EventHub.Instance.RegisterEvent<FirebaseEvents.FirebaseLoginFailed>(LoginFailed);
			EventHub.Instance.RegisterEvent<FirebaseEvents.FirebaseRegisterSuccess>(RegisterSuccess);
			EventHub.Instance.RegisterEvent<FirebaseEvents.FirebaseRegisterFailed>(RegisterFailed);

			titleText.text = "로그인";
			SetUIElement(isRegisterMode);
		}

		private void OnDisable()
		{
			loginButton.onClick.RemoveListener(OnLoginButtonClick);
			registerButton.onClick.RemoveListener(OnRegisterButtonClick);
			quitGameButton.onClick.RemoveListener(OnQuitGameButtonClick);
			changeModeButton.onClick.RemoveListener(OnChangeModeButtonClick);

			EventHub.Instance?.UnregisterEvent<FirebaseEvents.FirebaseLoginSuccess>(LoginSuccess);
			EventHub.Instance?.UnregisterEvent<FirebaseEvents.FirebaseLoginFailed>(LoginFailed);
			EventHub.Instance?.UnregisterEvent<FirebaseEvents.FirebaseRegisterSuccess>(RegisterSuccess);
			EventHub.Instance?.UnregisterEvent<FirebaseEvents.FirebaseRegisterFailed>(RegisterFailed);
		}

		#region 버튼 이벤트

		private void OnLoginButtonClick()
		{
			string email = emailInputField.text;
			string pw = passwordInputField.text;

			var evt = new FirebaseEvents.RequestLoginEvent(email, pw);
			EventHub.Instance?.RaiseEvent(evt);
		}

		private void OnRegisterButtonClick()
		{
			string email = emailInputField.text;
			string pw = passwordInputField.text;
			string nickName = nickNameInputField.text;

			var evt = new FirebaseEvents.RequestRegisterEvent(email, pw, nickName);
			EventHub.Instance?.RaiseEvent(evt);
		}

		private void OnChangeModeButtonClick()
		{
			isRegisterMode = !isRegisterMode;
			SetUIElement(isRegisterMode);
		}
		private void OnQuitGameButtonClick() => Application.Quit();

		private void SetUIElement(bool isRegiMode)
		{
			emailInputField.SetTextWithoutNotify("");
			passwordInputField.SetTextWithoutNotify("");
			nickNameInputField.SetTextWithoutNotify("");
			
			titleText.text = isRegiMode ? "회원가입" : "로그인";
			changeModeText.text = isRegiMode ? "돌아가기" : "회원가입";
			
			loginButton.gameObject.SetActive(!isRegiMode);
			registerButton.gameObject.SetActive(isRegiMode);
			
			nickNameObject.SetActive(isRegiMode);
		}

		#endregion

		#region 로그인/회원가입 결과 팝업창 이벤트

		private void LoginSuccess(FirebaseEvents.FirebaseLoginSuccess args)
		{
			var param = new PopupParam("로그인", "로그인 했습니다.");
			param.confirm = Hide;
			GlobalUIManager.Instance.ShowPanel(GlobalPanelType.ConfirmPopup, param);
		}

		private void LoginFailed(FirebaseEvents.FirebaseLoginFailed args)
		{
			var param = new PopupParam("<color=red>로그인 실패</color>", $"로그인에 실패 했습니다 {args.Error}");
			GlobalUIManager.Instance.ShowPanel(GlobalPanelType.ConfirmPopup, param);
		}

		private void RegisterSuccess(FirebaseEvents.FirebaseRegisterSuccess args)
		{
			var param = new PopupParam("회원가입", "회원가입 되었습니다.");
			param.confirm = Hide;
			GlobalUIManager.Instance.ShowPanel(GlobalPanelType.ConfirmPopup, param);
		}

		private void RegisterFailed(FirebaseEvents.FirebaseRegisterFailed args)
		{
			var param = new PopupParam("<color=red>회원가입 실패</color>", $"회원가입에 실패 하였습니다. {args.Error}");
			GlobalUIManager.Instance.ShowPanel(GlobalPanelType.ConfirmPopup, param);
		}

		#endregion
	}
}
