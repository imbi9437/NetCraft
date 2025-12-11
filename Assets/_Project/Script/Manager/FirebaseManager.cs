using _Project.Script.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using _Project.Script.EventStruct;	// 이벤트 구조체들 위치
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

using FBEvt = _Project.Script.EventStruct.FirebaseEvents;

namespace _Project.Script.Manager
{
	public class FirebaseManager : MonoSingleton<FirebaseManager>
	{
		private const string UserDataPath = "users";
		
		public FirebaseApp App { get; private set; }
		public FirebaseAuth Auth { get; private set; }
		public FirebaseDatabase Database { get; private set; }
		public bool IsLoggedIn { get; private set;}
		
		private DatabaseReference _rootRef;
		
		private Dictionary<Type, string> _dataPathDic;

		protected override void Awake()
		{
			base.Awake();
			
			_dataPathDic = new Dictionary<Type, string>();
			IsLoggedIn = false;

			RegisterEvent();
		}

		private void OnDestroy()
		{
			UnRegisterEvent();
		}

		public override void Initialize() => InitializeFirebaseAsync();
		private async void InitializeFirebaseAsync()
		{
			try
			{
                DependencyStatus state = await FirebaseApp.CheckAndFixDependenciesAsync();

				if (state == DependencyStatus.Available)
				{
					App = FirebaseApp.DefaultInstance;
					Auth = FirebaseAuth.DefaultInstance;
					Database = FirebaseDatabase.DefaultInstance;

                    _rootRef = Database.RootReference;
					
					IsInitialized = true;
					EventHub.Instance.RaiseEvent(new FirebaseEvents.FirebaseInitialized());
				}
				else
				{
					EventHub.Instance.RaiseEvent(new FirebaseEvents.FirebaseInitFailed(state.ToString()));
					Debug.LogError($"Firebase Init Failed: {state}");
				}
			}
			catch (Exception e)
			{
				EventHub.Instance.RaiseEvent(new FirebaseEvents.FirebaseInitFailed(e.Message));
				Debug.LogError($"Firebase Init Failed: {e.Message}");
			}
		}


		private void RegisterEvent()
		{
			EventHub.Instance?.RegisterEvent<FBEvt.RequestLoginEvent>(RequestLoginEvent);
			EventHub.Instance?.RegisterEvent<FBEvt.RequestRegisterEvent>(RequestRegisterEvent);
			EventHub.Instance?.RegisterEvent<FBEvt.RequestLogoutEvent>(RequestLogoutEvent);
		}
		private void UnRegisterEvent()
		{
			EventHub.Instance?.UnregisterEvent<FBEvt.RequestLoginEvent>(RequestLoginEvent);
			EventHub.Instance?.UnregisterEvent<FBEvt.RequestRegisterEvent>(RequestRegisterEvent);
			EventHub.Instance?.UnregisterEvent<FBEvt.RequestLogoutEvent>(RequestLogoutEvent);
		}
		
		#region Event Rapper Functions

		private void RequestLoginEvent(FBEvt.RequestLoginEvent evt) => LoginAsync(evt.email, evt.password);
		private void RequestRegisterEvent(FBEvt.RequestRegisterEvent evt) => RegisterUserAsync(evt.email, evt.password, evt.nickName);
		private void RequestLogoutEvent(FBEvt.RequestLogoutEvent evt) => Logout();

		#endregion
		
		
		private async Task LoginAsync(string email, string password)
		{
            try
			{
				AuthResult result = await Auth.SignInWithEmailAndPasswordAsync(email, password);
				string uid = result.User.UserId;

				_dataPathDic.TryAdd(typeof(UserData), $"{UserDataPath}/{uid}");
				IsLoggedIn = true;
				
				EventHub.Instance.RaiseEvent(new FBEvt.FirebaseLoginSuccess(uid));
			}
			catch (FirebaseException e)
			{
				Debug.LogError($"로그인 실패 (Firebase): {e.Message}");
				EventHub.Instance.RaiseEvent(new FBEvt.FirebaseLoginFailed(e.Message));
            }
			catch (Exception e)
			{
				Debug.LogError($"로그인 실패 (System): {e.Message}");
				EventHub.Instance.RaiseEvent(new FBEvt.FirebaseLoginFailed(e.Message));
            }
		}
		private async Task RegisterUserAsync(string email, string password, string nickName)
		{
			try
			{
				if (string.IsNullOrEmpty(nickName)) throw new Exception("닉네임을 입력해주세요.");
				
				AuthResult result = await Auth.CreateUserWithEmailAndPasswordAsync(email, password);
				string uid = result.User.UserId;
				
				_dataPathDic.TryAdd(typeof(UserData), $"{UserDataPath}/{uid}");
				IsLoggedIn = true;

				EventHub.Instance.RaiseEvent(new FBEvt.FirebaseRegisterSuccess(uid, nickName));
            }
			catch (FirebaseException e)
			{
				Debug.LogError($"회원가입 실패 (Firebase): {e.Message}");
				EventHub.Instance.RaiseEvent(new FBEvt.FirebaseRegisterFailed(e.Message));
            }
			catch (Exception e)
			{
				Debug.LogError($"회원가입 실패 (System): {e.Message}");
				EventHub.Instance.RaiseEvent(new FBEvt.FirebaseRegisterFailed(e.Message));
            }
		}
		private void Logout()
		{
			Auth.SignOut();
			IsLoggedIn = false;
			EventHub.Instance.RaiseEvent(new FBEvt.FirebaseLogoutEvent());
		}
		
		
		public async UniTask UploadData<T>(T data) where T : class
		{
			string json = JsonConvert.SerializeObject(data);
			try
			{
				if (_dataPathDic.TryGetValue(typeof(T), out var path) == false)
					throw new Exception($"경로 미설정: {typeof(T).Name}. Login 이후에 호출해야 합니다.");
				
				await _rootRef.Child(path).SetRawJsonValueAsync(json);
				
				EventHub.Instance?.RaiseEvent(new FBEvt.FirebaseDataUploaded(typeof(T)));
				Debug.Log($"{typeof(T)} 데이터 업로드 완료");
			}
			catch (Exception e)
			{
				Debug.LogError($"데이터 업로드 실패 : {e.Message}");
				EventHub.Instance?.RaiseEvent(new FBEvt.FirebaseDataUploadFailed(e.Message));
			}
		}
		public async UniTask DownloadData<T>(Action<T> callback) where T : class
		{
			try
			{
				if (_dataPathDic.TryGetValue(typeof(T), out var path) == false)
					throw new Exception($"경로 미설정: {typeof(T).Name}. Login 이후에 호출해야 합니다.");
				
				var snapshot = await _rootRef.Child(path).GetValueAsync();

				if (snapshot.Exists == false) throw new Exception("해당 경로에 데이터가 없습니다.");
				
				T data = JsonConvert.DeserializeObject<T>(snapshot.GetRawJsonValue());
				
				callback?.Invoke(data);
			}
			catch (Exception e)
			{
				Debug.LogError($"데이터 다운로드 실패 : {e.Message}");
				EventHub.Instance?.RaiseEvent(new FBEvt.FirebaseDataDownloadFailed(e.Message));
			}
		}
		
	}
}
