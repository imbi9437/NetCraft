using System;
using _Project.Script.Interface;
using _Project.Script.Manager;
using _Project.Script.Generic;
using Unity.VisualScripting;

namespace _Project.Script.EventStruct
{

	public static class FirebaseEvents
	{
		#region Firebase 초기화 이벤트
		
		
		public struct FirebaseInitialized : IEvent
		{
		}
		public struct FirebaseInitFailed : IEvent
		{
			public string Error;
			public FirebaseInitFailed(string error) => Error = error;
		}
		
		
		#endregion

		#region Firebase 인증 이벤트

		public struct FirebaseLoginSuccess : IEvent
		{
			public readonly string UserId;
			public FirebaseLoginSuccess(string userId) 
			{
				UserId = userId;
			}
		}
	
		public struct FirebaseLoginFailed : IEvent
		{
			public string Error;
			public FirebaseLoginFailed(string error) 
			{ 
				Error = error; 
			}
		}

		public struct FirebaseRegisterSuccess : IEvent
		{
			public string UserId;
			public string nickName;
			public FirebaseRegisterSuccess(string userId, string nickName) 
			{
				UserId = userId;
				this.nickName = nickName;
			}
		}
	
		public struct FirebaseRegisterFailed : IEvent
		{
			public string Error;
			public FirebaseRegisterFailed(string error) 
			{
				Error = error;
			} 
		}

		public struct FirebaseLogoutEvent : IEvent
		{
		
		}
	
		#endregion
		
		#region Firebase 데이터 이벤트

		public struct FirebaseDataUploaded : IEvent
		{
			public Type type;
			
			public FirebaseDataUploaded(Type type)
			{
				this.type = type;
			}
		}
		public struct FirebaseDataUploadFailed : IEvent
		{
			public string Error;
			public FirebaseDataUploadFailed(string error) => Error = error;
		}

		public struct FirebaseDataDownLoaded<T> : IEvent where T : class
		{
			public T data;
			public FirebaseDataDownLoaded(T data) => this.data = data;
		}
		public struct FirebaseDataDownloadFailed : IEvent
		{
			public string Error;
			public FirebaseDataDownloadFailed(string error) => Error = error;
		}
	
		#endregion

		#region Firebase Request Events

		public struct RequestLoginEvent : IEvent
		{
			public string email;
			public string password;

			public RequestLoginEvent(string email, string password)
			{
				this.email = email;
				this.password = password;
			}
		}
		public struct RequestRegisterEvent : IEvent
		{
			public string email;
			public string password;
			public string nickName;

			public RequestRegisterEvent(string email, string password, string nickName)
			{
				this.email = email;
				this.password = password;
				this.nickName = nickName;
			}
		}
		public struct RequestLogoutEvent : IEvent {}

		#endregion
	}
}
