using _Project.Script.EventStruct;
using _Project.Script.Manager;
using _Project.Script.StateMachine.Mono;
using Photon.Realtime;
using System.Threading.Tasks;
using _Project.Script.Items;
using UnityEngine;
using _Project.Script.Generic;

public class CHJPlayerCharacter : MonoBehaviour
{
	public int moveSpeed = 5;

	MonoStateMachine MonoStateMachine;

	float syncInterval = 0.2f;
	float syncTimer = 0;

	public bool isMine = false;

	public MentalState test;

	public ItemInstance testitem;


	private void Awake()
	{
		MonoStateMachine = GetComponent<MonoStateMachine>();
	}

	void OnEnable()
	{
		//EventHub.Instance.RegisterEvent<RightClickInputEvent>(MovePlayer);
		EventHub.Instance.RegisterEvent<CHJTakeDamageEvent>(TakeDamage);
		EventHub.Instance.RegisterEvent<CHJMentalEvent>(ChangeMentalState);
	}

	private void OnDisable()
	{
		//EventHub.Instance.UnregisterEvent<RightClickInputEvent>(MovePlayer);
		EventHub.Instance.UnregisterEvent<CHJTakeDamageEvent>(TakeDamage);
		EventHub.Instance.UnregisterEvent<CHJMentalEvent>(ChangeMentalState);
	}

	/// <summary>
	/// 인벤토리가 잘 불러와지는지 확인하는 함수
	/// </summary>
	void PrintInventory()
	{
		// var inventory = DataManager.Instance.localPlayerData.inventory;
		// Debug.Log("현재 인벤토리 상태:");
		// for (int i = 0; i < inventory.itemSlot.Length; i++)
		// {
		// 	var item = inventory.itemSlot[i];
		// 	if (item == null || item.itemData == null)
		// 		Debug.Log($"슬롯 {i}: 빈 슬롯");
		// 	else
		// 		Debug.Log($"슬롯 {i}: {item.itemData.name} x {item.count}");
		// }
	}
	private void Update()
	{
		if (!isMine) return;

		if (Input.GetKeyDown(KeyCode.Space))
		{
			//EventHub.Instance.RaiseEvent(new CHJTakeDamageEvent(3));//파이어베이스에 바로반영되는거 확인.
			//DataManager.Instance.AddItem(testitem, out bool isDestroyObject);
			//PrintInventory();
			_ = UploadPositionAsync();
		}
		if (Input.GetKeyDown(KeyCode.H))
		{
			EventHub.Instance.RaiseEvent(new CHJMentalEvent(MentalState.공포)); //이것도 확인
		}
		if(Input.GetKeyDown(KeyCode.P))
		{
			EventHub.Instance.RaiseEvent(new CHJMentalEvent(MentalState.평온));
		}
		syncTimer += Time.deltaTime;
		
		if (syncTimer > syncInterval&& transform.position.magnitude>0.01f)
		{
			syncTimer = 0;
		}
	}


	/// <summary>
	/// 위치정보 업로드 확인용 함수 없어지거나 다른거랑 통합할수도 있음
	/// </summary>
	/// <returns></returns>
	async Task UploadPositionAsync()
	{
		//PlayerData status = FirebaseManager.Instance.currentStatus;
		// if (status == null) return;
		// status.pos = transform.position;
		// await FirebaseManager.Instance.UploadData(status);
	}

	/// <summary>
	/// 움직임 구현.
	/// </summary>
	/// <param name="s"></param>
	//private void MovePlayer(MoveInputEvent s)
	//{
	//	if (!isMine) return;

	//	Vector3 dir = new Vector3(s.x, 0, s.y);
	//	if (dir.magnitude > 0.01f)
	//	{
	//		transform.Translate(dir * moveSpeed * Time.deltaTime);
	//		MonoStateMachine.ChangeState((int)CHJPlayerStateMachine.PlayerState.Walk);
	//	}
	//	else
	//	{
	//		MonoStateMachine.ChangeState((int)CHJPlayerStateMachine.PlayerState.Idle);
	//	}
	//}

	/// <summary>
	/// 멘탈 상태 변경 확인용함수
	/// </summary>
	/// <param name="m"></param>
	async void ChangeMentalState(CHJMentalEvent m)
	{
		// if (!isMine) return;
		// var status = FirebaseManager.Instance.currentStatus;
		// if (status == null) return;
		// //status.mentalState = m.mentalState.ToString();
		// await FirebaseManager.Instance.UploadData(status);
		//
		// var handler = GetComponent<CHJPlayerStatusHandler>();
		// if(handler != null)
		// {
		// 	handler.ApplyStatus(status);
		// }
	}

	/// <summary>
	/// 데미지 확인용 함수
	/// </summary>
	/// <param name="h"></param>
	async void TakeDamage(CHJTakeDamageEvent h)
	{
		if (!isMine) return;
		//var status = FirebaseManager.Instance.currentStatus;

		//await FirebaseManager.Instance.DownloadData<PlayerData>(FirebaseManager.Instance.userID, ApplyDamage);

		async void ApplyDamage(PlayerData status)
		{
			if (status == null) return;
			
			Debug.Log($"맞기전 {status.hp}");
			status.hp -= h.takedamage;
			Debug.Log($"맞은후 {status.hp}");
			//await FirebaseManager.Instance.UploadData(status);

			var handler = GetComponent<CHJPlayerStatusHandler>();
		
			handler.ApplyStatus(status);
		}
	}
}
